using System;
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

    // (ВИДАЛЕНО) Event 'OnPaletteChanged' більше не потрібен

    private Camera mainCamera;

    // (ОНОВЛЕНО) Три списки для всіх рендереів
    private List<SpriteRenderer> wallRenderers = new List<SpriteRenderer>();
    private List<SpriteRenderer> obstacleRenderers = new List<SpriteRenderer>();
    private List<SpriteRenderer> paintAndPlayerRenderers = new List<SpriteRenderer>();

    // Назви шарів (щоб уникнути помилок при наборі тексту)
    private const string WALL_LAYER = "Wall";
    private const string OBSTACLES_LAYER = "Obstacles";
    private const string PAINT_LAYER = "Paint";
    private const string PLAYER_LAYER = "Player";


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

    private void Start()
    {
        // Знаходимо всі статичні об'єкти на сцені
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
        SpriteRenderer[] allRenderers = FindObjectsByType<SpriteRenderer>();

        foreach (var renderer in allRenderers)
        {
            // Ми не перевіряємо, чи це клякса, чи ні.
            // Ми просто сортуємо ВСЕ, що знайшли.
            SortAndCacheRenderer(renderer);
        }

        Debug.Log($"PaletteManager: Знайдено та закешовано: {wallRenderers.Count} стін, {obstacleRenderers.Count} перешкод, {paintAndPlayerRenderers.Count} об'єктів фарби/гравця.");
    }

    private T[] FindObjectsByType<T>()
    {
        throw new NotImplementedException();
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
        else if (Camera.main != null) // Додаткова перевірка
        {
            mainCamera = Camera.main;
            mainCamera.backgroundColor = CurrentPalette.WallAndBackgroundColor;
        }

        // 2. Оновлюємо всі закешовані рендерери
        // Ми робимо цикл у зворотному порядку, щоб безпечно видаляти 'null' (знищені) об'єкти
        UpdateRendererList(wallRenderers, CurrentPalette.WallAndBackgroundColor);
        UpdateRendererList(obstacleRenderers, CurrentPalette.ObstacleColor);
        UpdateRendererList(paintAndPlayerRenderers, CurrentPalette.PaintAndPlayerColor);
    }

    /// <summary>
    /// (НОВИЙ МЕТОД)
    /// Проходить по списку, застосовує колір, видаляє знищені об'єкти.
    /// </summary>
    private void UpdateRendererList(List<SpriteRenderer> renderers, Color color)
    {
        for (int i = renderers.Count - 1; i >= 0; i--)
        {
            if (renderers[i] == null)
            {
                renderers.RemoveAt(i); // Об'єкт було знищено, видаляємо
            }
            else
            {
                renderers[i].color = color; // Застосовуємо колір
            }
        }
    }

    /// <summary>
    /// (НОВИЙ МЕТОД)
    /// Додає рендерер у потрібний список (використовується для сортування).
    /// </summary>
    private void SortAndCacheRenderer(SpriteRenderer renderer)
    {
        switch (renderer.sortingLayerName)
        {
            case WALL_LAYER:
                wallRenderers.Add(renderer);
                break;
            case OBSTACLES_LAYER:
                obstacleRenderers.Add(renderer);
                break;
            case PAINT_LAYER:
            case PLAYER_LAYER:
                paintAndPlayerRenderers.Add(renderer);
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
            case PAINT_LAYER:
            case PLAYER_LAYER:
                if (!paintAndPlayerRenderers.Contains(renderer)) paintAndPlayerRenderers.Add(renderer);
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

        // Просто видаляємо з усіх списків (List.Remove безпечно ігнорує, якщо елемента немає)
        wallRenderers.Remove(renderer);
        obstacleRenderers.Remove(renderer);
        paintAndPlayerRenderers.Remove(renderer);
    }

    /// <summary>
    /// (НОВИЙ МЕТОД)
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
            case PAINT_LAYER:
            case PLAYER_LAYER:
                renderer.color = CurrentPalette.PaintAndPlayerColor;
                break;
        }
    }

    [ContextMenu("Force Apply Current Palette")]
    public void ForceApplyPalette()
    {
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

