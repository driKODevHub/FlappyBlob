using MoreMountains.Feedbacks;
using UnityEngine;

/// <summary>
/// Керує рухом та основними діями гравця.
/// Включає механіку стрибка з варіативною висотою через "зрізання" швидкості.
/// Цей скрипт вимагає наявності компонента Rigidbody2D на тому ж ігровому об'єкті.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    // --- Компоненти та посилання ---
    [Header("Посилання на компоненти")]
    [SerializeField] private Rigidbody2D rb; // Компонент для керування фізикою

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
    private float horizontalInput; // Зберігає значення вводу по горизонталі (-1, 0, 1)
    private bool jumpPressed; // Зберігає інформацію про натискання стрибка
    private bool jumpReleased; // Зберігає інформацію про відпускання кнопки стрибка
    private bool isGameActive = false; // Чи почалась активна фаза гри (після першого стрибка)

    [Header("Jump MMFeedbacks info")]
    [Tooltip("Компонент Squash & Stretch для анімації стрибка.")]
    [SerializeField] private MMSpringSquashAndStretch mMSpringSquashAndStretch;
    [Tooltip("Мінімальна та максимальна сила 'поштовху' пружини для Squash & Stretch.")]
    [SerializeField] private Vector2 minMaxSquashForce = new Vector2(5f, 10f); // ОНОВЛЕНО

    [Tooltip("Компонент Spring Rotation для анімації стрибка.")]
    [SerializeField] private MMSpringRotation mMSpringRotation;
    [Tooltip("Мінімальна та максимальна сила 'поштовху' пружини для Обертання.")]
    [SerializeField] Vector2 minMaxSpringRotationForce = new Vector2(10f, 15f); // ОНОВЛЕНО (додав тултіп і дефолт)


    #region Unity Lifecycle Methods

    /// <summary>
    /// Метод Awake викликається один раз при завантаженні скрипта.
    /// </summary>
    private void Awake()
    {
        if (rb == null)
        {
            rb = GetComponent<Rigidbody2D>();
        }
        // Скидаємо стан гравця при старті
        ResetPlayer();
    }

    /// <summary>
    /// Метод Update викликається кожного кадру для зчитування вводу.
    /// </summary>
    private void Update()
    {
        HandleInput();
    }

    /// <summary>
    /// Метод FixedUpdate викликається з фіксованою частотою для фізичних розрахунків.
    /// </summary>
    private void FixedUpdate()
    {
        HandleMovement();
        HandleJump();
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// **ПУБЛІЧНИЙ МЕТОД**
    /// Скидає стан контролера до початкового. Викликається з GameManager.
    /// </summary>
    public void ResetPlayer()
    {
        isGameActive = false;

        // Повертаємо гравця в "кінематичний" стан, де він не рухається і не має гравітації
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0f;
        rb.linearVelocity = Vector2.zero;

        // Скидаємо кешований ввід
        horizontalInput = 0f;
        jumpPressed = false;
        jumpReleased = false;

        // Вмикаємо сам скрипт (на випадок, якщо він був вимкнений)
        this.enabled = true;
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Обробляє та зберігає ввід від користувача.
    /// </summary>
    private void HandleInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");

        if (Input.GetKeyDown(KeyCode.Space))
        {
            // TODO: Для кращого ефекту, ти можеш викликати .Squash() тут
            // if (mMSpringSquashAndStretch != null) mMSpringSquashAndStretch.Squash(someSquashVector);
            jumpPressed = true;
        }

        if (Input.GetKeyUp(KeyCode.Space))
        {
            jumpReleased = true;
        }
    }

    /// <summary>
    /// Застосовує горизонтальний рух до Rigidbody.
    /// </summary>
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

    /// <summary>
    /// Керує логікою стрибка: пряме встановлення швидкості та "зрізання" при відпусканні.
    /// </summary>
    private void HandleJump()
    {
        // --- Початок стрибка ---
        if (jumpPressed)
        {
            if (!isGameActive)
            {
                isGameActive = true;
                rb.bodyType = RigidbodyType2D.Dynamic; // Вмикаємо повну фізику
                rb.gravityScale = defaultGravityScale;
            }

            // Прямо встановлюємо вертикальну швидкість
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpVelocity);

            // --- MMFeedbacks --- (ОНОВЛЕНА ЛОГІКА)

            // 1. Ефект Squash & Stretch
            if (mMSpringSquashAndStretch != null)
            {
                // Генеруємо випадкову СИЛУ (float)
                float randomSquashForce = Random.Range(minMaxSquashForce.x, minMaxSquashForce.y);
                // Викликаємо .Bump() з одним float значенням
                mMSpringSquashAndStretch.Bump(randomSquashForce);
            }

            // 2. Ефект Обертання
            if (mMSpringRotation != null)
            {
                int randNegativeOrPositive = Random.Range(0, 2) * 2 - 1; // -1 або 1
                float randomRotationForce = Random.Range(minMaxSpringRotationForce.x, minMaxSpringRotationForce.y);

                // Викликаємо .Bump() з Vector3 (тільки по осі Z)
                mMSpringRotation.Bump(new Vector3(0f, 0f, randomRotationForce * randNegativeOrPositive));
            }
            // --- ---

            jumpPressed = false;
        }

        // --- "Зрізання" стрибка при відпусканні кнопки ---
        if (jumpReleased)
        {
            // Перевіряємо, чи гравець ще летить вгору (швидкість по Y > 0)
            if (rb.linearVelocity.y > 0)
            {
                // Множимо вертикальну швидкість на наш коефіцієнт
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * jumpCutMultiplier);
            }
            jumpReleased = false;
        }
    }

    #endregion
}

