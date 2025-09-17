using System.Collections;
using UnityEngine;

/// <summary>
/// Керує візуальною частиною клякси: вибирає випадковий спрайт,
/// задає поворот та анімує плавну появу через шейдер "розчинення".
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class SplatAppearance : MonoBehaviour
{
    [Header("Налаштування візуалу")]
    [Tooltip("Список можливих спрайтів для клякси. Один буде обрано випадково.")]
    [SerializeField] private Sprite[] possibleSprites;
    [Tooltip("Чи потрібно задавати випадковий поворот при створенні?")]
    [SerializeField] private bool randomizeRotation = true;

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
        if (randomizeRotation)
        {
            transform.rotation = Quaternion.Euler(0f, 0f, Random.Range(0f, 360f));
        }

        // --- 3. Підготовка до ефекту появи ---
        if (useAppearEffect)
        {
            // Створюємо унікальну копію матеріалу для анімації
            materialInstance = spriteRenderer.material;
        }
    }

    private void Start()
    {
        // --- 4. Запуск корутини появи ---
        if (useAppearEffect && materialInstance != null)
        {
            StartCoroutine(AppearCoroutine());
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
            // ВИПРАВЛЕНО: Інтерполяція від 0 (невидимий) до 1 (видимий)
            float fadeValue = Mathf.Lerp(0f, 1f, elapsedTime / appearDuration);
            materialInstance.SetFloat(FadePropertyID, fadeValue);
            yield return null;
        }
        // ВИПРАВЛЕНО: Гарантуємо, що в кінці клякса буде повністю видимою (значення 1)
        materialInstance.SetFloat(FadePropertyID, 1f);
    }
}

