using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem.UI;

public class MenuManager : MonoBehaviour
{
    [Header("Nombre de la escena del juego")]
    public string escenaJuego = "SampleScene";

    void Awake()
    {
        ConstruirMenu();
    }

    void ConstruirMenu()
    {
        // Camara para que la escena no se vea negra
        if (Camera.main == null)
        {
            var camObj = new GameObject("Main Camera");
            camObj.tag = "MainCamera";
            var cam = camObj.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.12f, 0.14f, 0.2f);
            cam.orthographic = true;
        }

        // EventSystem para que los clicks funcionen
        if (EventSystem.current == null)
        {
            var esObj = new GameObject("EventSystem");
            esObj.AddComponent<EventSystem>();
            esObj.AddComponent<InputSystemUIInputModule>();
        }

        // Canvas raiz
        var canvasObj = new GameObject("Canvas");
        var canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;
        canvasObj.AddComponent<GraphicRaycaster>();

        // Titulo
        CrearTexto(canvas.transform, "Mi Videojuego 2D", new Vector2(0, 250), new Vector2(900, 120), 72, FontStyle.Bold);

        // Boton JUGAR
        CrearBoton(canvas.transform, "BotonJugar", "JUGAR", new Vector2(0, 20),
            new Color(0.25f, 0.65f, 0.35f), JugarJuego);

        // Boton SALIR
        CrearBoton(canvas.transform, "BotonSalir", "SALIR", new Vector2(0, -100),
            new Color(0.75f, 0.25f, 0.25f), SalirJuego);

        // Pie de pagina
        CrearTexto(canvas.transform, "ESC en el juego para volver al menu", new Vector2(0, -400),
            new Vector2(900, 40), 24, FontStyle.Normal);
    }

    void CrearTexto(Transform padre, string contenido, Vector2 pos, Vector2 tam, int fontSize, FontStyle style)
    {
        var obj = new GameObject("Texto");
        obj.transform.SetParent(padre, false);
        var rt = obj.AddComponent<RectTransform>();
        rt.sizeDelta = tam;
        rt.anchoredPosition = pos;

        var txt = obj.AddComponent<Text>();
        txt.text = contenido;
        txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        txt.fontSize = fontSize;
        txt.fontStyle = style;
        txt.alignment = TextAnchor.MiddleCenter;
        txt.color = Color.white;
    }

    void CrearBoton(Transform padre, string nombre, string etiqueta, Vector2 pos, Color color, System.Action accion)
    {
        var obj = new GameObject(nombre);
        obj.transform.SetParent(padre, false);
        var rt = obj.AddComponent<RectTransform>();
        rt.sizeDelta = new Vector2(320, 90);
        rt.anchoredPosition = pos;

        var img = obj.AddComponent<Image>();
        img.color = color;

        var btn = obj.AddComponent<Button>();
        var colores = btn.colors;
        colores.normalColor = Color.white;
        colores.highlightedColor = new Color(0.9f, 0.9f, 0.9f);
        colores.pressedColor = new Color(0.7f, 0.7f, 0.7f);
        btn.colors = colores;
        btn.onClick.AddListener(() => accion());

        // Etiqueta hija
        var lblObj = new GameObject("Texto");
        lblObj.transform.SetParent(obj.transform, false);
        var lblRt = lblObj.AddComponent<RectTransform>();
        lblRt.anchorMin = Vector2.zero;
        lblRt.anchorMax = Vector2.one;
        lblRt.offsetMin = Vector2.zero;
        lblRt.offsetMax = Vector2.zero;
        var lbl = lblObj.AddComponent<Text>();
        lbl.text = etiqueta;
        lbl.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        lbl.fontSize = 40;
        lbl.fontStyle = FontStyle.Bold;
        lbl.alignment = TextAnchor.MiddleCenter;
        lbl.color = Color.white;
    }

    public void JugarJuego()
    {
        SceneManager.LoadScene(escenaJuego);
    }

    public void SalirJuego()
    {
        Debug.Log("Saliendo del juego...");

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
