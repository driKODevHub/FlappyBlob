using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(ParticleSystem))]
public class ParticleCollisionHandler : MonoBehaviour
{
    [Header("������� ��� ������")]
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
            // --- ����� ������ "��������" ---
            if (hitEffectPrefab != null)
            {
                // ��������� ����� � ���������� ���� "�� ����"
                Quaternion rotation = Quaternion.LookRotation(collisionEvents[i].normal);
                Instantiate(hitEffectPrefab, collisionEvents[i].intersection, rotation);
            }

            // --- ����� ������ ---
            if (splatPrefab != null)
            {
                // ��������� ������ � ���������� ��������� �� �� Z
                Quaternion randomRotation = Quaternion.Euler(0f, 0f, Random.Range(0f, 360f));
                GameObject splatInstance = Instantiate(splatPrefab, collisionEvents[i].intersection, randomRotation);

                // ����������� �������� ��� ���� ������
                if (SplatManager.Instance != null)
                {
                    SplatManager.Instance.AddSplat(splatInstance);
                    // Debug.Log("������ �������� � ������ �� ���������!");
                }
                else
                {
                    Debug.LogError("SplatManager �� �������� �� ����!");
                }
            }
        }
    }
}

