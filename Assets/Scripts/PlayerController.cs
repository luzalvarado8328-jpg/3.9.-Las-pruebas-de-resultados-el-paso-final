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

    [Header("Mecánicas avanzadas")]
    public float coyoteTime = 0.12f;
    public float jumpBufferTime = 0.15f;
    public float dashSpeed = 15f;
    public float dashDuration = 0.2f;
    public float dashCooldown = 0.8f;

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private bool isGrounded;
    private float moveInput;

    // Coyote time y jump buffer
    private float coyoteTimer;
    private float jumpBufferTimer;

    // Dash
    private bool isDashing;
    private float dashTimer;
    private float dashCooldownTimer;

    // Respawn
    private Vector3 spawnPosition;

    // Estado actual del personaje
    private enum PlayerState { Idle, Run, Jump, Dash }
    private PlayerState currentState = PlayerState.Idle;

    // Variables para animaciones por código
    private Vector3 originalScale;
    private float animTimer;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalScale = transform.localScale;
        spawnPosition = transform.position;
    }

    void Update()
    {
        if (isDashing)
        {
            dashTimer -= Time.deltaTime;
            if (dashTimer <= 0f)
            {
                isDashing = false;
                rb.gravityScale = 3f;
            }
            animTimer += Time.deltaTime;
            ApplyStateAnimation();
            return;
        }

        // Detectar si está en el suelo
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        // Coyote time: permite saltar poco después de dejar el suelo
        if (isGrounded)
            coyoteTimer = coyoteTime;
        else
            coyoteTimer -= Time.deltaTime;

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

        // Jump buffer: registra intención de salto
        if (keyboard.spaceKey.wasPressedThisFrame || keyboard.wKey.wasPressedThisFrame || keyboard.upArrowKey.wasPressedThisFrame)
            jumpBufferTimer = jumpBufferTime;
        else
            jumpBufferTimer -= Time.deltaTime;

        // Salto con coyote time + jump buffer
        if (jumpBufferTimer > 0f && coyoteTimer > 0f)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            jumpBufferTimer = 0f;
            coyoteTimer = 0f;
        }

        // Salto variable: soltar la tecla corta el salto
        if ((keyboard.spaceKey.wasReleasedThisFrame || keyboard.wKey.wasReleasedThisFrame || keyboard.upArrowKey.wasReleasedThisFrame)
            && rb.linearVelocity.y > 0f)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * 0.5f);
        }

        // Dash con Shift
        dashCooldownTimer -= Time.deltaTime;
        if (keyboard.leftShiftKey.wasPressedThisFrame && dashCooldownTimer <= 0f)
        {
            isDashing = true;
            dashTimer = dashDuration;
            dashCooldownTimer = dashCooldown;
            rb.gravityScale = 0f;
            rb.linearVelocity = new Vector2(facingDirection * dashSpeed, 0f);
        }

        // Disparo con tecla F o clic izquierdo
        if ((keyboard.fKey.wasPressedThisFrame || Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame) && Time.time >= nextFireTime)
        {
            Shoot();
            nextFireTime = Time.time + fireRate;
        }

        // Determinar estado actual
        if (isDashing)
            currentState = PlayerState.Dash;
        else if (!isGrounded)
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

            case PlayerState.Dash:
                // Aplastamiento horizontal al hacer dash
                transform.localScale = new Vector3(scaleX * 1.15f, scaleY * 0.85f, originalScale.z);
                transform.localRotation = Quaternion.identity;
                // Tinte rosado durante el dash
                spriteRenderer.color = new Color(1f, 0.7f, 0.85f, 1f);
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

    public void Respawn()
    {
        transform.position = spawnPosition;
        rb.linearVelocity = Vector2.zero;
        isDashing = false;
        rb.gravityScale = 3f;
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
