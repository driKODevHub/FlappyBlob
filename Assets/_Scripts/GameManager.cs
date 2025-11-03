using UnityEngine;

/// <summary>
/// Керує загальним станом гри та логікою поточного рівня.
/// (ОНОВЛЕНО): Тепер керує логікою 'Unlock Points', респавном
/// та використовує дані з 'LevelManager' та 'LevelData'.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Налаштування респавну")]
    [Tooltip("Час в секундах, через який гравець респавниться після смерті.")]
    [SerializeField] private float respawnDelay = 2.0f;

    // --- (ВИДАЛЕНО): 'spawnPoint' переїхав у LevelData ---
    // private Vector3 spawnPoint; 

    // --- (НОВЕ): Стан поточного рівня ---
    private LevelData currentLevelData;
    private int unlockPointsNeeded;
    private int unlockPointsCollected;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    // --- (ВИДАЛЕНО): Start() більше не потрібен, ініціалізація йде з LevelManager ---

    /// <summary>
    /// **(НОВИЙ) ПУБЛІЧНИЙ МЕТОД**
    /// Ініціалізує стан гри для нового рівня.
    /// Викликається з LevelManager, коли рівень завантажено.
    /// </summary>
    public void InitializeNewLevel(LevelData levelData)
    {
        currentLevelData = levelData;

        if (currentLevelData == null)
        {
            Debug.LogError("GameManager: Отримано null LevelData! Логіка рівня не працюватиме.", this);
            return;
        }

        // 1. Отримуємо дані про рівень
        unlockPointsNeeded = currentLevelData.GetUnlockPointsNeeded();
        unlockPointsCollected = 0;

        // 2. Деактивуємо вихід з рівня
        currentLevelData.GetLevelExit()?.SetActivation(false);
    }

    /// <summary>
    /// **(НОВИЙ) ПУБЛІЧНИЙ МЕТОД**
    /// Викликається з 'UnlockPoint', коли гравець його підбирає.
    /// </summary>
    public void OnUnlockPointCollected()
    {
        if (currentLevelData == null) return; // Рівень ще не ініціалізовано

        unlockPointsCollected++;

        // TODO: Оновити UI (наприклад, "Очки: 2 / 3")

        // 3. Перевірка умови перемоги
        if (unlockPointsCollected >= unlockPointsNeeded)
        {
            // Всі очки зібрано! Активуємо вихід.
            currentLevelData.GetLevelExit()?.SetActivation(true);
        }
    }

    /// <summary>
    /// **ПУБЛІЧНИЙ МЕТОД**
    /// Запускає процес респавну гравця.
    /// </summary>
    public void StartRespawnProcess()
    {
        // (ОНОВЛЕНО): Ми більше не викликаємо Invoke,
        // оскільки ресет очок має відбутись *одразу* після смерті,
        // а респавн гравця - із затримкою.

        // 1. (ОНОВЛЕНО) Негайно скидаємо прогрес рівня
        if (currentLevelData != null)
        {
            currentLevelData.ResetAllUnlockPoints();
            currentLevelData.GetLevelExit()?.SetActivation(false);
        }
        unlockPointsCollected = 0;

        // TODO: Оновити UI (наприклад, "Очки: 0 / 3")

        // 2. Запускаємо респавн гравця із затримкою
        Invoke(nameof(RespawnPlayer), respawnDelay);
    }

    /// <summary>
    /// (ОНОВЛЕНО): Тепер цей метод *тільки* респавнить гравця.
    /// Скидання логіки рівня відбувається в 'StartRespawnProcess'.
    /// </summary>
    private void RespawnPlayer()
    {
        if (currentLevelData == null)
        {
            Debug.LogError("GameManager: Немає 'currentLevelData' при респавні!", this);
            return;
        }

        // (ОНОВЛЕНО): Викликаємо методи через Singleton-и
        if (PlayerHealth.Instance != null)
        {
            // Використовуємо точку спавну з поточного рівня
            PlayerHealth.Instance.ResetPlayer(currentLevelData.GetSpawnPointPosition());
        }
        else
        {
            Debug.LogError("GameManager: PlayerHealth.Instance не знайдено! Не можу ресетнути здоров'я.", this);
        }

        if (PlayerController.Instance != null)
        {
            PlayerController.Instance.ResetPlayer();
        }
        else
        {
            Debug.LogError("GameManager: PlayerController.Instance не знайдено! Не можу ресетнути контролер.", this);
        }
    }
}
