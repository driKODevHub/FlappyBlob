using UnityEngine;

/// <summary>
/// Керує загальним станом гри, зокрема життєвим циклом гравця (респавн).
/// (ОНОВЛЕНО): Використовує Singleton-и для доступу до PlayerHealth та PlayerController.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    // --- (ВИДАЛЕНО): Посилання на PlayerHealth та PlayerController ---

    [Header("Налаштування респавну")]
    [Tooltip("Час в секундах, через який гравець респавниться після смерті.")]
    [SerializeField] private float respawnDelay = 2.0f;

    private Vector3 spawnPoint;

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

        // (ВИДАЛЕНО): Перевірка посилань
    }

    private void Start()
    {
        // (ОНОВЛЕНО): Отримуємо позицію через Singleton
        if (PlayerController.Instance != null)
        {
            spawnPoint = PlayerController.Instance.transform.position;
        }
        else
        {
            Debug.LogError("GameManager: Не можу знайти PlayerController.Instance при старті!", this);
        }
    }

    public void StartRespawnProcess()
    {
        Invoke(nameof(RespawnPlayer), respawnDelay);
    }

    private void RespawnPlayer()
    {
        // (ОНОВЛЕНО): Викликаємо методи через Singleton-и
        if (PlayerHealth.Instance != null)
        {
            PlayerHealth.Instance.ResetPlayer(spawnPoint);
        }
        else
        {
            Debug.LogError("GameManager: PlayerHealth.Instance не знайдено! Не можу ресетнути здоров'я.", this);
        }

        if (PlayerController.Instance != null)
        {
            PlayerController.Instance.ResetPlayer();
        }
        else
        {
            Debug.LogError("GameManager: PlayerController.Instance не знайдено! Не можу ресетнути контролер.", this);
        }
    }
}
