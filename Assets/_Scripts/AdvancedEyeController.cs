using MoreMountains.Feedbacks; // Потрібно для MMFollowTarget
using MoreMountains.Tools;
using System.Collections;
using UnityEngine;

/// <summary>
/// (НОВИЙ) "Мозок" для керування поглядом гравця.
/// Керує компонентами MMFollowTarget на зіницях,
/// перемикаючи їх ціль (Target) між мишкою, точкою "відпочинку"
/// та найближчою точкою інтересу (UnlockPoint).
/// </summary>
public class AdvancedEyeController : MonoBehaviour
{
    [Header("Посилання на 'М'язи' (Зіниці)")]
    [Tooltip("Компонент MMFollowTarget на лівій зіниці.")]
    [SerializeField] private MMFollowTarget leftPupilFollow;
    [Tooltip("Компонент MMFollowTarget на правій зіниці.")]
    [SerializeField] private MMFollowTarget rightPupilFollow;

    [Header("Посилання на 'Цілі' (Targets)")]
    [Tooltip("Transform об'єкта, що слідує за мишкою (той, що має FollowMouse2D).")]
    [SerializeField] private Transform mouseTarget;
    [Tooltip("Transform точки 'відпочинку' (куди дивляться очі прямо перед собою).")]
    [SerializeField] private Transform restingTarget;

    [Header("Налаштування 'Нудьги'")]
    [SerializeField] private float minLookAtMouseTime = 3.0f;
    [SerializeField] private float maxLookAtMouseTime = 7.0f;
    [Space]
    [SerializeField] private float minRestTime = 1.0f;
    [SerializeField] private float maxRestTime = 3.0f;

    [Header("Налаштування 'Інтересу'")]
    [Tooltip("Радіус, в якому гравець помічає 'Unlock Points'.")]
    [SerializeField] private float unlockPointDetectRadius = 5.0f;

    // --- Внутрішні змінні ---
    private enum EyeState { LookingAtMouse, LookingAtInterestPoint, Resting }
    private EyeState currentState;
    private float stateTimer;

    // ВАЖЛИВО: Цей скрипт має висіти на тому ж об'єкті,
    // що й PlayerVisualController та PlayerCosmeticRandomizer,
    // щоб мати доступ до player transform.position
    private Transform playerTransform;

    private void Awake()
    {
        playerTransform = this.transform;
    }

    private void Start()
    {
        if (leftPupilFollow == null || rightPupilFollow == null || mouseTarget == null || restingTarget == null)
        {
            Debug.LogError("AdvancedEyeController: Не всі посилання налаштовані! Скрипт вимикається.", this);
            this.enabled = false;
            return;
        }

        // Починаємо з корутини, яка керує станами
        StartCoroutine(EyeStateMachine());
    }

    /// <summary>
    /// Головна корутина, що працює як state machine для погляду.
    /// </summary>
    private IEnumerator EyeStateMachine()
    {
        // Починаємо зі стану "Відпочинок"
        currentState = EyeState.Resting;
        SetTarget(restingTarget);
        stateTimer = Random.Range(minRestTime, maxRestTime);

        while (true)
        {
            // --- 1. ПРІОРИТЕТНА ПЕРЕВІРКА: ІНТЕРЕС ---
            // Ця перевірка має найвищий пріоритет.
            Transform closestPoint = FindClosestUnlockPoint();

            if (closestPoint != null)
            {
                // Якщо ми знайшли точку інтересу
                if (currentState != EyeState.LookingAtInterestPoint)
                {
                    // Перемикаємось на неї
                    currentState = EyeState.LookingAtInterestPoint;
                    SetTarget(closestPoint);
                }
                // Якщо ми ВЖЕ дивимось на неї, просто продовжуємо
                yield return null; // Чекаємо наступного кадру
                continue; // Починаємо цикл знову (з пріоритетної перевірки)
            }

            // Якщо ми були в стані інтересу, але точка зникла (вийшли з радіусу / підібрали)
            if (currentState == EyeState.LookingAtInterestPoint)
            {
                // Повертаємось до стеження за мишкою
                currentState = EyeState.LookingAtMouse;
                SetTarget(mouseTarget);
                stateTimer = Random.Range(minLookAtMouseTime, maxLookAtMouseTime);
            }

            // --- 2. СТАНДАРТНА ПЕРЕВІРКА: НУДЬГА (Мишка / Відпочинок) ---
            stateTimer -= Time.deltaTime;

            if (stateTimer <= 0)
            {
                if (currentState == EyeState.LookingAtMouse)
                {
                    // Знудились дивитись на мишку, час відпочити
                    currentState = EyeState.Resting;
                    SetTarget(restingTarget);
                    stateTimer = Random.Range(minRestTime, maxRestTime);
                }
                else if (currentState == EyeState.Resting)
                {
                    // Відпочили, повертаємось до мишки
                    currentState = EyeState.LookingAtMouse;
                    SetTarget(mouseTarget);
                    stateTimer = Random.Range(minLookAtMouseTime, maxLookAtMouseTime);
                }
            }

            yield return null; // Чекаємо наступного кадру
        }
    }

    /// <summary>
    /// Встановлює нову ціль для ОБОХ зіниць.
    /// </summary>
    private void SetTarget(Transform newTarget)
    {
        leftPupilFollow.Target = newTarget;
        rightPupilFollow.Target = newTarget;
    }

    /// <summary>
    /// Шукає найближчий АКТИВНИЙ UnlockPoint в межах радіусу.
    /// </summary>
    private Transform FindClosestUnlockPoint()
    {
        if (LevelManager.Instance == null) return null;
        LevelData levelData = LevelManager.Instance.GetCurrentLevelData();

        // Використовуємо новий метод, який ми додали в LevelData
        UnlockPoint[] unlockPoints = levelData?.GetUnlockPoints();
        if (unlockPoints == null || unlockPoints.Length == 0)
        {
            return null;
        }

        Transform closestPoint = null;
        float minSqrDist = unlockPointDetectRadius * unlockPointDetectRadius;
        Vector2 playerPos = playerTransform.position;

        foreach (UnlockPoint point in unlockPoints)
        {
            // Перевіряємо, чи точка активна (не підібрана)
            // (Ми знаємо зі скрипту UnlockPoint.cs, що колайдер вимикається при підборі)
            if (point == null || !point.GetComponent<Collider2D>().enabled)
            {
                continue;
            }

            // Перевіряємо відстань
            float sqrDist = ((Vector2)point.transform.position - playerPos).sqrMagnitude;
            if (sqrDist < minSqrDist)
            {
                minSqrDist = sqrDist;
                closestPoint = point.transform;
            }
        }
        return closestPoint;
    }
}