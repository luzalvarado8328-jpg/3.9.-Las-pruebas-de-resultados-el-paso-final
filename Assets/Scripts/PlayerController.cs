using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Movimiento")]
    public float moveSpeed = 5f;
    public float jumpForce = 10f;

    [Header("Detección de suelo")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;

    [Header("Disparo")]
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float fireRate = 0.3f;
    private float nextFireTime = 0f;
    private float facingDirection = 1f;

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private bool isGrounded;
    private float moveInput;

    // Estado actual del personaje
    private enum PlayerState { Idle, Run, Jump }
    private PlayerState currentState = PlayerState.Idle;

    // Variables para animaciones por código
    private Vector3 originalScale;
    private float animTimer;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalScale = transform.localScale;
    }

    void Update()
    {
        // Detectar si está en el suelo
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        // Movimiento horizontal con teclas A/D o flechas
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null) return;

        moveInput = 0f;
        if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed)
            moveInput = -1f;
        else if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed)
            moveInput = 1f;

        rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);

        // Voltear el sprite según la dirección
        if (moveInput > 0)
        {
            spriteRenderer.flipX = false;
            facingDirection = 1f;
        }
        else if (moveInput < 0)
        {
            spriteRenderer.flipX = true;
            facingDirection = -1f;
        }

        // Salto con tecla Espacio o W o flecha arriba
        if ((keyboard.spaceKey.wasPressedThisFrame || keyboard.wKey.wasPressedThisFrame || keyboard.upArrowKey.wasPressedThisFrame) && isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        }

        // Disparo con tecla F o clic izquierdo
        if ((keyboard.fKey.wasPressedThisFrame || Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame) && Time.time >= nextFireTime)
        {
            Shoot();
            nextFireTime = Time.time + fireRate;
        }

        // Determinar estado actual
        if (!isGrounded)
            currentState = PlayerState.Jump;
        else if (Mathf.Abs(moveInput) > 0.1f)
            currentState = PlayerState.Run;
        else
            currentState = PlayerState.Idle;

        // Aplicar animación según estado
        animTimer += Time.deltaTime;
        ApplyStateAnimation();
    }

    void ApplyStateAnimation()
    {
        float scaleX = Mathf.Abs(originalScale.x);
        float scaleY = Mathf.Abs(originalScale.y);

        switch (currentState)
        {
            case PlayerState.Idle:
                // Respiración suave: escala sube y baja lentamente
                float breathe = 1f + Mathf.Sin(animTimer * 2f) * 0.03f;
                transform.localScale = new Vector3(scaleX, scaleY * breathe, originalScale.z);
                transform.localRotation = Quaternion.identity;
                spriteRenderer.color = Color.white;
                break;

            case PlayerState.Run:
                // Balanceo lateral + rebote al correr
                float bounce = 1f + Mathf.Abs(Mathf.Sin(animTimer * 10f)) * 0.05f;
                float tilt = Mathf.Sin(animTimer * 10f) * 3f;
                transform.localScale = new Vector3(scaleX, scaleY * bounce, originalScale.z);
                transform.localRotation = Quaternion.Euler(0, 0, tilt);
                spriteRenderer.color = Color.white;
                break;

            case PlayerState.Jump:
                // Estiramiento vertical al saltar
                float stretch = 1.08f;
                float squeeze = 0.92f;
                transform.localScale = new Vector3(scaleX * squeeze, scaleY * stretch, originalScale.z);
                transform.localRotation = Quaternion.identity;
                // Tinte ligeramente celeste al saltar
                spriteRenderer.color = new Color(0.9f, 0.95f, 1f, 1f);
                break;
        }
    }

    void Shoot()
    {
        if (bulletPrefab == null || firePoint == null) return;

        // Ajustar la posición del firePoint según la dirección
        Vector3 fpLocal = firePoint.localPosition;
        fpLocal.x = Mathf.Abs(fpLocal.x) * facingDirection;
        firePoint.localPosition = fpLocal;

        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
        bullet.GetComponent<Bullet>().SetDirection(facingDirection);
    }

    // Dibujar el radio de detección de suelo en el editor
    void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }
}
