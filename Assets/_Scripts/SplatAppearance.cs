using System.Collections;
using UnityEngine;

/// <summary>
/// Керує візуальною частиною клякси: вибирає випадковий спрайт,
/// задає випадковий поворот і масштаб,
/// та анімує плавну появу через шейдер "розчинення".
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class SplatAppearance : MonoBehaviour
{
    [Header("Налаштування візуалу")]
    [Tooltip("Список можливих спрайтів для клякси. Один буде обрано випадково.")]
    [SerializeField] private Sprite[] possibleSprites;

    [Header("Налаштування повороту")]
    [Tooltip("Чи потрібно задавати випадковий поворот при створенні?")]
    [SerializeField] private bool useRandomRotation = true;
    [Tooltip("Мінімальний кут повороту (в градусах).")]
    [SerializeField] private float minRotation = 0f;
    [Tooltip("Максимальний кут повороту (в градусах).")]
    [SerializeField] private float maxRotation = 360f;

    [Header("Налаштування масштабу")]
    [Tooltip("Чи потрібно задавати випадковий масштаб при створенні?")]
    [SerializeField] private bool useRandomScale = true;
    [Tooltip("Мінімальний множник масштабу.")]
    [SerializeField] private float minScale = 0.8f;
    [Tooltip("Максимальний множник масштабу.")]
    [SerializeField] private float maxScale = 1.2f;

    [Header("Налаштування ефекту появи")]
    [Tooltip("Чи використовувати ефект появи через шейдер?")]
    [SerializeField] private bool useAppearEffect = true;
    [Tooltip("Час в секундах, за який клякса повністю з'явиться.")]
    [SerializeField] private float appearDuration = 0.5f;

    // --- Внутрішні змінні ---
    private Material materialInstance;
    private static readonly int FadePropertyID = Shader.PropertyToID("_Fade");

    private void Awake()
    {
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();

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
        if (useRandomRotation)
        {
            transform.rotation = Quaternion.Euler(0f, 0f, Random.Range(minRotation, maxRotation));
        }

        // --- 3. Рандомізація масштабу (НОВЕ) ---
        if (useRandomScale)
        {
            float randomScale = Random.Range(minScale, maxScale);
            // Застосовуємо однаковий масштаб по X та Y, щоб зберегти пропорції
            transform.localScale = new Vector3(randomScale, randomScale, transform.localScale.z);
        }

        // --- 4. Підготовка до ефекту появи ---
        if (useAppearEffect)
        {
            // Створюємо унікальну копію матеріалу для анімації
            // (Важливо: це працює, лише якщо шейдер підтримує GPU Instancing
            // або якщо це не стандартний Sprite-Default матеріал)
            materialInstance = spriteRenderer.material;
        }
    }

    private void Start()
    {
        // --- 5. Запуск корутини появи ---
        if (useAppearEffect && materialInstance != null)
        {
            StartCoroutine(AppearCoroutine());
        }
        else if (useAppearEffect && materialInstance == null)
        {
            Debug.LogError("Ефект появи ввімкнено, але матеріал не вдалося отримати. " +
                           "Переконайтесь, що на SpriteRenderer висить унікальний матеріал з шейдером, що має властивість '_Fade'.", this);
        }
    }

    /// <summary>
    /// Корутина, що плавно змінює значення "_Fade" в матеріалі від 0 до 1 для появи.
    /// </summary>
    private IEnumerator AppearCoroutine()
    {
        float elapsedTime = 0f;
        while (elapsedTime < appearDuration)
        {
            elapsedTime += Time.deltaTime;
            // Інтерполяція від 0 (невидимий) до 1 (видимий)
            float fadeValue = Mathf.Lerp(0f, 1f, elapsedTime / appearDuration);
            materialInstance.SetFloat(FadePropertyID, fadeValue);
            yield return null;
        }
        // Гарантуємо, що в кінці клякса буде повністю видимою (значення 1)
        materialInstance.SetFloat(FadePropertyID, 1f);
    }
}
