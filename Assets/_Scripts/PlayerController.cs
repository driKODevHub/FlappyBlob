using UnityEngine;

/// <summary>
/// Керує рухом та основними діями гравця.
/// Цей скрипт вимагає наявності компонента Rigidbody2D на тому ж ігровому об'єкті.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    // --- Компоненти та посилання ---
    // [SerializeField] дозволяє налаштовувати приватні змінні в інспекторі Unity,
    // що є хорошою практикою для інкапсуляції.
    [Header("Посилання на компоненти")]
    [SerializeField] private Rigidbody2D rb; // Компонент для керування фізикою

    // --- Налаштування руху ---
    [Header("Параметри руху")]
    [Tooltip("Швидкість горизонтального руху гравця.")]
    [SerializeField] private float moveSpeed = 7f;

    [Tooltip("Сила стрибка-хопу.")]
    [SerializeField] private float jumpForce = 12f;

    // --- Внутрішні змінні ---
    private float horizontalInput; // Зберігає значення вводу по горизонталі (-1, 0, 1)
    private bool jumpInput; // Зберігає інформацію про натискання стрибка

    #region Unity Lifecycle Methods

    /// <summary>
    /// Метод Awake викликається один раз при завантаженні скрипта.
    /// Ідеальне місце для ініціалізації компонентів.
    /// </summary>
    private void Awake()
    {
        // Намагаємося автоматично знайти компонент Rigidbody2D, якщо його не призначили вручну.
        if (rb == null)
        {
            rb = GetComponent<Rigidbody2D>();
        }
    }

    /// <summary>
    /// Метод Update викликається кожного кадру.
    /// Найкраще місце для зчитування вводу від гравця (клавіатура, миша, геймпад).
    /// </summary>
    private void Update()
    {
        HandleInput();
    }

    /// <summary>
    /// Метод FixedUpdate викликається з фіксованою частотою.
    /// Усі фізичні розрахунки та маніпуляції з Rigidbody слід робити тут для стабільності.
    /// </summary>
    private void FixedUpdate()
    {
        HandleMovement();
        HandleJump();
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Обробляє та зберігає ввід від користувача.
    /// Відокремлення логіки вводу робить код чистішим.
    /// </summary>
    private void HandleInput()
    {
        // Input.GetAxisRaw дає різкіші результати (-1, 0, 1) без згладжування,
        // що добре підходить для платформерів.
        horizontalInput = Input.GetAxisRaw("Horizontal");

        // Input.GetKeyDown спрацьовує тільки в момент натискання клавіші.
        if (Input.GetKeyDown(KeyCode.Space))
        {
            jumpInput = true;
        }
    }

    /// <summary>
    /// Застосовує горизонтальний рух до Rigidbody.
    /// </summary>
    private void HandleMovement()
    {
        // Ми змінюємо швидкість по осі X, але зберігаємо поточну швидкість по Y (гравітація, стрибок).
        // Це запобігає дивній поведінці фізики.
        rb.linearVelocity = new Vector2(horizontalInput * moveSpeed, rb.linearVelocity.y);
    }

    /// <summary>
    /// Виконує стрибок, якщо був відповідний ввід.
    /// </summary>
    private void HandleJump()
    {
        if (jumpInput)
        {
            // Спершу обнуляємо вертикальну швидкість. Це робить кожен стрибок-хоп
            // однаковим по висоті, незалежно від того, падав гравець чи піднімався.
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);

            // Додаємо миттєву силу вгору. ForceMode2D.Impulse ідеально підходить для стрибків.
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);

            // Скидаємо прапорець вводу, щоб стрибок не повторювався в кожному кадрі FixedUpdate.
            jumpInput = false;
        }
    }

    #endregion
}

