using UnityEngine;

/// <summary>
/// ScriptableObject для зберігання набору кольорів.
/// Створити новий асет палітри: Assets -> Create -> Palettes -> New Color Palette
/// </summary>
[CreateAssetMenu(fileName = "NewColorPalette", menuName = "Palettes/New Color Palette")]
public class ColorPaletteSO : ScriptableObject
{
    [Header("Налаштування кольорів")]

    [Tooltip("Колір для стін (Sorting Layer 'Wall') та фону камери.")]
    public Color WallAndBackgroundColor = Color.gray;

    [Tooltip("Колір для гравця (Sorting Layer 'Player'), клякс та партиклів (Sorting Layer 'Paint').")]
    public Color PaintAndPlayerColor = Color.yellow;

    [Tooltip("Колір для перешкод (Sorting Layer 'Obstacles').")]
    public Color ObstacleColor = Color.red;
}
