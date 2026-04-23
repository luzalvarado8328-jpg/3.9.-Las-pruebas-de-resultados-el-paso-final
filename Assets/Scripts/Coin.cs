using UnityEngine;

public class Coin : MonoBehaviour
{
    public float rotationSpeed = 100f;
    public float bounceSpeed = 2f;
    public float bounceHeight = 0.3f;

    private Vector3 startPosition;

    void Start()
    {
        startPosition = transform.position;
    }

    void Update()
    {
        // Rotacion visual en el eje Y para simular giro de moneda
        transform.Rotate(0f, rotationSpeed * Time.deltaTime, 0f);

        // Movimiento de flotacion arriba/abajo
        float newY = startPosition.y + Mathf.Sin(Time.time * bounceSpeed) * bounceHeight;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            // Sumar moneda al GameManager
            if (GameManager.Instance != null)
                GameManager.Instance.AddCoin();

            Destroy(gameObject);
        }
    }
}
