using UnityEngine;
using System.Collections;

/// <summary>
/// (НОВИЙ) Керує завантаженням, вивантаженням та перемиканням префабів рівнів.
/// Він НЕ знає про логіку гри (очки, виходи), а лише керує префабами.
/// (ОНОВЛЕНО): Тепер очищує клякси *перед* завантаженням нового рівня.
/// </summary>
public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }

    [Header("Налаштування Рівнів")]
    [Tooltip("Масив усіх префабів рівнів, які будуть завантажуватись по порядку.")]
    [SerializeField] private GameObject[] levelPrefabs;

    [Tooltip("Порожній об'єкт на сцені, куди будуть спавнитись рівні (для чистоти ієрархії).")]
    [SerializeField] private Transform levelContainer;

    [Header("Налаштування Завантаження")]
    [Tooltip("Затримка в секундах перед завантаженням наступного рівня (для ефекту).")]
    [SerializeField] private float loadNextLevelDelay = 1.0f;

    // --- Внутрішні змінні ---
    private int currentLevelIndex = -1;
    private GameObject currentLevelInstance;
    private LevelData currentLevelData;

    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        else Instance = this;
    }

    private void Start()
    {
        if (levelPrefabs == null || levelPrefabs.Length == 0)
        {
            Debug.LogError("LevelManager: Список префабів рівнів порожній!", this);
            return;
        }

        if (levelContainer == null)
        {
            Debug.LogError("LevelManager: 'Level Container' не призначено!", this);
            levelContainer = this.transform; // Фолбек
        }

        // Завантажуємо перший рівень при старті
        LoadLevel(0);
    }

    /// <summary>
    /// **ПУБЛІЧНИЙ МЕТОД**
    /// Повністю завантажує рівень за його індексом.
    /// (ОНОВЛЕНО): Більше не очищує клякси, це робиться в 'LoadNextLevelCoroutine'.
    /// </summary>
    public void LoadLevel(int levelIndex)
    {
        if (levelIndex < 0 || levelIndex >= levelPrefabs.Length)
        {
            Debug.LogWarning($"LevelManager: Спроба завантажити неіснуючий рівень (індекс {levelIndex}). Можливо, гра пройдена.");
            // Тут можна додати логіку "Кінець гри"
            return;
        }

        // 1. Очищуємо попередній рівень (якщо він є)
        if (currentLevelInstance != null)
        {
            Destroy(currentLevelInstance);
            currentLevelInstance = null;
        }

        // (ВИДАЛЕНО): Очищення клякс переїхало в 'LoadNextLevelCoroutine'

        // 3. Оновлюємо індекс
        currentLevelIndex = levelIndex;

        // 4. Створюємо новий рівень
        currentLevelInstance = Instantiate(levelPrefabs[currentLevelIndex], levelContainer);
        currentLevelData = currentLevelInstance.GetComponent<LevelData>();

        if (currentLevelData == null)
        {
            Debug.LogError($"LevelManager: Префаб рівня '{levelPrefabs[currentLevelIndex].name}' не має компонента LevelData!", this);
            return;
        }

        // 5. Повідомляємо GameManager про новий рівень та його дані
        if (GameManager.Instance != null)
        {
            GameManager.Instance.InitializeNewLevel(currentLevelData);
        }
        else
        {
            Debug.LogError("LevelManager: Не можу знайти GameManager для ініціалізації рівня!");
        }

        // 6. Ресетимо гравця і ставимо його на точку спавну
        // (PlayerController ресетить стан, PlayerHealth ресетить візуал + позицію)
        if (PlayerController.Instance != null) PlayerController.Instance.ResetPlayer();
        if (PlayerHealth.Instance != null) PlayerHealth.Instance.ResetPlayer(currentLevelData.GetSpawnPointPosition());
    }

    /// <summary>
    /// **ПУБЛІЧНИЙ МЕТОД**
    /// Запускає процес завантаження наступного рівня.
    /// </summary>
    public void LoadNextLevel()
    {
        StartCoroutine(LoadNextLevelCoroutine());
    }

    /// <summary>
    /// (ОНОВЛЕНО): Тепер очищує клякси і чекає, ПЕРШ НІЖ завантажити рівень.
    /// </summary>
    private IEnumerator LoadNextLevelCoroutine()
    {
        // TODO: Тут можна додати ефект зникнення екрану

        // Вимикаємо гравця на час переходу
        if (PlayerController.Instance != null) PlayerController.Instance.enabled = false;

        // 1. Чекаємо початкову затримку (для ефекту "завершення" рівня)
        yield return new WaitForSeconds(loadNextLevelDelay);

        // 2. (НОВЕ) Запускаємо очищення клякс і чекаємо, поки воно завершиться
        if (SplatManager.Instance != null)
        {
            yield return StartCoroutine(SplatManager.Instance.ClearAllSplatsCoroutine());
        }
        else
        {
            Debug.LogError("LevelManager: Не можу знайти SplatManager для очищення клякс!");
        }

        // 3. Завантажуємо наступний рівень
        LoadLevel(currentLevelIndex + 1);
    }

    /// <summary>
    /// **ПУБЛІЧНИЙ МЕТОД**
    /// Повертає дані поточного завантаженого рівня.
    /// </summary>
    public LevelData GetCurrentLevelData()
    {
        return currentLevelData;
    }
}