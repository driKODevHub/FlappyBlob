using UnityEngine;

/// <summary>
/// Обробляє всі фізичні зіткнення гравця.
/// (ОНОВЛЕНО): Використовує Singleton-и для доступу до PlayerHealth та PlayerController.
/// </summary>
[RequireComponent(typeof(PlayerHealth), typeof(PlayerController), typeof(Rigidbody2D))]
public class PlayerCollisionHandler : MonoBehaviour
{
    // --- Singleton ---
    public static PlayerCollisionHandler Instance { get; private set; }

    // --- (ВИДАЛЕНО): Посилання на PlayerHealth та PlayerController ---

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

    private float lastWallSplatTime;

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

        // (ВИДАЛЕНО): GetComponent для health/controller

        lastWallSplatTime = -wallSplatCooldown;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // (ОНОВЛЕНО): Доступ через Singleton
        if (PlayerController.Instance == null || !PlayerController.Instance.enabled) return;

        int layer = collision.gameObject.layer;

        if (((1 << layer) & obstacleLayer) != 0)
        {
            // (ОНОВЛЕНО): Доступ через Singleton
            if (PlayerHealth.Instance != null)
            {
                PlayerHealth.Instance.Die();
            }
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        // (ОНОВЛЕНО): Доступ через Singleton
        if (PlayerController.Instance == null || !PlayerController.Instance.enabled) return;

        int layer = collision.gameObject.layer;

        if (((1 << layer) & wallLayer) != 0)
        {
            if (Time.time < lastWallSplatTime + wallSplatCooldown)
            {
                return;
            }
            lastWallSplatTime = Time.time;

            if (splatPrefab != null && collision.contacts.Length > 0)
            {
                ContactPoint2D contact = collision.contacts[0];
                GameObject splatInstance = Instantiate(splatPrefab, contact.point, Quaternion.identity);

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
