using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("Textos UI")]
    public Text coinsText;
    public Text levelStatusText;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        UpdateCoins(0);
        UpdateLevels(false, false);

        // Si el GameManager ya tiene datos cargados, actualizar
        if (GameManager.Instance != null)
        {
            UpdateCoins(GameManager.Instance.totalCoins);
            UpdateLevels(GameManager.Instance.level1Complete, GameManager.Instance.level2Complete);
        }
    }

    public void UpdateCoins(int coins)
    {
        if (coinsText != null)
            coinsText.text = "Monedas: " + coins;
    }

    public void UpdateLevels(bool level1, bool level2)
    {
        if (levelStatusText != null)
        {
            string status = "Nivel 1: " + (level1 ? "Completado" : "Pendiente") +
                           "\nNivel 2: " + (level2 ? "Completado" : "Pendiente");
            levelStatusText.text = status;
        }
    }
}
