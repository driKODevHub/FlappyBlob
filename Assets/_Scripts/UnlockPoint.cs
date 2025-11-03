using UnityEngine;
using MoreMountains.Feedbacks; // Якщо ви використовуєте Feel для ефектів

/// <summary>
/// (НОВИЙ) Контролер для "Unlock Point" (об'єкта, який треба зібрати).
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class UnlockPoint : MonoBehaviour
{
    [Header("Налаштування Об'єкта")]
    [Tooltip("Візуальна частина, яка буде вимикатись при підбиранні.")]
    [SerializeField] private GameObject visualElement;

    [Header("Ефекти (Опційно)")]
    [Tooltip("Ефект, що програється при підбиранні.")]
    [SerializeField] private MMF_Player collectFeedback;

    // --- Внутрішні змінні ---
    private Collider2D col;
    private bool isCollected = false;

    private void Awake()
    {
        col = GetComponent<Collider2D>();
        if (!col.isTrigger)
        {
            Debug.LogWarning($"UnlockPoint '{gameObject.name}': Колайдер не є тригером!", this);
            col.isTrigger = true;
        }

        ResetUnlockPoint(); // Встановлюємо початковий стан
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Якщо вже зібрано або це не гравець - ігноруємо
        if (isCollected || other.GetComponent<PlayerController>() == null)
        {
            return;
        }

        Collect();
    }

    /// <summary>
    /// Логіка "підбирання" точки.
    /// </summary>
    private void Collect()
    {
        isCollected = true;

        // Вимикаємо візуал та колайдер
        if (visualElement != null) visualElement.SetActive(false);
        col.enabled = false;

        // Програємо ефект
        if (collectFeedback != null)
        {
            collectFeedback.PlayFeedbacks();
        }

        // Повідомляємо GameManager, що ми зібрали точку
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnUnlockPointCollected();
        }
    }

    /// <summary>
    /// **ПУБЛІЧНИЙ МЕТОД**
    /// Скидає точку у початковий, незібраний стан.
    /// </summary>
    public void ResetUnlockPoint()
    {
        isCollected = false;

        // Вмикаємо візуал та колайдер
        if (visualElement != null) visualElement.SetActive(true);
        col.enabled = true;
    }
}
