using UnityEngine;
using System.Collections; // Потрібно для корутин
using System.Collections.Generic;

/// <summary>
/// Керує кількістю клякс на сцені.
/// (ОНОВЛЕНО): Використовує корутину для плавного "хвильового" видалення старих клякс.
/// </summary>
public class SplatManager : MonoBehaviour
{
    public static SplatManager Instance { get; private set; }

    [Header("Налаштування оптимізації")]
    [Tooltip("Максимальна кількість клякс, що можуть одночасно існувати на сцені.")]
    [SerializeField] private int maxSplats = 150;

    [Header("Налаштування Зникнення")] // (НОВИЙ РОЗДІЛ)
    [Tooltip("Затримка (в сек.) між початком зникнення кожної 'зайвої' клякси. Створює ефект хвилі.")]
    [SerializeField] private float waveFadeOutDelay = 0.05f;

    private Queue<SplatAppearance> splatsQueue = new Queue<SplatAppearance>();
    private Coroutine queueManagerCoroutine; // (НОВЕ)

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

    // (НОВИЙ МЕТОД)
    private void Start()
    {
        // Запускаємо корутину, яка буде жити вічно і слідкувати за чергою
        queueManagerCoroutine = StartCoroutine(ManageSplatQueue());
    }

    // (НОВИЙ МЕТОД)
    private void OnDestroy()
    {
        // Гарна практика - зупиняти корутини при знищенні об'єкта
        if (queueManagerCoroutine != null)
        {
            StopCoroutine(queueManagerCoroutine);
        }
    }

    /// <summary>
    /// **ПУБЛІЧНИЙ МЕТОД**
    /// (ОНОВЛЕНО): Тепер *тільки* додає кляксу в чергу. 
    /// Видаленням займається корутина.
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

        // (ОНОВЛЕНО): Ми більше не видаляємо клякси тут.
        // Ми просто додаємо нову. Корутина 'ManageSplatQueue' зробить все інше.
        splatsQueue.Enqueue(splat);
    }

    // (НОВА КОРУТИНА)
    /// <summary>
    /// Ця корутина постійно працює у фоновому режимі,
    /// перевіряючи, чи не перевищено ліміт клякс.
    /// </summary>
    private IEnumerator ManageSplatQueue()
    {
        // Вічний цикл
        while (true)
        {
            // Перевіряємо, чи кількість клякс в черзі *перевищує* ліміт
            if (splatsQueue.Count > maxSplats)
            {
                // Ліміт перевищено, видаляємо одну (найстарішу)
                SplatAppearance oldestSplat = splatsQueue.Dequeue();
                if (oldestSplat != null)
                {
                    // Запускаємо її зникнення
                    oldestSplat.StartFadeOutAndDestroy();
                }

                // (ГОЛОВНА ЗМІНА): Чекаємо 'waveFadeOutDelay' секунд
                // перед тим, як повернутись на початок циклу і перевірити/видалити НАСТУПНУ кляксу.
                yield return new WaitForSeconds(waveFadeOutDelay);
            }
            else
            {
                // Якщо ліміт не перевищено, просто чекаємо наступного кадру.
                // Це ефективно і не навантажує процесор.
                yield return null;
            }
        }
    }
}

