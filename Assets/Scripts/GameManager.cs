using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Progreso del jugador")]
    public int totalCoins = 0;
    public bool level1Complete = false;
    public bool level2Complete = false;

    [Header("Regreso al menu")]
    public string escenaMenu = "MainMenu";

    void Update()
    {
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            string actual = SceneManager.GetActiveScene().name;
            if (actual != escenaMenu)
            {
                SaveToFirestore();
                SceneManager.LoadScene(escenaMenu);
            }
        }
    }

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
            return;
        }
    }

    void Start()
    {
        // Al iniciar, cargar datos desde Firestore
        LoadFromFirestore();
    }

    // El jugador recoge una moneda
    public void AddCoin()
    {
        totalCoins++;
        Debug.Log("Monedas totales: " + totalCoins);

        // Actualizar la UI
        if (UIManager.Instance != null)
            UIManager.Instance.UpdateCoins(totalCoins);

        // Guardar cada vez que se recoge una moneda
        SaveToFirestore();
    }

    // Marcar nivel como completado
    public void CompleteLevel(int levelNumber)
    {
        if (levelNumber == 1)
            level1Complete = true;
        else if (levelNumber == 2)
            level2Complete = true;

        Debug.Log("Nivel " + levelNumber + " completado!");

        // Actualizar UI
        if (UIManager.Instance != null)
            UIManager.Instance.UpdateLevels(level1Complete, level2Complete);

        // Guardar progreso
        SaveToFirestore();
    }

    // Cargar siguiente nivel
    public void LoadNextLevel()
    {
        string currentScene = SceneManager.GetActiveScene().name;
        if (currentScene == "Nivel1")
            SceneManager.LoadScene("Nivel2");
        else if (currentScene == "Nivel2")
            Debug.Log("Has completado todos los niveles!");
    }

    // Guardar datos en Firestore
    public void SaveToFirestore()
    {
        if (FirestoreManager.Instance != null)
        {
            FirestoreManager.Instance.SavePlayerData(totalCoins, level1Complete, level2Complete);
        }
    }

    // Cargar datos desde Firestore
    public void LoadFromFirestore()
    {
        if (FirestoreManager.Instance != null)
        {
            FirestoreManager.Instance.LoadPlayerData((coins, lvl1, lvl2) =>
            {
                totalCoins = coins;
                level1Complete = lvl1;
                level2Complete = lvl2;

                Debug.Log($"Datos cargados - Monedas: {totalCoins}, Nivel1: {level1Complete}, Nivel2: {level2Complete}");

                // Actualizar la UI con los datos cargados
                if (UIManager.Instance != null)
                {
                    UIManager.Instance.UpdateCoins(totalCoins);
                    UIManager.Instance.UpdateLevels(level1Complete, level2Complete);
                }
            });
        }
    }

    // Guardar al salir de la aplicacion
    void OnApplicationQuit()
    {
        SaveToFirestore();
    }

    // Guardar al pausar (util en moviles)
    void OnApplicationPause(bool pause)
    {
        if (pause)
            SaveToFirestore();
    }
}
