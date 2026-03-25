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
    private Animator animator;
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

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
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
                spriteRenderer.color = Color.white;
            }
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
            spriteRenderer.color = new Color(1f, 0.7f, 0.85f, 1f);
        }

        // Disparo con tecla F o clic izquierdo
        if ((keyboard.fKey.wasPressedThisFrame || Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame) && Time.time >= nextFireTime)
        {
            Shoot();
            nextFireTime = Time.time + fireRate;
        }

        // Actualizar parámetros del Animator según el estado de movimiento
        animator.SetFloat("Speed", Mathf.Abs(moveInput));
        animator.SetBool("IsGrounded", isGrounded);
        animator.SetBool("IsJumping", !isGrounded);
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
