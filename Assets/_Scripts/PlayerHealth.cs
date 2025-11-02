using UnityEngine;

/// <summary>
/// Керує здоров'ям, смертю та створенням ефектів смерті для гравця.
/// Вимагає наявності PlayerController та Collider2D на об'єкті.
/// </summary>
[RequireComponent(typeof(PlayerController), typeof(Collider2D), typeof(Rigidbody2D))]
public class PlayerHealth : MonoBehaviour
{
    [Header("Посилання")]
    [Tooltip("Список спрайтів/рендерерів, які потрібно вимкнути при смерті.")]
    [SerializeField] private Renderer[] playerRenderers;
    [Tooltip("Префаб системи частинок, що спавниться при смерті.")]
    [SerializeField] private GameObject deathParticlePrefab;

    // --- Посилання на компоненти ---
    private PlayerController playerController;
    private Collider2D playerCollider;
    private Rigidbody2D rb;
    private bool isDead = false;

    private void Awake()
    {
        // Кешуємо компоненти для продуктивності
        playerController = GetComponent<PlayerController>();
        playerCollider = GetComponent<Collider2D>();
        rb = GetComponent<Rigidbody2D>();

        // Перевірка на випадок, якщо рендерери не призначені в інспекторі
        if (playerRenderers == null || playerRenderers.Length == 0)
        {
            Debug.LogWarning("У PlayerHealth не призначено жодного рендерера гравця.", this);
        }
    }

    /// <summary>
    /// Основна логіка смерті гравця.
    /// </summary>
    public void Die()
    {
        if (isDead) return; // Запобігаємо повторному виклику
        isDead = true;

        // 1. Вимикаємо всі візуальні частини гравця
        foreach (var renderer in playerRenderers)
        {
            if (renderer != null)
            {
                renderer.enabled = false;
            }
        }

        // 2. Вимикаємо керування та фізичну колізію
        if (playerController != null) playerController.enabled = false;
        if (playerCollider != null) playerCollider.enabled = false;

        // 3. Зупиняємо будь-який рух (ВИПРАВЛЕНО)
        rb.bodyType = RigidbodyType2D.Kinematic; // Робимо тіло кінематичним, щоб воно "зависло"
        rb.linearVelocity = Vector2.zero; // Використовуємо .linearVelocity

        // 4. Спавнимо частинки
        SpawnDeathParticles();

        // 5. Повідомляємо GameManager, щоб він почав респавн
        if (GameManager.Instance != null)
        {
            GameManager.Instance.StartRespawnProcess();
        }
        else
        {
            Debug.LogError("GameManager не знайдено на сцені! Гравець не респавниться.", this);
        }
    }

    /// <summary>
    /// **ПУБЛІЧНИЙ МЕТОД**
    /// Скидає здоров'я та візуал гравця. Викликається з GameManager.
    /// </summary>
    /// <param name="spawnPosition">Позиція, куди треба перемістити гравця.</param>
    public void ResetPlayer(Vector3 spawnPosition)
    {
        // 1. Переміщуємо гравця на точку спавну
        transform.position = spawnPosition;

        // 2. Вмикаємо візуал
        foreach (var renderer in playerRenderers)
        {
            if (renderer != null)
            {
                renderer.enabled = true;
            }
        }

        // 3. Вмикаємо колайдер
        if (playerCollider != null) playerCollider.enabled = true;

        // 4. Скидаємо прапорець смерті
        isDead = false;
    }

    /// <summary>
    /// Створює екземпляр префабу частинок смерті в позиції гравця.
    /// </summary>
    private void SpawnDeathParticles()
    {
        if (deathParticlePrefab != null)
        {
            // Створюємо партикли в поточній позиції гравця зі стандартним поворотом префаба
            Instantiate(deathParticlePrefab, transform.position, deathParticlePrefab.transform.rotation);
        }
        else
        {
            Debug.LogError("Префаб частинок смерті (Death Particle Prefab) не призначено в інспекторі!", this);
        }
    }
}

