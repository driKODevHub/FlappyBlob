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
                Instantiate(hitEffectPrefab, collisionEvents[i].intersection, rotation);
            }

            // --- Спавн клякси ---
            if (splatPrefab != null)
            {
                // Створюємо кляксу з випадковим поворотом по осі Z
                Quaternion randomRotation = Quaternion.Euler(0f, 0f, Random.Range(0f, 360f));
                GameObject splatInstance = Instantiate(splatPrefab, collisionEvents[i].intersection, randomRotation);

                // Повідомляємо менеджер про нову кляксу
                if (SplatManager.Instance != null)
                {
                    SplatManager.Instance.AddSplat(splatInstance);
                    // Debug.Log("Кляксу створено і додано до менеджера!");
                }
                else
                {
                    Debug.LogError("SplatManager не знайдено на сцені!");
                }
            }
        }
    }
}

