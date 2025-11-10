using UnityEngine;

/// <summary>
/// (НОВИЙ) "Паспорт" рівня. 
/// Цей компонент має висіти на КОРЕНЕВОМУ об'єкті префабу рівня.
/// Він містить посилання на всі ключові елементи цього рівня.
/// (ОНОВЛЕНО): Автоматично знаходить 'Unlock Points' в об'єкті-контейнері.
/// </summary>
public class LevelData : MonoBehaviour
{
    [Header("Ключові компоненти Рівня")]
    [Tooltip("Об'єкт (Transform), де має з'являтись гравець.")]
    [SerializeField] private Transform spawnPoint;

    [Tooltip("Скрипт виходу з цього рівня.")]
    [SerializeField] private LevelExit levelExit;

    // (ОНОВЛЕНО): Тепер ми посилаємось на батьківський об'єкт
    [Tooltip("Об'єкт-контейнер, що містить УСІ 'Unlock Points' цього рівня як дочірні об'єкти.")]
    [SerializeField] private Transform unlockPointsContainer;

    // (ОНОВЛЕНО): Це поле тепер заповнюється автоматично в Awake
    private UnlockPoint[] unlockPoints;

    private void Awake()
    {
        // Валідація SpawnPoint
        if (spawnPoint == null)
            Debug.LogError($"LevelData на '{gameObject.name}': 'Spawn Point' не призначено!", this);

        // Валідація LevelExit
        if (levelExit == null)
            Debug.LogError($"LevelData на '{gameObject.name}': 'Level Exit' не призначено!", this);

        // (ОНОВЛЕНО): Логіка пошуку Unlock Points
        if (unlockPointsContainer == null)
        {
            Debug.LogError($"LevelData на '{gameObject.name}': 'Unlock Points Container' не призначено!", this);
            unlockPoints = new UnlockPoint[0]; // Створюємо порожній масив, щоб уникнути NullReference
        }
        else
        {
            // Знаходимо ВСІ компоненти UnlockPoint у дочірніх об'єктах
            // (включно з неактивними, про всяк випадок)
            unlockPoints = unlockPointsContainer.GetComponentsInChildren<UnlockPoint>(true);
        }

        // Валідація результату пошуку
        if (unlockPoints.Length == 0)
            Debug.LogWarning($"LevelData на '{gameObject.name}': 'Unlock Points Container' не містить жодного об'єкта з компонентом 'UnlockPoint'.", this);
    }

    /// <summary>
    /// Повертає кількість "Unlock Points", необхідних для проходження рівня.
    /// </summary>
    public int GetUnlockPointsNeeded()
    {
        return unlockPoints.Length;
    }

    /// <summary>
    /// Повертає позицію точки спавну.
    /// </summary>
    public Vector3 GetSpawnPointPosition()
    {
        return spawnPoint.position;
    }

    /// <summary>
    /// Повертає посилання на вихід з рівня.
    /// </summary>
    public LevelExit GetLevelExit()
    {
        return levelExit;
    }

    /// <summary>
    /// **ПУБЛІЧНИЙ МЕТОД**
    /// Скидає стан усіх "Unlock Points" на цьому рівні (наприклад, при смерті гравця).
    /// </summary>
    public void ResetAllUnlockPoints()
    {
        foreach (UnlockPoint point in unlockPoints)
        {
            if (point != null)
            {
                point.ResetUnlockPoint();
            }
        }
    }

    /// <summary>
    /// **(НОВИЙ) ПУБЛІЧНИЙ МЕТОД**
    /// Повертає масив усіх точок інтересу на рівні.
    /// </summary>
    public UnlockPoint[] GetUnlockPoints()
    {
        return unlockPoints;
    }
}