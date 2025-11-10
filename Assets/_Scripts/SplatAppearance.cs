using System.Collections;
using UnityEngine;

/// <summary>
/// Керує візуальною частиною клякси: вибирає випадковий спрайт,
/// задає поворот/масштаб та анімує появу/зникнення через шейдер "розчинення".
/// (ОНОВЛЕНО): Тепер бере час зникнення (fadeOutDuration) з SplatManager.
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class SplatAppearance : MonoBehaviour
{
    [Header("Налаштування візуалу")]
    [Tooltip("Список можливих спрайтів для клякси. Один буде обрано випадково.")]
    [SerializeField] private Sprite[] possibleSprites;

    [Header("Рандомний Поворот")]
    [Tooltip("Чи потрібно задавати випадковий поворот при створенні?")]
    [SerializeField] private bool randomizeRotation = true;
    [Tooltip("Мінімальний кут повороту.")]
    [SerializeField] private float minRotation = 0f;
    [Tooltip("Максимальний кут повороту.")]
    [SerializeField] private float maxRotation = 360f;

    [Header("Рандомний Масштаб")]
    [Tooltip("Чи потрібно задавати випадковий масштаб при створенні?")]
    [SerializeField] private bool useRandomScale = true;
    [Tooltip("Мінімальний множник масштабу.")]
    [SerializeField] private float minScale = 1.0f;
    [Tooltip("Максимальний множник масштабу.")]
    [SerializeField] private float maxScale = 1.0f;


    [Header("Налаштування ефектів")]
    [Tooltip("Чи використовувати ефект появи через шейдер?")]
    [SerializeField] private bool useAppearEffect = true;
    [Tooltip("Час в секундах, за який клякса повністю з'явиться.")]
    [SerializeField] private float appearDuration = 0.5f;
    [Tooltip("Матеріал, який буде встановлено *після* завершення ефекту появи (для оптимізації).")]
    [SerializeField] private Material finalMaterial;

    // (ВИДАЛЕНО): 'fadeOutDuration' тепер береться з SplatManager
    // [SerializeField] private float fadeOutDuration = 0.3f; 

    // --- Внутрішні змінні ---
    private SpriteRenderer spriteRenderer;
    private Material originalFadeMaterialAsset; // (НОВЕ) Зберігаємо оригінальний матеріал
    private Material materialInstance; // Інстанс для АНІМАЦІЇ
    private static readonly int FadePropertyID = Shader.PropertyToID("_Fade");
    private bool isFadingOut = false; // (НОВЕ) Запобіжник від подвійного виклику

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        // --- 1. Рандомізація спрайту ---
        if (spriteRenderer != null && possibleSprites != null && possibleSprites.Length > 0)
        {
            int randomIndex = Random.Range(0, possibleSprites.Length);
            spriteRenderer.sprite = possibleSprites[randomIndex];
        }
        else if (possibleSprites == null || possibleSprites.Length == 0)
        {
            Debug.LogWarning("У SplatAppearance не призначено спрайтів для рандомізації.", this);
        }

        // --- 2. Рандомізація повороту ---
        if (randomizeRotation)
        {
            transform.rotation = Quaternion.Euler(0f, 0f, Random.Range(minRotation, maxRotation));
        }

        // --- 3. Рандомізація масштабу ---
        if (useRandomScale)
        {
            float scale = Random.Range(minScale, maxScale);
            transform.localScale = new Vector3(scale, scale, 1f);
        }

        // --- 4. Підготовка до ефекту появи --- (ОНОВЛЕНО)
        if (useAppearEffect)
        {
            // (ОНОВЛЕНО): Зберігаємо посилання на оригінальний матеріал (який має шейдер)
            originalFadeMaterialAsset = spriteRenderer.material;
            // Створюємо унікальну копію (інстанс) цього матеріалу для анімації "розчинення"
            materialInstance = new Material(originalFadeMaterialAsset);
            // Призначаємо інстанс рендереру
            spriteRenderer.material = materialInstance;
        }
        else if (finalMaterial != null)
        {
            // Якщо ефект не використовується, одразу ставимо фінальний матеріал
            spriteRenderer.material = finalMaterial;
        }
    }

    private void Start()
    {
        // --- (НОВИЙ РЯДОК) Реєструємо цю кляксу в менеджері ---
        if (PaletteManager.Instance != null)
        {
            PaletteManager.Instance.RegisterRenderer(spriteRenderer);
        }

        // --- 5. Запуск корутини появи ---
        // (ОНОВЛЕНО): Перевіряємо originalFadeMaterialAsset
        if (useAppearEffect && materialInstance != null && originalFadeMaterialAsset != null)
        {
            StartCoroutine(AppearCoroutine());
        }
    }

    private void OnDestroy()
    {
        // Коли кляксу знищують, кажемо менеджеру викреслити її зі списку
        if (PaletteManager.Instance != null)
        {
            PaletteManager.Instance.UnregisterRenderer(spriteRenderer);
        }
    }

    /// <summary>
    /// Корутина, що плавно змінює значення "_Fade" в матеріалі від 0 до 1 для появи.
    /// **Після завершення замінює матеріал на 'finalMaterial'.**
    /// </summary>
    private IEnumerator AppearCoroutine()
    {
        float elapsedTime = 0f;
        while (elapsedTime < appearDuration)
        {
            elapsedTime += Time.deltaTime;
            float fadeValue = Mathf.Lerp(0f, 1f, elapsedTime / appearDuration);
            // Анімуємо наш інстанс матеріалу
            materialInstance.SetFloat(FadePropertyID, fadeValue);
            yield return null;
        }

        // Гарантуємо, що клякса повністю видима
        materialInstance.SetFloat(FadePropertyID, 1f);

        // --- 6. Заміна матеріалу ---
        if (finalMaterial != null)
        {
            spriteRenderer.material = finalMaterial;
        }

        // (ОНОВЛЕНО): Ми більше не потребуємо інстанс для появи, 
        // але 'materialInstance' автоматично очиститься, 
        // оскільки ми зберегли 'originalFadeMaterialAsset'
    }

    /// <summary>
    /// **ПУБЛІЧНИЙ МЕТОД**
    /// Запускає корутину зникнення (fade-out) і подальшого знищення об'єкта.
    /// </summary>
    public void StartFadeOutAndDestroy()
    {
        if (isFadingOut) return; // Вже зникаємо
        isFadingOut = true;

        // Перевіряємо, чи є в нас матеріал для "зникнення"
        if (originalFadeMaterialAsset != null && SplatManager.Instance != null)
        {
            StartCoroutine(FadeOutCoroutine());
        }
        else
        {
            // Якщо матеріалу немає, просто знищуємо об'єкт
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Корутина, що плавно змінює значення "_Fade" від 1 до 0 для зникнення.
    /// (ОНОВЛЕНО): Бере час зникнення з SplatManager.
    /// </summary>
    private IEnumerator FadeOutCoroutine()
    {
        // 1. Створюємо НОВИЙ інстанс матеріалу для зникнення
        // (Ми не можемо використати 'finalMaterial', бо в ньому немає шейдера _Fade)
        Material fadeOutInstance = new Material(originalFadeMaterialAsset);

        // 2. Встановлюємо його повністю видимим
        fadeOutInstance.SetFloat(FadePropertyID, 1f);

        // 3. Призначаємо цей матеріал рендереру (замінюючи 'finalMaterial')
        spriteRenderer.material = fadeOutInstance;

        // 4. Анімуємо зникнення
        float elapsedTime = 0f;
        // (ОНОВЛЕНО): Використовуємо час з SplatManager
        float duration = SplatManager.Instance.splatFadeOutDuration;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float fadeValue = Mathf.Lerp(1f, 0f, elapsedTime / duration);
            fadeOutInstance.SetFloat(FadePropertyID, fadeValue);
            yield return null;
        }

        // 5. Гарантуємо повне зникнення
        fadeOutInstance.SetFloat(FadePropertyID, 0f);

        // 6. Знищуємо сам об'єкт клякси
        Destroy(gameObject);
    }
}