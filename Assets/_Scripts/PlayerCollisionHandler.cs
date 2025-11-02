using UnityEngine;

/// <summary>
/// Обробляє фізичні зіткнення гравця.
/// Викликає смерть при зіткненні з перешкодами (Obstacle)
/// та створює клякси при зіткненні зі стінами (Wall).
/// Має кулдаун на спавн клякс від стін, щоб уникнути спаму.
/// </summary>
[RequireComponent(typeof(Collider2D), typeof(PlayerHealth))]
public class PlayerCollisionHandler : MonoBehaviour
{
    [Header("Налаштування шарів")]
    [Tooltip("Шар, який вважається смертельною перешкодою.")]
    [SerializeField] private LayerMask obstacleLayer;
    [Tooltip("Шар, який вважається звичайною стіною (для клякс).")]
    [SerializeField] private LayerMask wallLayer;

    [Header("Посилання на ефекти")]
    [Tooltip("Префаб клякси, що спавниться при ударі об стіну. (Той самий, що і в ParticleCollisionHandler)")]
    [SerializeField] private GameObject splatPrefab;

    [Tooltip("Кулдаун в секундах між спавном клякс від удару об стіну.")]
    [SerializeField] private float wallSplatCooldown = 0.2f;

    // --- Кешовані компоненти ---
    private PlayerHealth playerHealth;

    // --- Змінні для кулдауну ---
    // Зберігає час (в секундах), коли була створена остання клякса від стіни.
    // Ініціалізуємо від'ємним значенням, щоб перша клякса спрацювала миттєво.
    private float lastWallSplatTime = -10f;

    private void Awake()
    {
        // Отримуємо посилання на PlayerHealth, що висить на цьому ж об'єкті
        playerHealth = GetComponent<PlayerHealth>();
    }

    /// <summary>
    /// Викликається автоматично Unity при зіткненні з іншим Collider2D.
    /// </summary>
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Отримуємо шар об'єкта, з яким зіткнулись
        int otherLayer = collision.gameObject.layer;

        // --- 1. Перевірка на ПЕРЕШКОДУ (Obstacle) ---
        // Використовуємо бітову маску, щоб перевірити, чи належить 'otherLayer' до 'obstacleLayer'
        if (obstacleLayer == (obstacleLayer | (1 << otherLayer)))
        {
            // Це перешкода. Викликаємо смерть.
            // PlayerHealth сам подбає про вимкнення гравця та спавн частинок смерті.
            playerHealth.Die();
        }
        // --- 2. Перевірка на СТІНУ (Wall) ---
        else if (wallLayer == (wallLayer | (1 << otherLayer)))
        {
            // --- ПЕРЕВІРКА КУЛДАУНУ ---
            // Перевіряємо, чи пройшло достатньо часу з останнього спавну клякси
            if (Time.time < lastWallSplatTime + wallSplatCooldown)
            {
                // Кулдаун ще не пройшов, нічого не робимо
                return;
            }

            // --- РЕЄСТРАЦІЯ КУЛДАУНУ ---
            // Якщо ми тут, значить спавн дозволено. Оновлюємо час.
            lastWallSplatTime = Time.time;

            // --- Спавн клякси (ТІЛЬКИ ОДНІЄЇ) ---
            // Щоб уникнути спавну десятків клякс при ударі об кут (де багато contact points),
            // ми беремо лише першу точку контакту.
            if (collision.contactCount > 0)
            {
                ContactPoint2D contact = collision.contacts[0];
                SpawnSplatAt(contact.point);
            }
        }
    }

    /// <summary>
    /// Створює екземпляр клякси в зазначеній позиції
    /// та реєструє її в SplatManager.
    /// </summary>
    /// <param name="position">Світова 2D-координата для спавну.</param>
    private void SpawnSplatAt(Vector2 position)
    {
        if (splatPrefab == null)
        {
            Debug.LogError("Splat Prefab не призначено в PlayerCollisionHandler!", this);
            return;
        }

        // Створюємо кляксу. 
        // Її скрипт SplatAppearance сам подбає про вибір спрайту та поворот.
        GameObject splatInstance = Instantiate(splatPrefab, position, Quaternion.identity);

        // Повідомляємо менеджер про нову кляксу для оптимізації
        if (SplatManager.Instance != null)
        {
            SplatManager.Instance.AddSplat(splatInstance);
        }
        else
        {
            Debug.LogError("SplatManager не знайдено на сцені! Клякса не буде відстежуватись.", this);
        }
    }
}

