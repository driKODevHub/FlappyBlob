using MoreMountains.Feedbacks;
using UnityEngine;
using System.Collections; // Додано для корутин

/// <summary>
/// Керує рухом та основними діями гравця.
/// (ОНОВЛЕНО): Тепер блокує керування рухом на короткий час після відскоку.
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
    [Tooltip("Сила відскоку від стін. 1 = ідеальне відбиття, 0.6 = 60% енергії повертається.")]
    [Range(0f, 1.5f)]
    [SerializeField] private float bounciness = 0.6f;

    // (НОВЕ):
    [Tooltip("Час (в секундах), на який блокується керування РУХОМ (вліво/вправо) після удару об стіну.")]
    [SerializeField] private float knockbackLockoutDuration = 0.2f;

    // --- Внутрішні змінні ---
    private float horizontalInput;
    private bool jumpPressed;
    private bool jumpReleased;
    private bool isGameActive = false;
    private Vector2 lastFixedUpdateVelocity;

    // (НОВЕ): Таймер, що відраховує час блокування
    private float knockbackLockoutTimer;

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
        // (НОВЕ): Оновлюємо таймер блокування
        if (knockbackLockoutTimer > 0)
        {
            knockbackLockoutTimer -= Time.fixedDeltaTime;
        }

        // (ВИПРАВЛЕНО): Використовуємо linearVelocity
        // Зберігаємо швидкість ДО того, як ми її змінимо в цьому кадрі
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
        knockbackLockoutTimer = 0f; // (НОВЕ): Скидаємо таймер
        this.enabled = true;
    }

    /// <summary>
    /// **ПУБЛІЧНИЙ МЕТОД**
    /// Застосовує фізичний відскок до Rigidbody.
    /// (ОНОВЛЕНО): Також активує таймер блокування руху.
    /// </summary>
    public void ApplyWallBounce(Vector2 contactNormal)
    {
        // Використовуємо Vector2.Reflect, щоб розрахувати новий напрямок
        Vector2 reflectedVelocity = Vector2.Reflect(lastFixedUpdateVelocity, contactNormal);

        // (ВИПРАВЛЕНО): Використовуємо linearVelocity
        // Застосовуємо нову швидкість з урахуванням "пружності"
        rb.linearVelocity = reflectedVelocity * bounciness;

        // (НОВЕ): Встановлюємо таймер блокування
        knockbackLockoutTimer = knockbackLockoutDuration;
    }

    #endregion

    #region Private Methods

    private void HandleInput()
    {
        // (ОНОВЛЕНО): Зчитуємо ввід, навіть якщо рух заблоковано,
        // щоб не "втратити" натискання стрибка.
        horizontalInput = Input.GetAxisRaw("Horizontal");
        if (Input.GetKeyDown(KeyCode.Space)) jumpPressed = true;
        if (Input.GetKeyUp(KeyCode.Space)) jumpReleased = true;
    }

    private void HandleMovement()
    {
        // (НОВЕ): Якщо ми в стані "кнокбеку", ігноруємо ввід
        // і не застосовуємо опір повітря (airDrag).
        if (knockbackLockoutTimer > 0)
        {
            return;
        }

        if (!isGameActive) return;

        // (Ці рядки вже використовували linearVelocity і були коректні)
        if (Mathf.Abs(horizontalInput) > 0.01f)
        {
            // (ВИПРАВЛЕНО)
            rb.linearVelocity = new Vector2(horizontalInput * moveSpeed, rb.linearVelocity.y);
        }
        else if (useAirInertIA)
        {
            float slowedVelocityX = rb.linearVelocity.x * airDrag;
            // (ВИПРАВЛЕНО)
            rb.linearVelocity = new Vector2(slowedVelocityX, rb.linearVelocity.y);
        }
        else
        {
            // (ВИПРАВЛЕНО)
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        }
    }

    private void HandleJump()
    {
        // Логіка стрибка залишається незалежною від блокування руху

        if (jumpPressed)
        {
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

