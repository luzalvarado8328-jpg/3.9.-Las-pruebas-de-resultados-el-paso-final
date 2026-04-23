using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelGoal : MonoBehaviour
{
    [Header("Numero de nivel (1 o 2)")]
    public int levelNumber = 1;

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            // Marcar nivel como completado
            if (GameManager.Instance != null)
            {
                GameManager.Instance.CompleteLevel(levelNumber);

                // Cargar siguiente nivel o mostrar mensaje de victoria
                if (levelNumber == 1)
                {
                    SceneManager.LoadScene("Nivel2");
                }
                else
                {
                    Debug.Log("Has completado todos los niveles!");
                }
            }
        }
    }
}
