using UnityEngine;
using MoreMountains.Feedbacks; // Якщо ви використовуєте Feel для ефектів

/// <summary>
/// (НОВИЙ) Контролер для "Виходу з рівня".
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class LevelExit : MonoBehaviour
{
    [Header("Налаштування Об'єкта")]
    [Tooltip("Візуал, коли вихід НЕактивний.")]
    [SerializeField] private GameObject visualElement_Disabled;
    [Tooltip("Візуал, коли вихід АКТИВОВАНО (очки зібрано).")]
    [SerializeField] private GameObject visualElement_Enabled;

    [Header("Ефекти (Опційно)")]
    [Tooltip("Ефект, що програється при активації (коли всі очки зібрано).")]
    [SerializeField] private MMF_Player activationFeedback;
    [Tooltip("Ефект, що програється при вході (перехід на рівень).")]
    [SerializeField] private MMF_Player exitFeedback;

    // --- Внутрішні змінні ---
    private Collider2D col;
    private bool isActivated = false;

    private void Awake()
    {
        col = GetComponent<Collider2D>();
        if (!col.isTrigger)
        {
            Debug.LogWarning($"LevelExit '{gameObject.name}': Колайдер не є тригером!", this);
            col.isTrigger = true;
        }

        // Починаємо в деактивованому стані
        SetActivation(false);
    }

    /// <summary>
    /// **ПУБЛІЧНИЙ МЕТОД**
    /// Вмикає або вимикає вихід. Викликається з GameManager.
    /// </summary>
    public void SetActivation(bool activated)
    {
        if (isActivated == activated) return; // Стан не змінився

        isActivated = activated;

        // Вмикаємо/вимикаємо колайдер (щоб не можна було увійти)
        col.enabled = isActivated;

        // Оновлюємо візуал
        if (visualElement_Disabled != null) visualElement_Disabled.SetActive(!isActivated);
        if (visualElement_Enabled != null) visualElement_Enabled.SetActive(isActivated);

        // Якщо ми активуємо вихід - програємо ефект
        if (isActivated && activationFeedback != null)
        {
            activationFeedback.PlayFeedbacks();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Якщо вихід не активовано або це не гравець - ігноруємо
        if (!isActivated || other.GetComponent<PlayerController>() == null)
        {
            return;
        }

        // Ми увійшли в активний вихід. Починаємо перехід.
        EnterExit();
    }

    private void EnterExit()
    {
        // Вимикаємо колайдер, щоб не спрацювати двічі
        col.enabled = false;

        // Програємо ефект виходу
        if (exitFeedback != null)
        {
            exitFeedback.PlayFeedbacks();
        }

        // Кажемо LevelManager завантажити наступний рівень
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.LoadNextLevel();
        }
    }
}
