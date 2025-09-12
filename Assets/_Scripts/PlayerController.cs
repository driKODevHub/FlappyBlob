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

    [Header("Параметри інерції в повітрі")]
    [Tooltip("Чи використовувати інерцію/опір повітря, коли немає вводу?")]
    [SerializeField] private bool useAirInertia = true;

    [Tooltip("Сила опору повітря (0 = миттєва зупинка, ~0.95 = плавне сповільнення).")]
    [Range(0f, 1f)]
    [SerializeField] private float airDrag = 0.95f;


    [Header("Ігрова логіка")]
    [Tooltip("Стандартне значення гравітації, яке ввімкнеться після першого стрибка.")]
    [SerializeField] private float defaultGravityScale = 3f;


    // --- Внутрішні змінні ---
    private float horizontalInput; // Зберігає значення вводу по горизонталі (-1, 0, 1)
    private bool jumpInput; // Зберігає інформацію про натискання стрибка
    private bool isGameActive = false; // Чи почалась активна фаза гри (після першого стрибка)

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

        // Починаємо гру з вимкненою гравітацією та "замороженим" станом
        rb.gravityScale = 0f;
        rb.bodyType = RigidbodyType2D.Kinematic; // Використовуємо новий, сучасний підхід
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
        // Не дозволяємо рухатись, поки гра не почалась (до першого стрибка).
        if (!isGameActive)
        {
            return;
        }

        // Якщо є активний ввід від гравця, встановлюємо швидкість напряму.
        if (Mathf.Abs(horizontalInput) > 0.01f)
        {
            rb.linearVelocity = new Vector2(horizontalInput * moveSpeed, rb.linearVelocity.y);
        }
        // Якщо вводу немає, і опція інерції увімкнена.
        else if (useAirInertia)
        {
            // Плавно сповільнюємо горизонтальну швидкість, створюючи ефект інерції/опору.
            // Це буде працювати як в повітрі, так і на землі.
            // Для майбутнього: можна додати перевірку isGrounded, щоб інерція працювала лише в повітрі.
            float slowedVelocityX = rb.linearVelocity.x * airDrag;
            rb.linearVelocity = new Vector2(slowedVelocityX, rb.linearVelocity.y);
        }
        // Якщо вводу немає, і інерція вимкнена, то зупиняємось миттєво.
        else
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        }
    }

    /// <summary>
    /// Виконує стрибок, якщо був відповідний ввід.
    /// </summary>
    private void HandleJump()
    {
        if (jumpInput)
        {
            // Якщо це перший стрибок, активуємо фізику та гравітацію.
            if (!isGameActive)
            {
                isGameActive = true;
                rb.bodyType = RigidbodyType2D.Dynamic; // "Розморожуємо" гравця, змінюючи тип тіла на динамічний
                rb.gravityScale = defaultGravityScale; // Вмикаємо гравітацію
            }

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




