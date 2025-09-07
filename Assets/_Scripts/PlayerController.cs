using UnityEngine;

/// <summary>
/// ���� ����� �� ��������� ���� ������.
/// ��� ������ ������ �������� ���������� Rigidbody2D �� ���� � �������� ��'���.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    // --- ���������� �� ��������� ---
    // [SerializeField] �������� ������������� ������� ���� � ��������� Unity,
    // �� � ������� ��������� ��� ������������.
    [Header("��������� �� ����������")]
    [SerializeField] private Rigidbody2D rb; // ��������� ��� ��������� �������

    // --- ������������ ���� ---
    [Header("��������� ����")]
    [Tooltip("�������� ��������������� ���� ������.")]
    [SerializeField] private float moveSpeed = 7f;

    [Tooltip("���� �������-����.")]
    [SerializeField] private float jumpForce = 12f;

    // --- ������� ���� ---
    private float horizontalInput; // ������ �������� ����� �� ���������� (-1, 0, 1)
    private bool jumpInput; // ������ ���������� ��� ���������� �������

    #region Unity Lifecycle Methods

    /// <summary>
    /// ����� Awake ����������� ���� ��� ��� ����������� �������.
    /// �������� ���� ��� ����������� ����������.
    /// </summary>
    private void Awake()
    {
        // ���������� ����������� ������ ��������� Rigidbody2D, ���� ���� �� ���������� ������.
        if (rb == null)
        {
            rb = GetComponent<Rigidbody2D>();
        }
    }

    /// <summary>
    /// ����� Update ����������� ������� �����.
    /// �������� ���� ��� ���������� ����� �� ������ (���������, ����, �������).
    /// </summary>
    private void Update()
    {
        HandleInput();
    }

    /// <summary>
    /// ����� FixedUpdate ����������� � ���������� ��������.
    /// �� ������ ���������� �� ���������� � Rigidbody ��� ������ ��� ��� ����������.
    /// </summary>
    private void FixedUpdate()
    {
        HandleMovement();
        HandleJump();
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// �������� �� ������ ��� �� �����������.
    /// ³����������� ����� ����� ������ ��� �������.
    /// </summary>
    private void HandleInput()
    {
        // Input.GetAxisRaw �� ����� ���������� (-1, 0, 1) ��� ������������,
        // �� ����� �������� ��� �����������.
        horizontalInput = Input.GetAxisRaw("Horizontal");

        // Input.GetKeyDown ��������� ����� � ������ ���������� ������.
        if (Input.GetKeyDown(KeyCode.Space))
        {
            jumpInput = true;
        }
    }

    /// <summary>
    /// ��������� �������������� ��� �� Rigidbody.
    /// </summary>
    private void HandleMovement()
    {
        // �� ������� �������� �� �� X, ��� �������� ������� �������� �� Y (���������, �������).
        // �� ������� ����� �������� ������.
        rb.linearVelocity = new Vector2(horizontalInput * moveSpeed, rb.linearVelocity.y);
    }

    /// <summary>
    /// ������ �������, ���� ��� ��������� ���.
    /// </summary>
    private void HandleJump()
    {
        if (jumpInput)
        {
            // ������ ��������� ����������� ��������. �� ������ ����� �������-���
            // ��������� �� �����, ��������� �� ����, ����� ������� �� ��������.
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);

            // ������ ������ ���� �����. ForceMode2D.Impulse �������� �������� ��� �������.
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);

            // ������� ��������� �����, ��� ������� �� ������������ � ������� ���� FixedUpdate.
            jumpInput = false;
        }
    }

    #endregion
}

