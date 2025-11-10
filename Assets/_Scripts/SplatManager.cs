using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Керує кількістю клякс на сцені.
/// (ОНОВЛЕНО): ClearAllSplats тепер є корутиною, яка чекає на зникнення.
/// </summary>
public class SplatManager : MonoBehaviour
{
    public static SplatManager Instance { get; private set; }

    [Header("Налаштування оптимізації")]
    [Tooltip("Максимальна кількість клякс, що можуть одночасно існувати на сцені.")]
    [SerializeField] private int maxSplats = 150;

    [Header("Налаштування Зникнення")]
    [Tooltip("Затримка (в сек.) між початком зникнення кожної 'зайвої' клякси. Створює ефект хвилі.")]
    [SerializeField] private float waveFadeOutDelay = 0.05f;

    // (НОВЕ): Загальний час зникнення клякси.
    // 'SplatAppearance' буде використовувати це значення.
    [Tooltip("Загальний час в секундах, за який клякса зникає (при очищенні рівня).")]
    [SerializeField] public float splatFadeOutDuration = 0.3f;

    private Queue<SplatAppearance> splatsQueue = new Queue<SplatAppearance>();
    private Coroutine queueManagerCoroutine;

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

    private void Start()
    {
        queueManagerCoroutine = StartCoroutine(ManageSplatQueue());
    }

    private void OnDestroy()
    {
        if (queueManagerCoroutine != null)
        {
            StopCoroutine(queueManagerCoroutine);
        }
    }

    /// <summary>
    /// **ПУБЛІЧНИЙ МЕТОД**
    /// Додає кляксу в чергу.
    /// </summary>
    public void AddSplat(GameObject splatInstance)
    {
        SplatAppearance splat = splatInstance.GetComponent<SplatAppearance>();
        if (splat == null)
        {
            Debug.LogWarning("SplatManager: Доданий об'єкт не має компонента SplatAppearance!", splatInstance);
            Destroy(splatInstance);
            return;
        }

        splatsQueue.Enqueue(splat);
    }

    /// <summary>
    /// Корутина, що постійно працює у фоновому режимі,
    /// перевіряючи, чи не перевищено ліміт клякс.
    /// </summary>
    private IEnumerator ManageSplatQueue()
    {
        while (true)
        {
            if (splatsQueue.Count > maxSplats)
            {
                // Видаляємо найстарішу, АЛЕ перевіряємо, чи її вже не знищили
                // (наприклад, під час ClearAllSplats)
                if (splatsQueue.Count > 0)
                {
                    SplatAppearance oldestSplat = splatsQueue.Dequeue();
                    if (oldestSplat != null)
                    {
                        oldestSplat.StartFadeOutAndDestroy();
                    }
                }

                yield return new WaitForSeconds(waveFadeOutDelay);
            }
            else
            {
                yield return null;
            }
        }
    }

    /// <summary>
    /// **(ОНОВЛЕНО) ПУБЛІЧНА КОРУТИНА**
    /// Запускає зникнення ВСІХ клякс і ЧЕКАЄ на їх повне зникнення.
    /// </summary>
    public IEnumerator ClearAllSplatsCoroutine()
    {
        // 1. Зупиняємо корутину, щоб вона не конфліктувала з очищенням
        if (queueManagerCoroutine != null)
        {
            StopCoroutine(queueManagerCoroutine);
        }

        // 2. Проходимо по всіх кляксах в черзі і запускаємо їх зникнення
        if (splatsQueue.Count > 0)
        {
            foreach (SplatAppearance splat in splatsQueue)
            {
                if (splat != null)
                {
                    // Викликаємо зникнення (скрипт 'SplatAppearance' сам себе знищить)
                    splat.StartFadeOutAndDestroy();
                }
            }

            // 3. (НОВЕ) Чекаємо, поки вони зникнуть
            yield return new WaitForSeconds(splatFadeOutDuration);
        }

        // 4. Очищуємо саму чергу
        splatsQueue.Clear();

        // 5. (ВАЖЛИВО) Перезапускаємо корутину, щоб вона була готова до нового рівня
        queueManagerCoroutine = StartCoroutine(ManageSplatQueue());
    }
}