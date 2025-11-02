using UnityEngine;

/// <summary>
/// Обробляє всі фізичні зіткнення гравця.
/// (ОНОВЛЕНО): Тепер делегує відскок PlayerController, а візуал - PlayerVisualController.
/// </summary>
[RequireComponent(typeof(PlayerHealth), typeof(PlayerController), typeof(Rigidbody2D))]
public class PlayerCollisionHandler : MonoBehaviour
{
    public static PlayerCollisionHandler Instance { get; private set; }

    [Header("Налаштування Шару")]
    [SerializeField] private LayerMask obstacleLayer;
    [SerializeField] private LayerMask wallLayer;

    [Header("Налаштування клякс (Стіни)")]
    [SerializeField] private GameObject splatPrefab;
    [SerializeField] private float wallSplatCooldown = 0.2f;

    private float lastWallSplatTime;

    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        else Instance = this;

        lastWallSplatTime = -wallSplatCooldown;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // (ОНОВЛЕНО): Перевіряємо через Instance
        if (PlayerController.Instance == null || !PlayerController.Instance.enabled) return;

        int layer = collision.gameObject.layer;

        // --- 1. Перевірка на смертельні перешкоди ---
        if (((1 << layer) & obstacleLayer) != 0)
        {
            if (PlayerHealth.Instance != null) PlayerHealth.Instance.Die();
            return;
        }

        // --- 2. Перевірка на стіни ---
        if (((1 << layer) & wallLayer) != 0)
        {
            if (Time.time < lastWallSplatTime + wallSplatCooldown) return;
            lastWallSplatTime = Time.time;

            if (collision.contacts.Length > 0)
            {
                ContactPoint2D contact = collision.contacts[0];

                // (ОНОВЛЕНО): Отримуємо силу удару
                float impactMagnitude = collision.relativeVelocity.magnitude;

                // --- Делегуємо завдання різним контролерам ---

                // 1. ФІЗИКА: Кажемо контролеру гравця відскочити
                if (PlayerController.Instance != null)
                {
                    PlayerController.Instance.ApplyWallBounce(contact.normal);
                }

                // 2. ВІЗУАЛ: Кажемо візуальному контролеру програти скваш
                if (PlayerVisualController.Instance != null)
                {
                    PlayerVisualController.Instance.PlayWallImpactEffect(contact.point, contact.normal, impactMagnitude);
                }

                // 3. ЕФЕКТИ: Спавнимо кляксу
                if (splatPrefab != null)
                {
                    GameObject splatInstance = Instantiate(splatPrefab, contact.point, Quaternion.identity);
                    if (SplatManager.Instance != null) SplatManager.Instance.AddSplat(splatInstance);
                    else Debug.LogWarning("SplatManager не знайдено, клякса не була зареєстрована.");
                }
            }
        }
    }
}

