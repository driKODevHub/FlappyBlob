using UnityEngine;

/// <summary>
/// Керує здоров'ям, смертю та створенням ефектів смерті для гравця.
/// (ОНОВЛЕНО): Тепер також викликає 'PlayerCosmeticRandomizer' при ресеті.
/// (ОНОВЛЕНО 2): Додано ефект "pop-up" при ресеті.
/// </summary>
[RequireComponent(typeof(PlayerController), typeof(Collider2D), typeof(Rigidbody2D))]
public class PlayerHealth : MonoBehaviour
{
    // --- Singleton ---
    public static PlayerHealth Instance { get; private set; }

    // --- Посилання на компоненти ---
    private PlayerController playerController;
    private Collider2D playerCollider;
    private Rigidbody2D rb;
    private bool isDead = false;

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

        // Кешуємо компоненти
        playerController = GetComponent<PlayerController>();
        playerCollider = GetComponent<Collider2D>();
        rb = GetComponent<Rigidbody2D>();
    }

    /// <summary>
    /// Основна логіка смерті гравця.
    /// </summary>
    public void Die()
    {
        if (isDead) return;
        isDead = true;

        // 1. (ОНОВЛЕНО): Вимикаємо візуал через PlayerVisualController
        if (PlayerVisualController.Instance != null)
        {
            PlayerVisualController.Instance.PlayDeathEffect(transform.position);
        }

        // 2. Вимикаємо керування та фізичну колізію
        if (playerController != null) playerController.enabled = false;
        if (playerCollider != null) playerCollider.enabled = false;

        // 3. Зупиняємо будь-який рух
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.linearVelocity = Vector2.zero;

        // 4. Повідомляємо GameManager
        if (GameManager.Instance != null)
        {
            GameManager.Instance.StartRespawnProcess();
        }
        else
        {
            Debug.LogError("GameManager не знайдено! Гравець не респавниться.", this);
        }
    }

    /// <summary>
    /// **ПУБЛІЧНИЙ МЕТОД**
    /// Скидає здоров'я та візуал гравця. Викликається з GameManager.
    /// </summary>
    public void ResetPlayer(Vector3 spawnPosition)
    {
        // 1. Переміщуємо гравця
        transform.position = spawnPosition;

        // 2. (ОНОВЛЕНО): Вмикаємо візуал через PlayerVisualController
        if (PlayerVisualController.Instance != null)
        {
            PlayerVisualController.Instance.ResetVisuals();
        }

        // 3. (НОВЕ!) Запускаємо рандомізацію кастомізації
        if (PlayerCosmeticRandomizer.Instance != null)
        {
            PlayerCosmeticRandomizer.Instance.RandomizeCosmetics();
        }

        // 4. (НОВЕ!) Запускаємо ефект появи
        if (PlayerVisualController.Instance != null)
        {
            PlayerVisualController.Instance.PlaySpawnEffect();
        }

        // 5. Вмикаємо колайдер (раніше був 4)
        if (playerCollider != null) playerCollider.enabled = true;

        // 6. Скидаємо прапорець смерті (раніше був 5)
        isDead = false;
    }
}
