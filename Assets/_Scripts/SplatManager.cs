using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// ���� ������� ����� �� ����, ��� �������� ������� � �������������.
/// ������� ��������� ������, ���� ��������� ����.
/// </summary>
public class SplatManager : MonoBehaviour
{
    // Singleton ������, ��� �� ������� ����� ���� ����� ���������� � ����-����� ������ ����.
    public static SplatManager Instance { get; private set; }

    [Header("������������ ����������")]
    [Tooltip("����������� ������� �����, �� ������ ��������� �������� �� ����.")]
    [SerializeField] private int maxSplats = 150;

    // ������������� ����� (Queue), ��� ����� �����������, ��� ������ ���������.
    // ������, ��� ������ - ������, ��� �����.
    private Queue<GameObject> splatsQueue = new Queue<GameObject>();

    private void Awake()
    {
        // ��������� Singleton �������
        if (Instance != null && Instance != this)
        {
            // ���� ��������� ��� ����, ������� ��� �������.
            Destroy(gameObject);
        }
        else
        {
            // ���� ���������� �� ����, ������ ��� ��'��� ��������.
            Instance = this;
        }
    }

    /// <summary>
    /// **���˲���� �����**
    /// ���� ���� ������ �� ������� ����������.
    /// ���� ��� ����������, ������� ��������� ������.
    /// </summary>
    /// <param name="splatInstance">������� ��'��� ������, ��� ������� ������.</param>
    public void AddSplat(GameObject splatInstance)
    {
        // ���� ������� ����� ������� ��� ���������� ���, �������� �������� ����.
        while (splatsQueue.Count >= maxSplats)
        {
            // ��������� ��������� ������ � �����
            GameObject oldestSplat = splatsQueue.Dequeue(); // �������� ��������� ������� ������� � �����.
            if (oldestSplat != null)
            {
                Destroy(oldestSplat);
            }
        }

        // ������ ���� ������ � ����� �����
        splatsQueue.Enqueue(splatInstance);
    }
}

