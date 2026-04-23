using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class FirestoreManager : MonoBehaviour
{
    // =====================================================
    // IMPORTANTE: Cambia esto por tu Project ID de Firebase
    // Lo encuentras en: https://console.firebase.google.com/
    // =====================================================
    public string firebaseProjectId = "videojuego3d-3743d";

    private string baseUrl => $"https://firestore.googleapis.com/v1/projects/{firebaseProjectId}/databases/(default)/documents";

    public static FirestoreManager Instance { get; private set; }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Guardar datos del jugador en Firestore
    public void SavePlayerData(int totalCoins, bool level1Complete, bool level2Complete, Action<bool> onComplete = null)
    {
        StartCoroutine(SaveDataCoroutine(totalCoins, level1Complete, level2Complete, onComplete));
    }

    // Cargar datos del jugador desde Firestore
    public void LoadPlayerData(Action<int, bool, bool> onLoaded)
    {
        StartCoroutine(LoadDataCoroutine(onLoaded));
    }

    private IEnumerator SaveDataCoroutine(int totalCoins, bool level1Complete, bool level2Complete, Action<bool> onComplete)
    {
        string url = $"{baseUrl}/jugadores/jugador1";

        // Construir el JSON en formato Firestore
        string json = "{\"fields\":{" +
            "\"totalMonedas\":{\"integerValue\":\"" + totalCoins + "\"}," +
            "\"nivel1Completado\":{\"booleanValue\":" + level1Complete.ToString().ToLower() + "}," +
            "\"nivel2Completado\":{\"booleanValue\":" + level2Complete.ToString().ToLower() + "}" +
            "}}";

        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);

        // Usamos PATCH para crear o actualizar el documento
        UnityWebRequest request = new UnityWebRequest(url, "PATCH");
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("Datos guardados en Firestore correctamente");
            onComplete?.Invoke(true);
        }
        else
        {
            Debug.LogError("Error al guardar en Firestore: " + request.error);
            Debug.LogError("Respuesta: " + request.downloadHandler.text);
            onComplete?.Invoke(false);
        }
    }

    private IEnumerator LoadDataCoroutine(Action<int, bool, bool> onLoaded)
    {
        string url = $"{baseUrl}/jugadores/jugador1";

        UnityWebRequest request = UnityWebRequest.Get(url);
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            string response = request.downloadHandler.text;
            Debug.Log("Datos cargados de Firestore: " + response);

            // Parsear la respuesta JSON de Firestore
            int coins = ParseIntField(response, "totalMonedas");
            bool lvl1 = ParseBoolField(response, "nivel1Completado");
            bool lvl2 = ParseBoolField(response, "nivel2Completado");

            onLoaded?.Invoke(coins, lvl1, lvl2);
        }
        else
        {
            Debug.LogWarning("No se encontraron datos previos en Firestore (primer inicio): " + request.error);
            onLoaded?.Invoke(0, false, false);
        }
    }

    // Parsear un campo entero del JSON de Firestore
    private int ParseIntField(string json, string fieldName)
    {
        // Eliminar espacios y saltos de linea para parseo mas seguro
        string clean = json.Replace(" ", "").Replace("\n", "").Replace("\r", "").Replace("\t", "");

        string search = "\"" + fieldName + "\":{\"integerValue\":\"";
        int idx = clean.IndexOf(search);
        if (idx < 0) return 0;

        int start = idx + search.Length;
        int end = clean.IndexOf("\"", start);
        if (end < 0) return 0;

        string value = clean.Substring(start, end - start);
        Debug.Log("ParseInt " + fieldName + " = " + value);
        int result;
        return int.TryParse(value, out result) ? result : 0;
    }

    // Parsear un campo booleano del JSON de Firestore
    private bool ParseBoolField(string json, string fieldName)
    {
        string clean = json.Replace(" ", "").Replace("\n", "").Replace("\r", "").Replace("\t", "");

        string search = "\"" + fieldName + "\":{\"booleanValue\":";
        int idx = clean.IndexOf(search);
        if (idx < 0) return false;

        int start = idx + search.Length;
        int endComma = clean.IndexOf(",", start);
        int endBrace = clean.IndexOf("}", start);
        int end = (endComma >= 0 && endComma < endBrace) ? endComma : endBrace;
        if (end < 0) return false;

        string value = clean.Substring(start, end - start).Trim();
        Debug.Log("ParseBool " + fieldName + " = " + value);
        return value == "true";
    }
}
