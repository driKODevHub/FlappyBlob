using MoreMountains.Feedbacks;
using UnityEngine;

/// <summary>
/// (НОВИЙ СКРИПТ) Керує ВСІМА візуальними ефектами гравця.
/// Відповідає за анімації Feel, ввімкнення/вимкнення рендерерів та партикли смерті.
/// Цей скрипт має висіти на об'єкті 'Visuals'.
/// </summary>
public class PlayerVisualController : MonoBehaviour
{
    // --- Singleton ---
    public static PlayerVisualController Instance { get; private set; }

    [Header("Посилання на візуал")]
    [Tooltip("Список спрайтів/рендерерів, які потрібно вимкнути при смерті. " +
             "Якщо залишити порожнім, скрипт спробує знайти їх автоматично.")] // (Оновлений Tooltip)
    [SerializeField] private Renderer[] playerRenderers;
    [Tooltip("Префаб системи частинок, що спавниться при смерті.")]
    [SerializeField] private GameObject deathParticlePrefab;

    [Header("Компоненти Feel (MMFeedbacks)")]
    [Tooltip("Компонент Squash & Stretch для анімації стрибка.")]
    [SerializeField] private MMSpringSquashAndStretch mMSpringSquashAndStretch;
    [Tooltip("Мінімальна та максимальна сила 'поштовху' пружини для Squash & Stretch.")]
    [SerializeField] private Vector2 minMaxSquashForce = new Vector2(5f, 10f);

    [Tooltip("Компонент Spring Rotation для анімації стрибка.")]
    [SerializeField] private MMSpringRotation mMSpringRotation;
    [Tooltip("Мінімальна та максимальна сила 'поштовху' пружини для Обертання.")]
    [SerializeField] private Vector2 minMaxSpringRotationForce = new Vector2(10f, 15f);

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

        // Авто-пошук компонентів Feel на цьому ж об'єкті (Visuals)
        if (mMSpringSquashAndStretch == null)
        {
            mMSpringSquashAndStretch = GetComponent<MMSpringSquashAndStretch>();
        }
        if (mMSpringRotation == null)
        {
            mMSpringRotation = GetComponent<MMSpringRotation>();
        }

        // (ОНОВЛЕНО): Авто-пошук рендерерів, якщо список порожній
        if (playerRenderers == null || playerRenderers.Length == 0)
        {
            // Шукаємо всі компоненти Renderer на цьому об'єкті та у всіх його дочірніх
            // Це включає SpriteRenderer, MeshRenderer, і т.д.
            playerRenderers = GetComponentsInChildren<Renderer>();

            if (playerRenderers.Length > 0)
            {
                Debug.Log($"PlayerVisualController: Автоматично знайдено {playerRenderers.Length} рендерерів.", this);
            }
            else
            {
                Debug.LogWarning("PlayerVisualController: Список 'playerRenderers' порожній і " +
                                 "авто-пошук не знайшов жодного компонента Renderer в дочірніх об'єктах.", this);
            }
        }
    }

    /// <summary>
    /// **ПУБЛІЧНИЙ МЕТОД**
    /// Програє візуальний ефект стрибка (Squash & Rotate).
    /// Викликається з PlayerController.
    /// </summary>
    public void PlayJumpEffect()
    {
        // 1. Ефект Squash & Stretch
        if (mMSpringSquashAndStretch != null)
        {
            float randomSquashForce = Random.Range(minMaxSquashForce.x, minMaxSquashForce.y);
            mMSpringSquashAndStretch.Bump(randomSquashForce);
        }

        // 2. Ефект Обертання
        if (mMSpringRotation != null)
        {
            int randNegativeOrPositive = Random.Range(0, 2) * 2 - 1; // -1 або 1
            float randomRotationForce = Random.Range(minMaxSpringRotationForce.x, minMaxSpringRotationForce.y);
            mMSpringRotation.Bump(new Vector3(0f, 0f, randomRotationForce * randNegativeOrPositive));
        }
    }

    /// <summary>
    /// **ПУБЛІЧНИЙ МЕТОД**
    /// Програє візуальний ефект смерті (ховає гравця, спавнить партикли).
    /// Викликається з PlayerHealth.
    /// </summary>
    /// <param name="deathPosition">Позиція, де потрібно створити партикли.</param>
    public void PlayDeathEffect(Vector3 deathPosition)
    {
        // 1. Вимикаємо всі візуальні частини гравця
        foreach (var renderer in playerRenderers)
        {
            if (renderer != null)
            {
                renderer.enabled = false;
            }
        }

        // 2. Спавнимо частинки
        if (deathParticlePrefab != null)
        {
            Instantiate(deathParticlePrefab, deathPosition, deathParticlePrefab.transform.rotation);
        }
        else
        {
            Debug.LogError("PlayerVisualController: Префаб частинок смерті не призначено!", this);
        }
    }

    /// <summary>
    /// **ПУБЛІЧНИЙ МЕТОД**
    /// Скидає візуал (вмикає рендерери).
    /// Викликається з PlayerHealth при респавні.
    /// </summary>
    public void ResetVisuals()
    {
        foreach (var renderer in playerRenderers)
        {
            if (renderer != null)
            {
                renderer.enabled = true;
            }
        }
    }
}

