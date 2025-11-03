using UnityEngine;

/// <summary>
/// (НОВИЙ) Універсальний скрипт-хелпер.
/// Автоматично встановлює вказаний 'SpriteMaskInteraction' 
/// для SpriteRenderer на цьому об'єкті під час 'Awake'.
/// Це дозволяє в редакторі бачити об'єкт (None), а в грі 
/// ховати його за маскою (Visible Inside Mask).
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class SpriteMaskSetter : MonoBehaviour
{
    [Header("Налаштування Маски")]
    [Tooltip("Режим взаємодії з маскою, який буде встановлено при запуску гри (в Awake).")]
    [SerializeField] private SpriteMaskInteraction runtimeMaskInteraction = SpriteMaskInteraction.VisibleInsideMask;

    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        // 1. Отримуємо компонент
        spriteRenderer = GetComponent<SpriteRenderer>();

        // 2. Встановлюємо потрібний режим
        spriteRenderer.maskInteraction = runtimeMaskInteraction;

        // 3. (Опційно) Можна навіть вимкнути цей компонент, 
        // щоб він не "висів" в Update (хоча він і так порожній).
        // this.enabled = false; 
    }
}
