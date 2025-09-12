using UnityEngine;

/// <summary>
/// Керує здоров'ям, смертю та створенням ефектів смерті для гравця.
/// Вимагає наявності PlayerController та Collider2D на об'єкті.
/// </summary>
[RequireComponent(typeof(PlayerController), typeof(Collider2D))]
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
    private bool isDead = false;

    private void Awake()
    {
        // Кешуємо компоненти для продуктивності
        playerController = GetComponent<PlayerController>();
        playerCollider = GetComponent<Collider2D>();

        // Перевірка на випадок, якщо рендерери не призначені в інспекторі
        if (playerRenderers == null || playerRenderers.Length == 0)
        {
            Debug.LogWarning("У PlayerHealth не призначено жодного рендерера гравця.", this);
        }
    }

    void Update()
    {
        // Тестова кнопка для спавну частинок без смерті гравця.
        if (Input.GetKeyDown(KeyCode.K))
        {
            SpawnDeathParticles();
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

        // 3. Спавнимо частинки
        SpawnDeathParticles();

        // TODO: В майбутньому тут можна додати логіку для респавну або перезапуску рівня
        // Invoke(nameof(Respawn), 2f);
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

