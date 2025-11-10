using UnityEngine;
using MoreMountains.Feedbacks; // Якщо ви використовуєте Feel для ефектів
using System.Collections; // (ДОДАНО): Потрібно для корутин

/// <summary>
/// (НОВИЙ) Контролер для "Виходу з рівня".
/// (ОНОВЛЕНО): Тепер плавно притягує гравця до центру перед переходом.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class LevelExit : MonoBehaviour
{
    [Header("Налаштування Об'єкта")]
    [Tooltip("Візуал, коли вихід НЕактивний.")]
    [SerializeField] private GameObject visualElement_Disabled;
    [Tooltip("Візуал, коли вихід АКТИВОВАНО (очки зібрано).")]
    [SerializeField] private GameObject visualElement_Enabled;

    // (НОВИЙ БЛОК)
    [Header("Налаштування Притягування")]
    [Tooltip("Час в секундах, за який гравець притягнеться до центру виходу.")]
    [SerializeField] private float centeringDuration = 0.5f;
    // (КІНЕЦЬ НОВОГО БЛОКУ)

    [Header("Ефекти (Опційно)")]
    [Tooltip("Ефект, що програється при активації (коли всі очки зібрано).")]
    [SerializeField] private MMF_Player activationFeedback;
    [Tooltip("Ефект, що програється при вході (перехід на рівень).")]
    [SerializeField] private MMF_Player exitFeedback;

    // --- Внутрішні змінні ---
    private Collider2D col;

    // (ОНОВЛЕНО): Ініціалізуємо як 'true', щоб 'Awake' 
    // гарантовано виконав оновлення візуалу при виклику SetActivation(false).
    private bool isActivated = true;

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
        // (ОНОВЛЕНО): Отримуємо PlayerController
        PlayerController player = other.GetComponent<PlayerController>();

        // Якщо вихід не активовано або це не гравець - ігноруємо
        if (!isActivated || player == null)
        {
            return;
        }

        // Ми увійшли в активний вихід. Починаємо перехід.
        // (ОНОВЛЕНО): Передаємо Transform гравця
        EnterExit(player.transform);
    }

    // (ОНОВЛЕНО): Тепер приймає Transform гравця
    private void EnterExit(Transform playerTransform)
    {
        // Вимикаємо колайдер, щоб не спрацювати двічі
        col.enabled = false;

        // (ОНОВЛЕНО): Вимикаємо керування гравцем та фізику
        if (PlayerController.Instance != null)
        {
            PlayerController.Instance.enabled = false;
            Rigidbody2D playerRb = PlayerController.Instance.GetComponent<Rigidbody2D>();
            if (playerRb != null)
            {
                playerRb.bodyType = RigidbodyType2D.Kinematic;
                // (ВИПРАВЛЕНО) Використовуємо linearVelocity замість застарілого velocity
                playerRb.linearVelocity = Vector2.zero;
            }
        }

        // Програємо ефект виходу
        if (exitFeedback != null)
        {
            exitFeedback.PlayFeedbacks();
        }

        // (ОНОВЛЕНО): Запускаємо корутину, яка плавно перемістить гравця
        StartCoroutine(CenterAndLoadNextLevel(playerTransform));
    }

    /// <summary>
    /// (НОВЕ) Корутина, що плавно рухає гравця до центру
    /// і ПОТІМ завантажує наступний рівень.
    /// </summary>
    private IEnumerator CenterAndLoadNextLevel(Transform playerTransform)
    {
        Vector3 startPos = playerTransform.position;
        // (ОНОВЛЕНО): Переконуємось, що Z-координата гравця не зміниться
        Vector3 endPos = new Vector3(transform.position.x, transform.position.y, startPos.z);
        float elapsedTime = 0f;

        // Фаза 1: Плавне переміщення
        while (elapsedTime < centeringDuration)
        {
            if (playerTransform == null) yield break; // Гравця знищили? Виходимо.

            playerTransform.position = Vector3.Lerp(startPos, endPos, elapsedTime / centeringDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Гарантуємо фінальну позицію
        if (playerTransform != null)
        {
            playerTransform.position = endPos;
        }

        // Фаза 2: Завантаження наступного рівня
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.LoadNextLevel();
        }
    }
}