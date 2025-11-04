using UnityEngine;

/// <summary>
/// (НОВИЙ) "Паспорт" рівня. 
/// Цей компонент має висіти на КОРЕНЕВОМУ об'єкті префабу рівня.
/// Він містить посилання на всі ключові елементи цього рівня.
/// </summary>
public class LevelData : MonoBehaviour
{
    [Header("Ключові компоненти Рівня")]
    [Tooltip("Об'єкт (Transform), де має з'являтись гравець.")]
    [SerializeField] private Transform spawnPoint;

    [Tooltip("Скрипт виходу з цього рівня.")]
    [SerializeField] private LevelExit levelExit;

    [Tooltip("Масив усіх 'Unlock Points', які є на цьому рівні.")]
    [SerializeField] private UnlockPoint[] unlockPoints;

    private void Awake()
    {
        // Валідація
        if (spawnPoint == null)
            Debug.LogError($"LevelData на '{gameObject.name}': 'Spawn Point' не призначено!", this);

        if (levelExit == null)
            Debug.LogError($"LevelData на '{gameObject.name}': 'Level Exit' не призначено!", this);

        if (unlockPoints == null || unlockPoints.Length == 0)
            Debug.LogWarning($"LevelData на '{gameObject.name}': 'Unlock Points' не призначено.", this);
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