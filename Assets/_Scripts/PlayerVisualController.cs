using MoreMountains.Feedbacks;
using UnityEngine;
using System.Collections;

/// <summary>
/// Керує ВСІМА візуальними ефектами гравця.
/// (ОНОВЛЕНО): Тепер також спавнить клякси та партикли при ударі об стіну.
/// (ОНОВЛЕНО 2): Додано ефект "сильного приземлення".
/// (ОНОВЛЕНО 3): Додано ефект "pop-up" при спавні + виправлено race condition.
/// (ОНОВЛЕНО 4): Виправлено баг з поворотом при скваші об стіну.
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

    [Header("Компоненти Feel (Спавн)")]
    [Tooltip("Спрінг-ефект для 'pop-up' при спавні.")]
    [SerializeField] private MMSpringScale mMSpringScale;

    [Header("Ефекти Ударів (Стіни)")]
    [Tooltip("Префаб партиклів 'бризок', що спавняться при ударі.")]
    [SerializeField] private GameObject wallHitParticlePrefab;
    [Tooltip("Префаб клякси, що спавниться при ударі.")]
    [SerializeField] private GameObject splatPrefab;

    [Header("Налаштування Сквашу (Удар об стіну)")]
    [SerializeField] private Transform collisionPivot;
    [SerializeField] private float impactSquashAmount = 0.8f;
    [SerializeField] private float impactStretchAmount = 1.2f;
    [SerializeField] private float impactAnimationDuration = 0.2f;

    [Header("Налаштування Динамічного Сквашу (Удар)")]
    [SerializeField] private float minImpactSpeed = 1f;
    [SerializeField] private float maxImpactSpeed = 15f;

    [Header("Ефекти Сильного Приземлення (Стіни)")]
    [Tooltip("Префаб партиклів. (Підказка: можна с-дублювати 'deathParticlePrefab' і змінити 'Shape' на 'Cone')")]
    [SerializeField] private GameObject hardLandingParticlePrefab;
    [Tooltip("Мінімальна швидкість удару об стіну, щоб спрацював ефект.")]
    [SerializeField] private float minLandingSpeed = 8f;
    [Tooltip("Швидкість удару, при якій ефект досягне 100% сили.")]
    [SerializeField] private float maxSpeedForLandingParticles = 20f;
    [Space]
    [Tooltip("Кількість партиклів при мінімальній швидкості.")]
    [SerializeField] private int minParticleCount = 10;
    [Tooltip("Кількість партиклів при максимальній швидкості.")]
    [SerializeField] private int maxParticleCount = 30;
    [Space]
    [Tooltip("Швидкість партиклів при мінімальній швидкості.")]
    [SerializeField] private float minParticleSpeed = 3f;
    [Tooltip("Швидкість партиклів при максимальній швидкості.")]
    [SerializeField] private float maxParticleSpeed = 8f;

    // --- Внутрішні змінні ---
    private Transform visualsTransform;
    private Transform visualsOriginalParent; // Це Transform кореневого об'єкта Гравця
    private bool isImpacting = false;

    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        else Instance = this;

        // Кешуємо компоненти Feel
        if (mMSpringSquashAndStretch == null) mMSpringSquashAndStretch = GetComponent<MMSpringSquashAndStretch>();
        if (mMSpringRotation == null) mMSpringRotation = GetComponent<MMSpringRotation>();
        if (mMSpringScale == null) mMSpringScale = GetComponent<MMSpringScale>();

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

    /// <summary>
    /// (ОНОВЛЕНО) Запускає ефект "pop-up" при спавні/ресеті.
    /// </summary>
    public void PlaySpawnEffect()
    {
        if (mMSpringScale == null)
        {
            Debug.LogWarning("PlayerVisualController: MMSpringScale не призначено! Ефект спавну не працюватиме.", this);
            return;
        }

        // 1. Миттєво ховаємо візуал (встановлюємо скейл в 0)
        mMSpringScale.MoveToInstant(Vector3.zero);

        // 2. (ВИПРАВЛЕНО) Чітко кажемо спрінгу рухатись до Vector3.one.
        // Це надійніше, ніж RestoreInitialValue(), бо не залежить від кешування.
        mMSpringScale.MoveTo(Vector3.one);
    }

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
    /// Спавнить кляксу та партикли "бризок" у точці контакту.
    /// </summary>
    public void PlayWallHitEffects(Vector2 contactPoint, Vector2 contactNormal)
    {
        // 1. Спавн партиклів "бризок"
        if (wallHitParticlePrefab != null)
        {
            float angle = Vector2.SignedAngle(Vector2.up, contactNormal);
            Quaternion rotation2D = Quaternion.Euler(0, 0, angle);
            GameObject particleInstance = Instantiate(wallHitParticlePrefab, (Vector3)contactPoint, rotation2D);
            if (PaletteManager.Instance != null && PaletteManager.Instance.CurrentPalette != null)
            {
                var mainModule = particleInstance.GetComponent<ParticleSystem>().main;
                mainModule.startColor = PaletteManager.Instance.CurrentPalette.PaintAndPlayerColor;
            }
        }

        // 2. Спавн клякси
        if (splatPrefab != null)
        {
            GameObject splatInstance = Instantiate(splatPrefab, contactPoint, Quaternion.identity);
            if (SplatManager.Instance != null)
            {
                SplatManager.Instance.AddSplat(splatInstance);
            }
        }
    }


    /// <summary>
    /// Запускає корутину анімації "сквашу".
    /// </summary>
    public void PlayWallImpactEffect(Vector2 contactPoint, Vector2 contactNormal, float impactMagnitude)
    {
        if (isImpacting || collisionPivot == null) return;
        if (impactMagnitude < minImpactSpeed) return;
        StartCoroutine(WallImpactCoroutine(contactPoint, contactNormal, impactMagnitude));
    }

    /// <summary>
    /// **ПУБЛІЧНИЙ МЕТОД**
    /// Спавнить партикли "сильного приземлення", динамічно налаштовуючи їх.
    /// </summary>
    public void PlayHardLandingEffect(Vector2 contactPoint, Vector2 contactNormal, float impactMagnitude)
    {
        // 1. Перевірка швидкості та префабу
        if (hardLandingParticlePrefab == null) return;
        if (impactMagnitude < minLandingSpeed) return;

        // 2. Створюємо інстанс
        Vector3 spawnPosition = visualsOriginalParent.position;
        GameObject particleInstance = Instantiate(hardLandingParticlePrefab, spawnPosition, Quaternion.identity);

        ParticleSystem ps = particleInstance.GetComponent<ParticleSystem>();
        if (ps == null)
        {
            Debug.LogError("HardLandingEffect: Префаб не має компонента ParticleSystem!", particleInstance);
            Destroy(particleInstance);
            return;
        }

        // 3. Розраховуємо силу (0..1)
        float force = Mathf.InverseLerp(minLandingSpeed, maxSpeedForLandingParticles, impactMagnitude);
        force = Mathf.Clamp01(force);

        // 4. Налаштовуємо Головний модуль (Main)
        var mainModule = ps.main;
        float particleSpeed = Mathf.Lerp(minParticleSpeed, maxParticleSpeed, force);
        mainModule.startSpeed = particleSpeed;

        if (PaletteManager.Instance != null && PaletteManager.Instance.CurrentPalette != null)
        {
            mainModule.startColor = PaletteManager.Instance.CurrentPalette.PaintAndPlayerColor;
        }

        // 5. Налаштовуємо Модуль Емісії (Emission)
        var emissionModule = ps.emission;
        emissionModule.enabled = true;
        int particleCount = (int)Mathf.Lerp(minParticleCount, maxParticleCount, force);

        var burstArray = new ParticleSystem.Burst[1];
        burstArray[0] = new ParticleSystem.Burst(0.0f, (short)particleCount);
        emissionModule.SetBursts(burstArray);

        // 6. Налаштовуємо Поворот (Shape)
        float angle = Vector2.SignedAngle(Vector2.up, contactNormal);
        particleInstance.transform.rotation = Quaternion.Euler(0, 0, angle);

        // 7. Запускаємо
        ps.Play();
    }

    /// <summary>
    /// Корутина, що анімує 'Visuals'.
    /// </summary>
    private IEnumerator WallImpactCoroutine(Vector2 contactPoint, Vector2 contactNormal, float impactMagnitude)
    {
        isImpacting = true;

        // (ВИПРАВЛЕНО): Зберігаємо стабільний поворот кореневого об'єкта,
        // а не візуалу, що обертається.
        Quaternion stableWorldRotation = visualsOriginalParent.rotation;

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
            // (ВИПРАВЛЕНО): Утримуємо стабільний поворот
            visualsTransform.rotation = stableWorldRotation;
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Фаза 2: Повернення
        elapsedTime = 0f;
        while (elapsedTime < halfDuration)
        {
            collisionPivot.localScale = Vector3.Lerp(squashScale, originalScale, elapsedTime / halfDuration);
            // (ВИПРАВЛЕНО): Утримуємо стабільний поворот
            visualsTransform.rotation = stableWorldRotation;
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // 5. Очищення
        collisionPivot.localScale = Vector3.one;
        visualsTransform.SetParent(visualsOriginalParent, true);
        // (ВИПРАВЛЕНО): Відновлюємо стабільний поворот
        visualsTransform.rotation = stableWorldRotation;
        collisionPivot.rotation = Quaternion.identity;
        isImpacting = false;
    }
}

