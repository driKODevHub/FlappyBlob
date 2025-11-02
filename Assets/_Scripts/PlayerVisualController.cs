using MoreMountains.Feedbacks;
using UnityEngine;
using System.Collections;

/// <summary>
/// Керує ВСІМА візуальними ефектами гравця.
/// (ОНОВЛЕНО): Тепер приймає силу удару для динамічного сквашу.
/// </summary>
public class PlayerVisualController : MonoBehaviour
{
    public static PlayerVisualController Instance { get; private set; }

    [Header("Посилання на візуал")]
    [SerializeField] private Renderer[] playerRenderers;
    [SerializeField] private GameObject deathParticlePrefab;

    [Header("Компоненти Feel (Стрибок)")]
    [SerializeField] private MMSpringSquashAndStretch mMSpringSquashAndStretch;
    [SerializeField] private Vector2 minMaxSquashForce = new Vector2(20f, 30f);
    [SerializeField] private MMSpringRotation mMSpringRotation;
    [SerializeField] private Vector2 minMaxSpringRotationForce = new Vector2(1000f, 1500f);

    [Header("Налаштування Сквашу (Удар об стіну)")]
    [SerializeField] private Transform collisionPivot;
    [Tooltip("Наскільки сильно сквашити при МАКСИМАЛЬНІЙ швидкості.")]
    [SerializeField] private float impactSquashAmount = 0.8f;
    [Tooltip("Наскільки сильно розтягнути при МАКСИМАЛЬНІЙ швидкості.")]
    [SerializeField] private float impactStretchAmount = 1.2f;
    [SerializeField] private float impactAnimationDuration = 0.2f;

    // --- (НОВЕ): Динамічний скваш ---
    [Header("Налаштування Динамічного Сквашу")]
    [Tooltip("Мінімальна швидкість удару, щоб ефект спрацював.")]
    [SerializeField] private float minImpactSpeed = 1f;
    [Tooltip("Швидкість удару, при якій ефект досягне 100% сили (impactSquashAmount).")]
    [SerializeField] private float maxImpactSpeed = 15f;


    // --- Внутрішні змінні ---
    private Transform visualsTransform;
    private Transform visualsOriginalParent;
    private bool isImpacting = false;

    private void Awake()
    {
        // ... (Singleton, пошук рендерерів і компонентів) ...
        if (Instance != null && Instance != this) Destroy(gameObject);
        else Instance = this;
        if (mMSpringSquashAndStretch == null) mMSpringSquashAndStretch = GetComponent<MMSpringSquashAndStretch>();
        if (mMSpringRotation == null) mMSpringRotation = GetComponent<MMSpringRotation>();
        if (playerRenderers == null || playerRenderers.Length == 0)
        {
            playerRenderers = GetComponentsInChildren<Renderer>();
            if (playerRenderers.Length > 0) Debug.Log($"PlayerVisualController: Автоматично знайдено {playerRenderers.Length} рендерерів.", this);
            else Debug.LogWarning("PlayerVisualController: 'playerRenderers' порожній і не знайдено в дочірніх об'єктах.", this);
        }
        visualsTransform = transform;
        visualsOriginalParent = transform.parent;
        if (collisionPivot == null) Debug.LogError("PlayerVisualController: 'Collision Pivot' не призначено!", this);
    }

    // ... (PlayJumpEffect, PlayDeathEffect, ResetVisuals - без змін) ...
    public void PlayJumpEffect()
    {
        if (mMSpringSquashAndStretch != null) mMSpringSquashAndStretch.Bump(Random.Range(minMaxSquashForce.x, minMaxSquashForce.y));
        if (mMSpringRotation != null) mMSpringRotation.Bump(new Vector3(0, 0, Random.Range(minMaxSpringRotationForce.x, minMaxSpringRotationForce.y) * (Random.Range(0, 2) * 2 - 1)));
    }
    public void PlayDeathEffect(Vector3 deathPosition)
    {
        foreach (var renderer in playerRenderers) { if (renderer != null) renderer.enabled = false; }
        if (deathParticlePrefab != null) Instantiate(deathParticlePrefab, deathPosition, deathParticlePrefab.transform.rotation);
        else Debug.LogError("PlayerVisualController: Префаб частинок смерті не призначено!", this);
    }
    public void ResetVisuals()
    {
        foreach (var renderer in playerRenderers) { if (renderer != null) renderer.enabled = true; }
    }


    /// <summary>
    /// **ПУБЛІЧНИЙ МЕТОД (ОНОВЛЕНО)**
    /// Запускає корутину анімації "сквашу".
    /// </summary>
    public void PlayWallImpactEffect(Vector2 contactPoint, Vector2 contactNormal, float impactMagnitude)
    {
        if (isImpacting || collisionPivot == null) return;

        // (НОВЕ): Перевіряємо, чи швидкість удару достатньо сильна
        if (impactMagnitude < minImpactSpeed) return;

        StartCoroutine(WallImpactCoroutine(contactPoint, contactNormal, impactMagnitude));
    }

    /// <summary>
    /// Корутина, що анімує 'Visuals'.
    /// (ОНОВЛЕНО): Розраховує силу сквашу на основі impactMagnitude.
    /// </summary>
    private IEnumerator WallImpactCoroutine(Vector2 contactPoint, Vector2 contactNormal, float impactMagnitude)
    {
        isImpacting = true;
        Quaternion visualOriginalWorldRotation = visualsTransform.rotation;
        collisionPivot.position = contactPoint;
        collisionPivot.up = contactNormal;
        visualsTransform.SetParent(collisionPivot, true);

        // --- (НОВЕ): Розрахунок динамічної сили ---
        // Нормалізуємо швидкість (отримуємо значення від 0 до 1)
        float normalizedSpeed = Mathf.InverseLerp(minImpactSpeed, maxImpactSpeed, impactMagnitude);
        // Розраховуємо поточну силу сквашу/розтягування
        // Lerp(1, 0.8, 0.5) = 0.9 (трохи сквашимо)
        // Lerp(1, 1.2, 0.5) = 1.1 (трохи розтягнемо)
        float currentSquash = Mathf.Lerp(1f, impactSquashAmount, normalizedSpeed);
        float currentStretch = Mathf.Lerp(1f, impactStretchAmount, normalizedSpeed);

        // --- ---

        float elapsedTime = 0f;
        Vector3 originalScale = Vector3.one;
        // Використовуємо розраховані значення
        Vector3 squashScale = new Vector3(currentStretch, currentSquash, 1f);

        float halfDuration = impactAnimationDuration / 2f;

        // Фаза 1: Скваш
        while (elapsedTime < halfDuration)
        {
            collisionPivot.localScale = Vector3.Lerp(originalScale, squashScale, elapsedTime / halfDuration);
            visualsTransform.rotation = visualOriginalWorldRotation;
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Фаза 2: Повернення
        elapsedTime = 0f;
        while (elapsedTime < halfDuration)
        {
            collisionPivot.localScale = Vector3.Lerp(squashScale, originalScale, elapsedTime / halfDuration);
            visualsTransform.rotation = visualOriginalWorldRotation;
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // 5. Очищення
        collisionPivot.localScale = Vector3.one;
        visualsTransform.SetParent(visualsOriginalParent, true);
        visualsTransform.rotation = visualOriginalWorldRotation;
        collisionPivot.rotation = Quaternion.identity;
        isImpacting = false;
    }
}

