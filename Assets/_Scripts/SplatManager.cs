using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Керує кількістю клякс на сцені, щоб уникнути проблем з продуктивністю.
/// Видаляє найстаріші клякси, коли досягнуто ліміту.
/// </summary>
public class SplatManager : MonoBehaviour
{
    // Singleton патерн, щоб до скрипта можна було легко звернутись з будь-якого іншого місця.
    public static SplatManager Instance { get; private set; }

    [Header("Налаштування оптимізації")]
    [Tooltip("Максимальна кількість клякс, що можуть одночасно існувати на сцені.")]
    [SerializeField] private int maxSplats = 150;

    // Використовуємо чергу (Queue), щоб легко відстежувати, яка клякса найстаріша.
    // Перший, хто зайшов - перший, хто вийде.
    private Queue<GameObject> splatsQueue = new Queue<GameObject>();

    private void Awake()
    {
        // Реалізація Singleton патерну
        if (Instance != null && Instance != this)
        {
            // Якщо екземпляр вже існує, знищуємо цей дублікат.
            Destroy(gameObject);
        }
        else
        {
            // Якщо екземпляра ще немає, робимо цей об'єкт головним.
            Instance = this;
        }
    }

    /// <summary>
    /// **ПУБЛІЧНИЙ МЕТОД**
    /// Додає нову кляксу до системи відстеження.
    /// Якщо ліміт перевищено, видаляє найстарішу кляксу.
    /// </summary>
    /// <param name="splatInstance">Ігровий об'єкт клякси, яку потрібно додати.</param>
    public void AddSplat(GameObject splatInstance)
    {
        // Якщо кількість клякс досягла або перевищила ліміт, починаємо видаляти старі.
        while (splatsQueue.Count >= maxSplats)
        {
            // Видаляємо найстарішу кляксу зі сцени
            GameObject oldestSplat = splatsQueue.Dequeue(); // Витягуємо найперший доданий елемент з черги.
            if (oldestSplat != null)
            {
                Destroy(oldestSplat);
            }
        }

        // Додаємо нову кляксу в кінець черги
        splatsQueue.Enqueue(splatInstance);
    }
}

