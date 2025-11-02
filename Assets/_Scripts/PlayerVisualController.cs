using MoreMountains.Feedbacks;
using UnityEngine;
using System.Collections;

/// <summary>
/// Керує ВСІМА візуальними ефектами гравця.
/// (ОНОВЛЕНО): Тепер також спавнить клякси та партикли при ударі об стіну.
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

    // --- (НОВИЙ РОЗДІЛ з минулого разу) ---
    [Header("Ефекти Ударів (Стіни)")]
    [Tooltip("Префаб партиклів 'бризок', що спавняться при ударі.")]
    [SerializeField] private GameObject wallHitParticlePrefab;
    [Tooltip("Префаб клякси, що спавниться при ударі.")]
    [SerializeField] private GameObject splatPrefab;
    // --- ---

    [Header("Налаштування Сквашу (Удар об стіну)")]
    [SerializeField] private Transform collisionPivot;
    [Tooltip("Наскільки сильно сквашити при МАКСИМАЛЬНІЙ швидкості.")]
    [SerializeField] private float impactSquashAmount = 0.8f;
    [Tooltip("Наскільки сильно розтягнути при МАКСИМАЛЬНІЙ швидкості.")]
    [SerializeField] private float impactStretchAmount = 1.2f;
    [SerializeField] private float impactAnimationDuration = 0.2f;

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
        // (ТВІЙ КОД з минулого разу)
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

    // (ТВІЙ КОД з минулого разу)
    public void PlayJumpEffect()
    {
        if (mMSpringSquashAndStretch != null) mMSpringSquashAndStretch.Bump(Random.Range(minMaxSquashForce.x, minMaxSquashForce.y));
        if (mMSpringRotation != null) mMSpringRotation.Bump(new Vector3(0, 0, Random.Range(minMaxSpringRotationForce.x, minMaxSpringRotationForce.y) * (Random.Range(0, 2) * 2 - 1)));
    }

    // (ТВІЙ КОД з минулого разу)
    public void PlayDeathEffect(Vector3 deathPosition)
    {
        foreach (var renderer in playerRenderers) { if (renderer != null) renderer.enabled = false; }
        if (deathParticlePrefab != null) Instantiate(deathParticlePrefab, deathPosition, deathParticlePrefab.transform.rotation);
        else Debug.LogError("PlayerVisualController: Префаб частинок смерті не призначено!", this);
    }

    // (ТВІЙ КОД з минулого разу)
    public void ResetVisuals()
    {
        foreach (var renderer in playerRenderers) { if (renderer != null) renderer.enabled = true; }
    }

    // --- (НОВИЙ МЕТОД з минулого разу) ---
    /// <summary>
    /// **ПУБЛІЧНИЙ МЕТОД**
    /// Спавнить кляксу та партикли "бризок" у точці контакту.
    /// </summary>
    public void PlayWallHitEffects(Vector2 contactPoint, Vector2 contactNormal)
    {
        // 1. Спавн партиклів "бризок"
        if (wallHitParticlePrefab != null)
        {
            // (ВИПРАВЛЕНО): Розраховуємо кут повороту ТІЛЬКИ навколо осі Z
            // Припускаємо, що партикли "дивляться" вгору (Vector2.up) по дефолту
            float angle = Vector2.SignedAngle(Vector2.up, contactNormal);
            Quaternion rotation2D = Quaternion.Euler(0, 0, angle);

            GameObject particleInstance = Instantiate(wallHitParticlePrefab, (Vector3)contactPoint, rotation2D); // (ЗМІНЕНО)

            // (НОВЕ): Встановлюємо колір партиклів з PaletteManager
            if (PaletteManager.Instance != null && PaletteManager.Instance.CurrentPalette != null)
            {
                var mainModule = particleInstance.GetComponent<ParticleSystem>().main;
                mainModule.startColor = PaletteManager.Instance.CurrentPalette.PaintAndPlayerColor;
            }
        }

        // 2. Спавн клякси (ТУТ БЕЗ ЗМІН)
        if (splatPrefab != null)
        {
            // Клякса спавниться без повороту (Quaternion.identity), 
            // а її скрипт 'SplatAppearance' сам задає випадковий Z-поворот.
            GameObject splatInstance = Instantiate(splatPrefab, contactPoint, Quaternion.identity);
            if (SplatManager.Instance != null)
            {
                SplatManager.Instance.AddSplat(splatInstance);
            }
            else
            {
                Debug.LogWarning("SplatManager не знайдено, клякса не була зареєстрована.");
            }
        }
    }
    // --- ---


    /// <summary>
    /// **ПУБЛІЧНИЙ МЕТОД (ТВІЙ КОД з минулого разу)**
    /// Запускає корутину анімації "сквашу".
    /// </summary>
    public void PlayWallImpactEffect(Vector2 contactPoint, Vector2 contactNormal, float impactMagnitude)
    {
        if (isImpacting || collisionPivot == null) return;
        if (impactMagnitude < minImpactSpeed) return;
        StartCoroutine(WallImpactCoroutine(contactPoint, contactNormal, impactMagnitude));
    }

    /// <summary>
    /// Корутина, що анімує 'Visuals'. (ТВІЙ КОД з минулого разу)
    /// </summary>
    private IEnumerator WallImpactCoroutine(Vector2 contactPoint, Vector2 contactNormal, float impactMagnitude)
    {
        isImpacting = true;
        Quaternion visualOriginalWorldRotation = visualsTransform.rotation;
        collisionPivot.position = contactPoint;
        collisionPivot.up = contactNormal;
        visualsTransform.SetParent(collisionPivot, true);

        float normalizedSpeed = Mathf.InverseLerp(minImpactSpeed, maxImpactSpeed, impactMagnitude);
        float currentSquash = Mathf.Lerp(1f, impactSquashAmount, normalizedSpeed);
        float currentStretch = Mathf.Lerp(1f, impactStretchAmount, normalizedSpeed);

        float elapsedTime = 0f;
        Vector3 originalScale = Vector3.one;
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

