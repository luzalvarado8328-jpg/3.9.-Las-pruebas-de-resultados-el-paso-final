using UnityEngine;

public class Enemy : MonoBehaviour
{
    public float moveSpeed = 2f;
    public float patrolDistance = 3f;

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Vector3 startPosition;
    private float direction = 1f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        startPosition = transform.position;
    }

    void FixedUpdate()
    {
        // Movimiento de patrulla
        rb.linearVelocity = new Vector2(direction * moveSpeed, rb.linearVelocity.y);

        // Cambiar dirección al llegar al límite de patrulla
        float distanceFromStart = transform.position.x - startPosition.x;
        if (distanceFromStart > patrolDistance)
            direction = -1f;
        else if (distanceFromStart < -patrolDistance)
            direction = 1f;

        // Voltear sprite según dirección
        spriteRenderer.flipX = direction < 0f;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Player"))
        {
            // Si el jugador cae encima del enemigo, destruir enemigo
            float contactY = collision.GetContact(0).normal.y;
            if (contactY < -0.5f)
            {
                // El jugador viene desde arriba: destruir enemigo y rebotar
                Rigidbody2D playerRb = collision.collider.GetComponent<Rigidbody2D>();
                if (playerRb != null)
                    playerRb.linearVelocity = new Vector2(playerRb.linearVelocity.x, 8f);
                Destroy(gameObject);
            }
            else
            {
                // El jugador toca al enemigo por el lado: respawn
                PlayerController player = collision.collider.GetComponent<PlayerController>();
                if (player != null)
                    player.Respawn();
            }
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        // Destruir enemigo si le pega una bala
        if (collision.GetComponent<Bullet>() != null)
        {
            Destroy(collision.gameObject);
            Destroy(gameObject);
        }
    }
}
