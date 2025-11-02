using UnityEngine;

/// <summary>
/// Обробляє всі фізичні зіткнення гравця.
/// Викликає смерть при зіткненні з перешкодами (ObstacleLayer).
/// Спавнить клякси при терті об стіни (WallLayer).
/// </summary>
[RequireComponent(typeof(PlayerHealth), typeof(PlayerController), typeof(Rigidbody2D))]
public class PlayerCollisionHandler : MonoBehaviour
{
    [Header("Компоненти")]
    [Tooltip("Посилання на скрипт здоров'я гравця.")]
    [SerializeField] private PlayerHealth playerHealth;
    [Tooltip("Посилання на скрипт контролера гравця.")]
    [SerializeField] private PlayerController playerController;

    [Header("Налаштування Шару")]
    [Tooltip("Шар (або шари), на якому знаходяться смертельні перешкоди.")]
    [SerializeField] private LayerMask obstacleLayer;
    [Tooltip("Шар (або шари), на якому знаходяться звичайні стіни.")]
    [SerializeField] private LayerMask wallLayer;

    [Header("Налаштування клякс (Стіни)")]
    [Tooltip("Префаб клякси, що спавниться при ударі об стіну.")]
    [SerializeField] private GameObject splatPrefab;
    [Tooltip("Як часто (в секундах) можна спавнити кляксу при терті об стіну.")]
    [SerializeField] private float wallSplatCooldown = 0.2f;

    // --- Внутрішні змінні ---
    private float lastWallSplatTime;

    private void Awake()
    {
        // Кешуємо компоненти
        if (playerHealth == null) playerHealth = GetComponent<PlayerHealth>();
        if (playerController == null) playerController = GetComponent<PlayerController>();

        // Ініціалізуємо таймер так, щоб клякса могла з'явитись одразу
        lastWallSplatTime = -wallSplatCooldown;
    }

    /// <summary>
    /// Цей метод спрацьовує ОДИН РАЗ при вході в колізію.
    /// Використовуємо лише для миттєвих подій, як-от СМЕРТЬ.
    /// </summary>
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Якщо контролер вимкнено, значить ми мертві. Ігноруємо колізії.
        if (!playerController.enabled) return;

        int layer = collision.gameObject.layer;

        // Перевіряємо, чи входить шар об'єкта в нашу LayerMask для перешкод
        // (1 << layer) - це бітова маска для поточного шару
        if (((1 << layer) & obstacleLayer) != 0)
        {
            playerHealth.Die();
        }
    }

    /// <summary>
    /// Цей метод спрацьовує КОЖЕН ФІЗИЧНИЙ КАДР, поки триває колізія.
    /// Використовуємо для постійних ефектів, як-от ТЕРТЯ ОБ СТІНУ.
    /// </summary>
    private void OnCollisionStay2D(Collision2D collision)
    {
        // Якщо контролер вимкнено (ми мертві), не спавнити клякси
        if (!playerController.enabled) return;

        int layer = collision.gameObject.layer;

        // Перевіряємо, чи входить шар об'єкта в нашу LayerMask для стін
        if (((1 << layer) & wallLayer) != 0)
        {
            // --- Логіка кулдауну ---
            // Перевіряємо, чи пройшло достатньо часу з останнього спавну
            if (Time.time < lastWallSplatTime + wallSplatCooldown)
            {
                return; // Ще не час, виходимо
            }
            lastWallSplatTime = Time.time; // Скидаємо таймер

            // --- Логіка спавну клякси ---
            if (splatPrefab != null && collision.contacts.Length > 0)
            {
                // Беремо першу точку контакту
                ContactPoint2D contact = collision.contacts[0];

                // Створюємо кляксу в точці контакту
                GameObject splatInstance = Instantiate(splatPrefab, contact.point, Quaternion.identity);

                // Повідомляємо менеджер про нову кляксу
                if (SplatManager.Instance != null)
                {
                    SplatManager.Instance.AddSplat(splatInstance);
                }
                else
                {
                    Debug.LogWarning("SplatManager не знайдено, клякса не була зареєстрована.");
                }
            }
        }
    }
}

