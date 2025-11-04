using UnityEngine;

/// <summary>
/// Обробляє всі фізичні зіткнення гравця.
/// (ОНОВЛЕНО): Тепер просто роздає команди контролерам, не спавнить префаби.
/// (ОНОВЛЕНО 2): Додано виклик ефекту сильного приземлення.
/// </summary>
[RequireComponent(typeof(PlayerHealth), typeof(PlayerController), typeof(Rigidbody2D))]
public class PlayerCollisionHandler : MonoBehaviour
{
    [Header("Налаштування Шару")]
    [SerializeField] private LayerMask obstacleLayer;
    [SerializeField] private LayerMask wallLayer;

    [Header("Налаштування клякс (Стіни)")]
    [Tooltip("Кулдаун на спавн клякс ТА ефектів удару.")]
    [SerializeField] private float wallSplatCooldown = 0.15f;

    // --- Внутрішні змінні ---
    private float lastWallSplatTime;

    private void Awake()
    {
        lastWallSplatTime = -wallSplatCooldown;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!PlayerController.Instance.enabled) return;

        int layer = collision.gameObject.layer;

        // --- 1. Перевірка на СМЕРТЕЛЬНІ перешкоди ---
        if (((1 << layer) & obstacleLayer) != 0)
        {
            PlayerHealth.Instance.Die();
            return;
        }

        // --- 2. Перевірка на ЗВИЧАЙНІ стіни ---
        if (((1 << layer) & wallLayer) != 0)
        {
            PlayerController.Instance.SetGroundedState(true);

            if (Time.time < lastWallSplatTime + wallSplatCooldown)
            {
                return;
            }
            lastWallSplatTime = Time.time;

            if (collision.contacts.Length > 0)
            {
                ContactPoint2D contact = collision.contacts[0];
                float impactMagnitude = collision.relativeVelocity.magnitude;

                // --- (ОНОВЛЕНО): Викликаємо ВСІ візуальні ефекти ---

                // 2а. Спавнимо кляксу ТА партикли (Візуал)
                PlayerVisualController.Instance.PlayWallHitEffects(contact.point, contact.normal);

                // 2b. Анімуємо скваш (Візуал)
                PlayerVisualController.Instance.PlayWallImpactEffect(contact.point, contact.normal, impactMagnitude);

                // 2c. (НОВЕ): Спавнимо партикли сильного приземлення (Візуал)
                PlayerVisualController.Instance.PlayHardLandingEffect(contact.point, contact.normal, impactMagnitude);

                // 2d. Застосовуємо відскок (Фізика)
                PlayerController.Instance.ApplyWallBounce(contact.normal);
            }
        }
    }
}
