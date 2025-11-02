using System.Collections;
using UnityEngine;

/// <summary>
/// Керує візуальною частиною клякси: вибирає випадковий спрайт,
/// задає поворот/масштаб та анімує появу через шейдер "розчинення".
/// **Після появи замінює матеріал на постійний (finalMaterial) для оптимізації.**
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


    [Header("Налаштування ефекту появи")]
    [Tooltip("Чи використовувати ефект появи через шейдер?")]
    [SerializeField] private bool useAppearEffect = true;
    [Tooltip("Час в секундах, за який клякса повністю з'явиться.")]
    [SerializeField] private float appearDuration = 0.5f;
    [Tooltip("Матеріал, який буде встановлено *після* завершення ефекту появи (для оптимізації).")]
    [SerializeField] private Material finalMaterial; // (ЗАМІНЕНО)

    // --- Внутрішні змінні ---
    private SpriteRenderer spriteRenderer;
    private Material materialInstance; // Це буде інстанс матеріалу для "розчинення"
    private static readonly int FadePropertyID = Shader.PropertyToID("_Fade");

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
            // Створюємо унікальну копію матеріалу для анімації "розчинення"
            materialInstance = spriteRenderer.material;
        }
        else if (finalMaterial != null)
        {
            // Якщо ефект не використовується, одразу ставимо фінальний матеріал
            spriteRenderer.material = finalMaterial;
        }
    }

    private void Start()
    {
        // --- 5. Запуск корутини появи ---
        if (useAppearEffect && materialInstance != null)
        {
            StartCoroutine(AppearCoroutine());
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

        // --- 6. Заміна матеріалу --- (ОНОВЛЕНО)
        if (finalMaterial != null)
        {
            // Повністю замінюємо матеріал на SpriteRenderer
            // "materialInstance", який ми анімували, буде автоматично знищено, 
            // оскільки він більше не прив'язаний до рендерера.
            spriteRenderer.material = finalMaterial;
        }
    }
}

