using UnityEngine;

/// <summary>
/// Керує загальним станом гри, зокрема життєвим циклом гравця (респавн).
/// Це Singleton, до якого можна звернутись з будь-якого скрипта.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Посилання на гравця")]
    [Tooltip("Посилання на скрипт PlayerHealth гравця.")]
    [SerializeField] private PlayerHealth playerHealth;
    [Tooltip("Посилання на скрипт PlayerController гравця.")]
    [SerializeField] private PlayerController playerController;

    [Header("Налаштування респавну")]
    [Tooltip("Час в секундах, через який гравець респавниться після смерті.")]
    [SerializeField] private float respawnDelay = 2.0f;

    // --- Внутрішні змінні ---
    private Vector3 spawnPoint; // Позиція, де гравець почав гру

    private void Awake()
    {
        // Налаштування Singleton патерну
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }

        // Перевірка, чи призначені посилання на гравця
        if (playerHealth == null || playerController == null)
        {
            Debug.LogError("GameManager: Посилання на PlayerHealth або PlayerController не встановлено в інспекторі!", this);
        }
    }

    private void Start()
    {
        // Зберігаємо початкову позицію гравця як точку респавну
        if (playerHealth != null)
        {
            spawnPoint = playerHealth.transform.position;
        }
    }

    /// <summary>
    /// **ПУБЛІЧНИЙ МЕТОД**
    /// Запускає процес респавну (викликається з PlayerHealth).
    /// </summary>
    public void StartRespawnProcess()
    {
        // Викликаємо метод RespawnPlayer через 'respawnDelay' секунд
        Invoke(nameof(RespawnPlayer), respawnDelay);
    }

    /// <summary>
    /// Скидає стан гравця і повертає його на точку спавну.
    /// </summary>
    private void RespawnPlayer()
    {
        if (playerHealth == null || playerController == null) return;

        // 1. Викликаємо методи "скидання" на кожному компоненті гравця
        playerHealth.ResetPlayer(spawnPoint);
        playerController.ResetPlayer();
    }
}
