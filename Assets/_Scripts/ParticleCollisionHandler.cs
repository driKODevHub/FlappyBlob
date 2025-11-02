using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(ParticleSystem))]
public class ParticleCollisionHandler : MonoBehaviour
{
    [Header("Префаби для спавну")]
    [SerializeField] private GameObject splatPrefab;
    [SerializeField] private GameObject hitEffectPrefab;

    private ParticleSystem partSystem;
    private List<ParticleCollisionEvent> collisionEvents;

    void Awake()
    {
        partSystem = GetComponent<ParticleSystem>();
        collisionEvents = new List<ParticleCollisionEvent>();
    }

    private void OnParticleCollision(GameObject other)
    {
        int numCollisionEvents = partSystem.GetCollisionEvents(other, collisionEvents);

        for (int i = 0; i < numCollisionEvents; i++)
        {
            // --- Спавн ефекту "хляпання" ---
            if (hitEffectPrefab != null)
            {
                // Створюємо ефект і розвертаємо його "від стіни"
                Quaternion rotation = Quaternion.LookRotation(collisionEvents[i].normal);
                GameObject hitEffectInstance = Instantiate(hitEffectPrefab, collisionEvents[i].intersection, rotation); // (ЗМІНЕНО)

                // (НОВЕ): Встановлюємо колір партиклів з PaletteManager
                if (PaletteManager.Instance != null && PaletteManager.Instance.CurrentPalette != null)
                {
                    var mainModule = hitEffectInstance.GetComponent<ParticleSystem>().main;
                    mainModule.startColor = PaletteManager.Instance.CurrentPalette.PaintAndPlayerColor;
                }
            }

            // --- Спавн клякси ---
            if (splatPrefab != null)
            {
                // Тепер префаб сам відповідає за свій вигляд (спрайт, поворот).
                // Ми лише створюємо його в потрібному місці зі стандартним поворотом (Quaternion.identity).
                GameObject splatInstance = Instantiate(splatPrefab, collisionEvents[i].intersection, Quaternion.identity);

                // Повідомляємо менеджер про нову кляксу
                if (SplatManager.Instance != null)
                {
                    SplatManager.Instance.AddSplat(splatInstance);
                }
                else
                {
                    Debug.LogError("SplatManager не знайдено на сцені!");
                }
            }
        }
    }
}

