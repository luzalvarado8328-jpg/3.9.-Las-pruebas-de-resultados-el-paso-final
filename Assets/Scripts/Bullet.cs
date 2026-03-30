using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed = 12f;
    public float lifeTime = 3f;

    private Rigidbody2D rb;
    private float direction;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        // Desactivar gravedad para que la bala no caiga
        rb.gravityScale = 0f;

        // Agregar efecto visual de trayectoria (estela)
        TrailRenderer trail = gameObject.AddComponent<TrailRenderer>();
        trail.time = 0.3f;
        trail.startWidth = 0.15f;
        trail.endWidth = 0.0f;
        trail.material = new Material(Shader.Find("Sprites/Default"));
        trail.startColor = new Color(1f, 0.85f, 0.2f, 1f);   // Amarillo dorado
        trail.endColor = new Color(1f, 0.4f, 0.1f, 0f);       // Naranja que se desvanece
        trail.sortingOrder = 1;

        Destroy(gameObject, lifeTime);
    }

    public void SetDirection(float dir)
    {
        direction = dir;
        // Voltear el sprite si va a la izquierda
        if (dir < 0)
            transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
    }

    void FixedUpdate()
    {
        // Trayectoria lineal sin componente vertical
        rb.linearVelocity = new Vector2(direction * speed, 0f);
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        // Destruir la bala al chocar con algo que no sea el jugador
        if (!collision.CompareTag("Player"))
        {
            Destroy(gameObject);
        }
    }
}
