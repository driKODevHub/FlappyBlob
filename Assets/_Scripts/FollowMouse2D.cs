using UnityEngine;

/// <summary>
/// Змушує цей GameObject слідувати за курсором миші у 2D-світі.
/// </summary>
public class FollowMouse2D : MonoBehaviour
{
    private Camera mainCamera;
    private float objectZCoordinate;

    void Start()
    {
        // Отримуємо посилання на головну камеру
        mainCamera = Camera.main;

        // Зберігаємо початкову Z-координату об'єкта.
        // Це важливо, щоб об'єкт залишався на своїй 2D-площині.
        objectZCoordinate = transform.position.z;
    }

    void Update()
    {
        // 1. Отримуємо позицію миші в екранних координатах (пікселях)
        Vector3 mouseScreenPosition = Input.mousePosition;

        // 2. Конвертуємо екранні координати у світові координати
        //    Для ортографічної камери (стандарт для 2D) вхідна 'z' не має значення,
        //    але вихідна 'z' буде дорівнювати z-позиції камери.
        Vector3 mouseWorldPosition = mainCamera.ScreenToWorldPoint(mouseScreenPosition);

        // 3. Оновлюємо позицію об'єкта.
        //    Ми використовуємо X та Y з позиції миші, 
        //    але ЗБЕРІГАЄМО оригінальну Z-координату об'єкта.
        transform.position = new Vector3(mouseWorldPosition.x, mouseWorldPosition.y, objectZCoordinate);
    }
}   