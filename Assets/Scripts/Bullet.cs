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
