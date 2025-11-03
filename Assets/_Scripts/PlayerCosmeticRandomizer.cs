using UnityEngine;
using System.Collections.Generic; // Потрібно для List

/// <summary>
/// (НОВИЙ) Керує всією процедурною кастомізацією візуалу гравця.
/// Автоматично рандомить спрайти, позиції та масштаб частин тіла
/// щоразу при виклику 'RandomizeCosmetics()'.
/// 
/// **ВАЖЛИВО:** Цей скрипт має висіти на 'Visual' GameObject гравця.
/// </summary>
public class PlayerCosmeticRandomizer : MonoBehaviour
{
    public static PlayerCosmeticRandomizer Instance { get; private set; }

    [Header("Частини Тіла (Рендерери)")]
    [Tooltip("SpriteRenderer тіла гравця.")]
    [SerializeField] private SpriteRenderer bodyRenderer;
    [Tooltip("SpriteRenderer рота гравця.")]
    [SerializeField] private SpriteRenderer mouthRenderer;
    [Tooltip("SpriteRenderer білка лівого ока.")]
    [SerializeField] private SpriteRenderer leftEyeBallRenderer;
    [Tooltip("SpriteRenderer зіниці лівого ока.")]
    [SerializeField] private SpriteRenderer leftEyePupilRenderer;
    [Tooltip("SpriteRenderer білка правого ока.")]
    [SerializeField] private SpriteRenderer rightEyeBallRenderer;
    [Tooltip("SpriteRenderer зіниці правого ока.")]
    [SerializeField] private SpriteRenderer rightEyePupilRenderer;

    [Header("Частини Тіла (Трансформи)")]
    [Tooltip("Transform, що керує лівим оком (для позиції/масштабу).")]
    [SerializeField] private Transform leftEyeTransform;
    [Tooltip("Transform, що керує правим оком (для позиції/масштабу).")]
    [SerializeField] private Transform rightEyeTransform;
    [Tooltip("Transform, що керує ротом (для позиції/масштабу).")]
    [SerializeField] private Transform mouthTransform;

    [Header("Доступні Спрайти")]
    [SerializeField] private Sprite[] bodySprites;
    [SerializeField] private Sprite[] mouthSprites;
    [SerializeField] private Sprite[] eyeBallSprites;
    [SerializeField] private Sprite[] eyePupilSprites;

    [Header("Налаштування Рандомізації")]
    [Tooltip("Скільки останніх варіантів пам'ятати, щоб уникнути повторів.")]
    [SerializeField] private int antiRepeatCount = 3;

    [Header("Рандомізація Масштабу")]
    [Tooltip("Мінімальний масштаб (у відсотках від оригіналу).")]
    [SerializeField][Range(0.5f, 1.5f)] private float minScalePercent = 0.9f;
    [Tooltip("Максимальний масштаб (у відсотках від оригіналу).")]
    [SerializeField][Range(0.5f, 1.5f)] private float maxScalePercent = 1.1f;
    [Tooltip("Максимальна асиметрія між очима (у відсотках). 0.15 = 15% різниці.")]
    [SerializeField][Range(0f, 0.5f)] private float maxEyeAsymmetryPercent = 0.15f;

    [Header("Рандомізація Позицій (Локальні)")]
    [Tooltip("Область (прямокутник), в якій може з'явитись ліве око.")]
    [SerializeField] private Rect leftEyePositionBounds = new Rect(-0.5f, 0.2f, 0.3f, 0.3f);
    [Tooltip("Область (прямокутник), в якій може з'явитись праве око.")]
    [SerializeField] private Rect rightEyePositionBounds = new Rect(0.2f, 0.2f, 0.3f, 0.3f);
    [Tooltip("Область (прямокутник), в якій може з'явитись рот.")]
    [SerializeField] private Rect mouthPositionBounds = new Rect(-0.15f, -0.3f, 0.3f, 0.2f);

    // Внутрішні змінні
    private Vector3 originalLeftEyeScale, originalRightEyeScale, originalMouthScale;

    // Хелпери для уникнення повторів
    private RandomSpritePicker bodyPicker, mouthPicker, eyeballPicker, pupilPicker;

    private void Awake()
    {
        // Налаштування Singleton патерну
        if (Instance != null && Instance != this) Destroy(gameObject);
        else Instance = this;

        // Зберігаємо початкові масштаби (вони можуть бути не 1,1,1)
        if (leftEyeTransform) originalLeftEyeScale = leftEyeTransform.localScale;
        if (rightEyeTransform) originalRightEyeScale = rightEyeTransform.localScale;
        if (mouthTransform) originalMouthScale = mouthTransform.localScale;

        // Ініціалізуємо наші хелпери для рандомізації спрайтів
        bodyPicker = new RandomSpritePicker(bodyRenderer, bodySprites, antiRepeatCount);
        mouthPicker = new RandomSpritePicker(mouthRenderer, mouthSprites, antiRepeatCount);
        eyeballPicker = new RandomSpritePicker(null, eyeBallSprites, antiRepeatCount); // Один пікер на обидва ока
        pupilPicker = new RandomSpritePicker(null, eyePupilSprites, antiRepeatCount); // Один пікер на обидві зіниці
    }

    /// <summary>
    /// **ПУБЛІЧНИЙ МЕТОД**
    /// Запускає повний процес рандомізації.
    /// Викликається з PlayerHealth.ResetPlayer().
    /// </summary>
    [ContextMenu("Randomize Cosmetics")] // Додає кнопку в інспектор для тестування
    public void RandomizeCosmetics()
    {
        RandomizeSprites();
        RandomizeTransforms();
    }

    /// <summary>
    /// Встановлює нові випадкові спрайти для всіх частин тіла.
    /// (ОНОВЛЕНО): Тепер перевіряє 'CanRandomize' перед викликом.
    /// </summary>
    private void RandomizeSprites()
    {
        // (ОНОВЛЕНО): Додано перевірки, щоб не чіпати спрайт, 
        // якщо масив 'null' або має менше 2 елементів.
        if (bodyPicker.CanRandomize)
        {
            bodyPicker.ApplySprite();
        }

        if (mouthPicker.CanRandomize)
        {
            mouthPicker.ApplySprite();
        }

        // Використовуємо той самий спрайт для обох очей (але з одного пікера)
        if (eyeballPicker.CanRandomize)
        {
            Sprite newEyeball = eyeballPicker.GetRandomSprite();
            if (leftEyeBallRenderer) leftEyeBallRenderer.sprite = newEyeball;
            if (rightEyeBallRenderer) rightEyeBallRenderer.sprite = newEyeball;
        }

        if (pupilPicker.CanRandomize)
        {
            Sprite newPupil = pupilPicker.GetRandomSprite();
            if (leftEyePupilRenderer) leftEyePupilRenderer.sprite = newPupil;
            if (rightEyePupilRenderer) rightEyePupilRenderer.sprite = newPupil;
        }
    }

    /// <summary>
    /// Встановлює нові випадкові позиції та масштаби для очей і рота.
    /// </summary>
    private void RandomizeTransforms()
    {
        // --- Рот ---
        if (mouthTransform)
        {
            // Масштаб
            float mouthScaleMod = Random.Range(minScalePercent, maxScalePercent);
            mouthTransform.localScale = originalMouthScale * mouthScaleMod;
            // Позиція
            mouthTransform.localPosition = GetRandomPointInRect(mouthPositionBounds);
        }

        // --- Очі (з асиметрією) ---

        // 1. Генеруємо "базові" значення для обох очей
        float baseScaleMod = Random.Range(minScalePercent, maxScalePercent);

        // 2. Ліве око
        if (leftEyeTransform)
        {
            // Додаємо легку асиметрію до масштабу
            float leftScaleMod = baseScaleMod + Random.Range(-maxEyeAsymmetryPercent, maxEyeAsymmetryPercent);
            leftEyeTransform.localScale = originalLeftEyeScale * leftScaleMod;
            // Позиція
            leftEyeTransform.localPosition = GetRandomPointInRect(leftEyePositionBounds);
        }

        // 3. Праве око
        if (rightEyeTransform)
        {
            // Додаємо легку асиметрію до масштабу (іншу)
            float rightScaleMod = baseScaleMod + Random.Range(-maxEyeAsymmetryPercent, maxEyeAsymmetryPercent);
            rightEyeTransform.localScale = originalRightEyeScale * rightScaleMod;
            // Позиція
            rightEyeTransform.localPosition = GetRandomPointInRect(rightEyePositionBounds);
        }
    }

    /// <summary>
    /// Повертає випадкову Vector2 точку всередині заданого Rect.
    /// </summary>
    private Vector2 GetRandomPointInRect(Rect rect)
    {
        return new Vector2(
            Random.Range(rect.xMin, rect.xMax),
            Random.Range(rect.yMin, rect.yMax)
        );
    }

    /// <summary>
    /// Малює Ґізмос в редакторі для візуального налаштування областей.
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        DrawGizmoRect(leftEyePositionBounds);
        DrawGizmoRect(rightEyePositionBounds);

        Gizmos.color = Color.yellow;
        DrawGizmoRect(mouthPositionBounds);
    }

    /// <summary>
    /// Хелпер-метод для малювання Rect-ґізмо у локальних координатах.
    /// </summary>
    private void DrawGizmoRect(Rect rect)
    {
        Vector3 center = transform.TransformPoint(new Vector3(rect.center.x, rect.center.y, 0));
        Vector3 size = transform.TransformVector(new Vector3(rect.size.x, rect.size.y, 0));
        Gizmos.DrawWireCube(center, size);
    }
}

/// <summary>
/// (Внутрішній Хелпер-Клас)
/// Керує вибором випадкового спрайту з масиву, уникаючи N останніх виборів.
/// </summary>
internal class RandomSpritePicker
{
    private SpriteRenderer targetRenderer;
    private Sprite[] sprites;
    private int antiRepeatCount;
    private List<int> recentIndices;

    // (ОНОВЛЕНО): Зробили цю змінну публічною (але тільки для читання ззовні)
    public bool CanRandomize { get; private set; }

    public RandomSpritePicker(SpriteRenderer renderer, Sprite[] spriteArray, int repeatCount)
    {
        targetRenderer = renderer;
        sprites = spriteArray;
        antiRepeatCount = repeatCount;
        recentIndices = new List<int>();

        // "Безпечна перевірка", яку ви просили
        // (ОНОВЛЕНО): Встановлюємо публічну змінну
        CanRandomize = (sprites != null && sprites.Length > 1);
    }

    /// <summary>
    /// Отримує випадковий спрайт з логікою анти-повтору.
    /// **ВАЖЛИВО: Викликайте, тільки якщо 'CanRandomize' = true.**
    /// </summary>
    public Sprite GetRandomSprite()
    {
        // (ОНОВЛЕНО): Ми прибрали перевірку '!canRandomize' звідси, 
        // бо тепер її робить головний клас ПЕРЕД викликом цього методу.
        // Це робить код чистішим.

        // "Безпечна перевірка" на випадок, якщо спрайтів менше, ніж 'antiRepeatCount'
        int maxPossibleIndices = sprites.Length;
        int maxAntiRepeat = Mathf.Min(antiRepeatCount, maxPossibleIndices - 1); // Не може бути більше, ніж (кількість - 1)

        if (maxAntiRepeat <= 0)
        {
            // Недостатньо спрайтів для логіки, просто рандомимо
            return sprites[Random.Range(0, sprites.Length)];
        }

        // Логіка анти-повтору
        int randomIndex;
        int attempts = 0; // Запобіжник від нескінченного циклу
        do
        {
            randomIndex = Random.Range(0, sprites.Length);
            attempts++;
            if (attempts > 20) break; // Якщо щось пішло не так, просто беремо будь-який
        }
        while (recentIndices.Contains(randomIndex));

        // Оновлюємо список останніх
        recentIndices.Add(randomIndex);
        if (recentIndices.Count > maxAntiRepeat)
        {
            recentIndices.RemoveAt(0); // Видаляємо найстаріший
        }

        return sprites[randomIndex];
    }

    /// <summary>
    /// Застосовує випадковий спрайт до цільового рендерера (якщо він є).
    /// **ВАЖЛИВО: Викликайте, тільки якщо 'CanRandomize' = true.**
    /// </summary>
    public void ApplySprite()
    {
        if (targetRenderer != null)
        {
            targetRenderer.sprite = GetRandomSprite();
        }
    }
}

