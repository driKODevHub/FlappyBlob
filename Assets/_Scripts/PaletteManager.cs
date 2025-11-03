using System;
using System.Collections; // (ДОДАНО): Потрібно для корутин
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Керує активною палітрою кольорів у грі.
/// (ОНОВЛЕНО): Повністю автоматично!
/// 1. Знаходить всі рендерери при старті.
/// 2. Дозволяє динамічним об'єктам (як клякси) реєструвати себе.
/// </summary>
public class PaletteManager : MonoBehaviour
{
    public static PaletteManager Instance { get; private set; }

    [Header("Активна палітра")]
    [Tooltip("Палітра, яка буде завантажена при старті сцени.")]
    [SerializeField] private ColorPaletteSO initialPalette;

    public ColorPaletteSO CurrentPalette { get; private set; }

    private Camera mainCamera;

    // (ОНОВЛЕНО): Перейменовано
    private List<SpriteRenderer> wallRenderers = new List<SpriteRenderer>();
    private List<SpriteRenderer> obstacleRenderers = new List<SpriteRenderer>();
    private List<SpriteRenderer> paintRenderers = new List<SpriteRenderer>();

    // Назви шарів (щоб уникнути помилок при наборі тексту)
    private const string WALL_LAYER = "Wall";
    private const string OBSTACLES_LAYER = "Obstacles";
    private const string PAINT_LAYER = "Paint";
    // (ВИДАЛЕНО): Player layer більше не потрібен
    // private const string PLAYER_LAYER = "Player";


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }

        mainCamera = Camera.main;
    }

    // (ОНОВЛЕНО): Використовуємо корутину для вирішення проблем з порядком запуску
    private void Start()
    {
        // Запускаємо сканування ТІЛЬКИ ЧЕРЕЗ ОДИН КАДР.
        // Це дає LevelManager час заспавнити початковий рівень у своєму Start().
        StartCoroutine(DelayedScanAndApplyPalette());
    }

    /// <summary>
    /// Чекає один кадр, сканує сцену і застосовує палітру.
    /// </summary>
    private IEnumerator DelayedScanAndApplyPalette()
    {
        // Чекаємо кінця кадру, де всі Start() вже мали виконатись
        yield return null;

        // Тепер скануємо сцену, коли рівень вже гарантовано заспавнений
        FindAndCacheAllRenderers();

        if (initialPalette != null)
        {
            ApplyPalette(initialPalette);
        }
        else
        {
            Debug.LogError("PaletteManager: 'Initial Palette' не призначено!", this);
        }
    }

    /// <summary>
    /// Знаходить АБСОЛЮТНО всі SpriteRenderer на сцені ОДИН РАЗ при старті
    /// і сортує їх у відповідні списки.
    /// </summary>
    private void FindAndCacheAllRenderers()
    {
        // (ОНОВЛЕНО): Додаємо очищення списків про всяк випадок,
        // якщо цей метод колись буде викликано повторно.
        wallRenderers.Clear();
        obstacleRenderers.Clear();
        paintRenderers.Clear();

        SpriteRenderer[] allRenderers = FindObjectsByType<SpriteRenderer>(FindObjectsSortMode.None);

        foreach (var renderer in allRenderers)
        {
            SortAndCacheRenderer(renderer);
        }

        // (ОНОВЛЕНО): Оновлений Debug.Log
        Debug.Log($"PaletteManager: Знайдено та закешовано: {wallRenderers.Count} стін, {obstacleRenderers.Count} перешкод, {paintRenderers.Count} об'єктів фарби.");
    }

    /// <summary>
    /// Встановлює нову палітру як активну і оновлює ВСІ закешовані рендерери.
    /// </summary>
    public void ApplyPalette(ColorPaletteSO newPalette)
    {
        if (newPalette == null)
        {
            Debug.LogError("PaletteManager: Спроба застосувати null палітру.", this);
            return;
        }

        CurrentPalette = newPalette;

        // 1. Оновлюємо фон камери
        if (mainCamera != null)
        {
            mainCamera.backgroundColor = CurrentPalette.WallAndBackgroundColor;
        }
        else if (Camera.main != null)
        {
            mainCamera = Camera.main;
            mainCamera.backgroundColor = CurrentPalette.WallAndBackgroundColor;
        }

        // 2. Оновлюємо всі закешовані рендерери
        UpdateRendererList(wallRenderers, CurrentPalette.WallAndBackgroundColor);
        UpdateRendererList(obstacleRenderers, CurrentPalette.ObstacleColor);
        // (ОНОВЛЕНО): Використовуємо новий список
        UpdateRendererList(paintRenderers, CurrentPalette.PaintAndPlayerColor);
    }

    /// <summary>
    /// Проходить по списку, застосовує колір, видаляє знищені об'єкти.
    /// </summary>
    private void UpdateRendererList(List<SpriteRenderer> renderers, Color color)
    {
        for (int i = renderers.Count - 1; i >= 0; i--)
        {
            if (renderers[i] == null)
            {
                renderers.RemoveAt(i);
            }
            else
            {
                renderers[i].color = color;
            }
        }
    }

    /// <summary>
    /// Додає рендерер у потрібний список (використовується для сортування).
    /// </summary>
    private void SortAndCacheRenderer(SpriteRenderer renderer)
    {
        if (renderer == null) return;

        switch (renderer.sortingLayerName)
        {
            case WALL_LAYER:
                wallRenderers.Add(renderer);
                break;
            case OBSTACLES_LAYER:
                obstacleRenderers.Add(renderer);
                break;
            // (ОНОВЛЕНО): Об'єднано
            case PAINT_LAYER:
                paintRenderers.Add(renderer);
                break;
        }
    }

    // --- ПУБЛІЧНІ МЕТОДИ ДЛЯ ДИНАМІЧНИХ ОБ'ЄКТІВ ---

    /// <summary>
    /// **ПУБЛІЧНИЙ МЕТОД**
    /// Дозволяє новим об'єктам (кляксам) реєструвати себе.
    /// </summary>
    public void RegisterRenderer(SpriteRenderer renderer)
    {
        if (renderer == null) return;

        // 1. Додаємо у список
        string layerName = renderer.sortingLayerName;
        switch (layerName)
        {
            case WALL_LAYER:
                if (!wallRenderers.Contains(renderer)) wallRenderers.Add(renderer);
                break;
            case OBSTACLES_LAYER:
                if (!obstacleRenderers.Contains(renderer)) obstacleRenderers.Add(renderer);
                break;
            // (ОНОВЛЕНО): Об'єднано
            case PAINT_LAYER:
                if (!paintRenderers.Contains(renderer)) paintRenderers.Add(renderer);
                break;
        }

        // 2. Негайно застосовуємо колір
        if (CurrentPalette != null)
        {
            ApplyColorToRenderer(renderer, layerName);
        }
    }

    /// <summary>
    /// **ПУБЛІЧНИЙ МЕТОД**
    /// Дозволяє об'єктам видаляти себе зі списків при знищенні.
    /// </summary>
    public void UnregisterRenderer(SpriteRenderer renderer)
    {
        if (renderer == null) return;

        wallRenderers.Remove(renderer);
        obstacleRenderers.Remove(renderer);
        // (ОНОВЛЕНО): Використовуємо новий список
        paintRenderers.Remove(renderer);
    }

    /// <summary>
    /// Приватний хелпер для миттєвого фарбування одного рендерера.
    /// </summary>
    private void ApplyColorToRenderer(SpriteRenderer renderer, string layerName)
    {
        switch (layerName)
        {
            case WALL_LAYER:
                renderer.color = CurrentPalette.WallAndBackgroundColor;
                break;
            case OBSTACLES_LAYER:
                renderer.color = CurrentPalette.ObstacleColor;
                break;
            // (ОНОВЛЕНО): Об'єднано
            case PAINT_LAYER:
                renderer.color = CurrentPalette.PaintAndPlayerColor;
                break;
        }
    }

    [ContextMenu("Force Apply Current Palette")]
    public void ForceApplyPalette()
    {
        // (ОНОВЛЕНО): Цей метод тепер також має пересканувати сцену,
        // на випадок, якщо рівень завантажився ПІСЛЯ запуску гри.
        Debug.Log("Примусове сканування та застосування палітри...");
        FindAndCacheAllRenderers();

        if (CurrentPalette != null)
        {
            ApplyPalette(CurrentPalette);
        }
        else if (initialPalette != null)
        {
            ApplyPalette(initialPalette);
        }
        else
        {
            Debug.LogError("PaletteManager: Немає палітри для застосування.", this);
        }
    }
}

