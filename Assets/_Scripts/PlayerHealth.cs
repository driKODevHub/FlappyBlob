using UnityEngine;

/// <summary>
/// ���� ������'��, ������ �� ���������� ������ ����� ��� ������.
/// ������ �������� PlayerController �� Collider2D �� ��'���.
/// </summary>
[RequireComponent(typeof(PlayerController), typeof(Collider2D))]
public class PlayerHealth : MonoBehaviour
{
    [Header("���������")]
    [Tooltip("������ �������/���������, �� ������� �������� ��� �����.")]
    [SerializeField] private Renderer[] playerRenderers;
    [Tooltip("������ ������� ��������, �� ���������� ��� �����.")]
    [SerializeField] private GameObject deathParticlePrefab;

    // --- ��������� �� ���������� ---
    private PlayerController playerController;
    private Collider2D playerCollider;
    private bool isDead = false;

    private void Awake()
    {
        // ������ ���������� ��� �������������
        playerController = GetComponent<PlayerController>();
        playerCollider = GetComponent<Collider2D>();

        // �������� �� �������, ���� ��������� �� ��������� � ���������
        if (playerRenderers == null || playerRenderers.Length == 0)
        {
            Debug.LogWarning("� PlayerHealth �� ���������� ������� ��������� ������.", this);
        }
    }

    void Update()
    {
        // ������� ������ ��� ������ �������� ��� ����� ������.
        if (Input.GetKeyDown(KeyCode.K))
        {
            SpawnDeathParticles();
        }
    }

    /// <summary>
    /// ������� ����� ����� ������.
    /// </summary>
    public void Die()
    {
        if (isDead) return; // ��������� ���������� �������
        isDead = true;

        // 1. �������� �� ������� ������� ������
        foreach (var renderer in playerRenderers)
        {
            if (renderer != null)
            {
                renderer.enabled = false;
            }
        }

        // 2. �������� ��������� �� ������� �����
        if (playerController != null) playerController.enabled = false;
        if (playerCollider != null) playerCollider.enabled = false;

        // 3. �������� ��������
        SpawnDeathParticles();

        // TODO: � ����������� ��� ����� ������ ����� ��� �������� ��� ����������� ����
        // Invoke(nameof(Respawn), 2f);
    }

    /// <summary>
    /// ������� ��������� ������� �������� ����� � ������� ������.
    /// </summary>
    private void SpawnDeathParticles()
    {
        if (deathParticlePrefab != null)
        {
            // ��������� �������� � ������� ������� ������ � ����������� ��������� �������
            Instantiate(deathParticlePrefab, transform.position, deathParticlePrefab.transform.rotation);
        }
        else
        {
            Debug.LogError("������ �������� ����� (Death Particle Prefab) �� ���������� � ���������!", this);
        }
    }
}

