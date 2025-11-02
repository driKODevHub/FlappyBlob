using MoreMountains.Feedbacks;
using UnityEngine;

/// <summary>
/// Керує рухом та основними діями гравця.
/// (ОНОВЛЕНО): Більше не керує візуальними ефектами, 
/// а лише викликає PlayerVisualController.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    // --- Singleton ---
    public static PlayerController Instance { get; private set; }

    // --- Компоненти та посилання ---
    [Header("Посилання на компоненти")]
    [SerializeField] private Rigidbody2D rb;

    // --- Налаштування руху ---
    [Header("Параметри руху")]
    [Tooltip("Швидкість горизонтального руху гравця.")]
    [SerializeField] private float moveSpeed = 7f;

    [Header("Параметри стрибка")]
    [Tooltip("Вертикальна швидкість, що миттєво надається гравцю при стрибку.")]
    [SerializeField] private float jumpVelocity = 15f;
    [Tooltip("Множник, на який ділиться швидкість при відпусканні кнопки стрибка (напр., 0.5 = швидкість вдвічі менша).")]
    [SerializeField] private float jumpCutMultiplier = 0.5f;

    [Header("Параметри інерції в повітрі")]
    [Tooltip("Чи використовувати інерцію/опір повітря, коли немає вводу?")]
    [SerializeField] private bool useAirInertia = true;
    [Tooltip("Сила опору повітря (0 = миттєва зупинка, ~0.95 = плавне сповільнення).")]
    [Range(0f, 1f)]
    [SerializeField] private float airDrag = 0.95f;

    [Tooltip("Стандартне значення гравітації, яке ввімкнеться після першого стрибка.")]
    [SerializeField] private float defaultGravityScale = 3f;

    // --- Внутрішні змінні ---
    private float horizontalInput;
    private bool jumpPressed;
    private bool jumpReleased;
    private bool isGameActive = false;

    // --- (ВИДАЛЕНО): Усі посилання на MMFeedbacks ---

    #region Unity Lifecycle Methods

    private void Awake()
    {
        // Налаштування Singleton патерну
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }

        if (rb == null)
        {
            rb = GetComponent<Rigidbody2D>();
        }
        ResetPlayer();
    }

    private void Update()
    {
        HandleInput();
    }

    private void FixedUpdate()
    {
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
        horizontalInput = 0f;
        jumpPressed = false;
        jumpReleased = false;
        this.enabled = true;
    }

    #endregion

    #region Private Methods

    private void HandleInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        if (Input.GetKeyDown(KeyCode.Space))
        {
            jumpPressed = true;
        }
        if (Input.GetKeyUp(KeyCode.Space))
        {
            jumpReleased = true;
        }
    }

    private void HandleMovement()
    {
        if (!isGameActive) return;

        if (Mathf.Abs(horizontalInput) > 0.01f)
        {
            rb.linearVelocity = new Vector2(horizontalInput * moveSpeed, rb.linearVelocity.y);
        }
        else if (useAirInertia)
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
            if (!isGameActive)
            {
                isGameActive = true;
                rb.bodyType = RigidbodyType2D.Dynamic;
                rb.gravityScale = defaultGravityScale;
            }

            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpVelocity);

            // --- (ОНОВЛЕНО): Делегуємо візуальний ефект ---
            if (PlayerVisualController.Instance != null)
            {
                PlayerVisualController.Instance.PlayJumpEffect();
            }
            // --- ---

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
