using MoreMountains.Feedbacks;
using UnityEngine;
using System.Collections;

/// <summary>
/// Керує рухом та основними діями гравця.
/// (ОНОВЛЕНО): Блокує рух, коли гравець "на землі", але дозволяє безкінечні стрибки.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    // --- Singleton ---
    public static PlayerController Instance { get; private set; }

    [Header("Посилання на компоненти")]
    [SerializeField] private Rigidbody2D rb;

    [Header("Параметри руху")]
    [SerializeField] private float moveSpeed = 7f;

    [Header("Параметри стрибка")]
    [SerializeField] private float jumpVelocity = 15f;
    [SerializeField] private float jumpCutMultiplier = 0.5f;

    [Header("Параметри інерції в повітрі")]
    [SerializeField] private bool useAirInertIA = true;
    [Range(0f, 1f)]
    [SerializeField] private float airDrag = 0.95f;

    [SerializeField] private float defaultGravityScale = 3f;

    [Header("Налаштування Відскоку (Баунсу)")]
    [Range(0f, 1.5f)]
    [SerializeField] private float bounciness = 0.6f;
    [Tooltip("Час (в секундах), на який блокується керування РУХОМ (вліво/вправо) після удару об стіну.")]
    [SerializeField] private float knockbackLockoutDuration = 0.2f;

    // --- Внутрішні змінні ---
    private float horizontalInput;
    private bool jumpPressed;
    private bool jumpReleased;
    private bool isGameActive = false;
    private Vector2 lastFixedUpdateVelocity;
    private float knockbackLockoutTimer;

    // Стан, що показує, чи торкаємось ми землі/стіни
    private bool isGrounded = true;

    #region Unity Lifecycle Methods

    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        else Instance = this;

        if (rb == null) rb = GetComponent<Rigidbody2D>();
        ResetPlayer();
    }

    private void Update()
    {
        HandleInput();
    }

    private void FixedUpdate()
    {
        if (knockbackLockoutTimer > 0)
        {
            knockbackLockoutTimer -= Time.fixedDeltaTime;
        }

        lastFixedUpdateVelocity = rb.linearVelocity;

        HandleMovement();
        HandleJump();
    }

    #endregion

    #region Public Methods

    public void ResetPlayer()
    {
        isGameActive = false;
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0f;
        rb.linearVelocity = Vector2.zero;
        lastFixedUpdateVelocity = Vector2.zero;
        horizontalInput = 0f;
        jumpPressed = false;
        jumpReleased = false;
        knockbackLockoutTimer = 0f;
        isGrounded = true;
        this.enabled = true;
    }

    public void ApplyWallBounce(Vector2 contactNormal)
    {
        Vector2 reflectedVelocity = Vector2.Reflect(lastFixedUpdateVelocity, contactNormal);
        rb.linearVelocity = reflectedVelocity * bounciness;
        knockbackLockoutTimer = knockbackLockoutDuration;

        // Відскок означає, що ми в повітрі
        isGrounded = false;
    }

    /// <summary>
    /// **ПУБЛІЧНИЙ МЕТОД**
    /// Встановлює стан "на землі". Викликається з PlayerCollisionHandler.
    /// </summary>
    public void SetGroundedState(bool grounded)
    {
        isGrounded = grounded;
    }

    #endregion

    #region Private Methods

    private void HandleInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        if (Input.GetKeyDown(KeyCode.Space)) jumpPressed = true;
        if (Input.GetKeyUp(KeyCode.Space)) jumpReleased = true;
    }

    private void HandleMovement()
    {
        // (ОНОВЛЕНО): Якщо ми на землі, рух заборонено
        if (isGrounded)
        {
            // Зупиняємо будь-який горизонтальний рух, що виник від ковзання
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            return;
        }

        // (СТАРЕ): Блокування кнокбеку має вищий пріоритет
        if (knockbackLockoutTimer > 0)
        {
            return;
        }

        if (!isGameActive) return;

        if (Mathf.Abs(horizontalInput) > 0.01f)
        {
            rb.linearVelocity = new Vector2(horizontalInput * moveSpeed, rb.linearVelocity.y);
        }
        else if (useAirInertIA)
        {
            float slowedVelocityX = rb.linearVelocity.x * airDrag;
            rb.linearVelocity = new Vector2(slowedVelocityX, rb.linearVelocity.y);
        }
        else
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        }
    }

    private void HandleJump()
    {
        if (jumpPressed)
        {
            // (ВИДАЛЕНО): Перевірку на 'groundJumpOnly' видалено. Стрибати можна завжди.

            // Ми стрибнули, отже ми в повітрі
            isGrounded = false;

            if (!isGameActive)
            {
                isGameActive = true;
                rb.bodyType = RigidbodyType2D.Dynamic;
                rb.gravityScale = defaultGravityScale;
            }

            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpVelocity);

            if (PlayerVisualController.Instance != null)
            {
                PlayerVisualController.Instance.PlayJumpEffect();
            }

            jumpPressed = false;
        }

        if (jumpReleased)
        {
            if (rb.linearVelocity.y > 0)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * jumpCutMultiplier);
            }
            jumpReleased = false;
        }
    }

    #endregion
}

