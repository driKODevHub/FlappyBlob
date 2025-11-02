using UnityEditor; // Цей скрипт має бути у папці "Editor"
using UnityEngine;

/// <summary>
/// Створює кастомний інспектор для PaletteManager,
/// щоб показувати кольори поточної палітри.
/// </summary>
[CustomEditor(typeof(PaletteManager))]
public class PaletteManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // Малюємо стандартний інспектор (поле 'initialPalette')
        DrawDefaultInspector();

        EditorGUILayout.Space(10); // Відступ

        // Отримуємо посилання на наш PaletteManager
        PaletteManager manager = (PaletteManager)target;

        // Визначаємо, яку палітру показувати (поточну або початкову)
        ColorPaletteSO paletteToShow = manager.CurrentPalette;
        if (paletteToShow == null)
        {
            // Якщо гра не запущена, CurrentPalette буде null.
            // Беремо 'initialPalette' через серіалізоване поле.
            SerializedProperty initialPaletteProp = serializedObject.FindProperty("initialPalette");
            if (initialPaletteProp.objectReferenceValue != null)
            {
                paletteToShow = (ColorPaletteSO)initialPaletteProp.objectReferenceValue;
            }
        }

        // Малюємо кольори, якщо палітра є
        if (paletteToShow != null)
        {
            EditorGUILayout.LabelField("Попередній перегляд палітри:", EditorStyles.boldLabel);

            // Використовуємо GUI.enabled = false, щоб поля були "тільки для читання"
            bool originalEnabled = GUI.enabled;
            GUI.enabled = false;

            EditorGUILayout.ColorField(
                new GUIContent("Wall & Background", "Колір для 'Wall' та фону камери"),
                paletteToShow.WallAndBackgroundColor
                );

            EditorGUILayout.ColorField(
                new GUIContent("Paint & Player", "Колір для 'Paint' та 'Player'"),
                paletteToShow.PaintAndPlayerColor
                );

            EditorGUILayout.ColorField(
                new GUIContent("Obstacles", "Колір для 'Obstacles'"),
                paletteToShow.ObstacleColor
                );

            // Повертаємо GUI у звичайний стан
            GUI.enabled = originalEnabled;
        }
        else
        {
            EditorGUILayout.HelpBox("Призначте 'Initial Palette', щоб побачити попередній перегляд кольорів.", MessageType.Info);
        }
    }
}
