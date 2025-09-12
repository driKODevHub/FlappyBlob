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

    [Header("��������� ������� � �����")]
    [Tooltip("�� ��������������� �������/��� ������, ���� ���� �����?")]
    [SerializeField] private bool useAirInertia = true;

    [Tooltip("���� ����� ������ (0 = ������ �������, ~0.95 = ������ �����������).")]
    [Range(0f, 1f)]
    [SerializeField] private float airDrag = 0.95f;


    [Header("������ �����")]
    [Tooltip("���������� �������� ���������, ��� ���������� ���� ������� �������.")]
    [SerializeField] private float defaultGravityScale = 3f;


    // --- ������� ���� ---
    private float horizontalInput; // ������ �������� ����� �� ���������� (-1, 0, 1)
    private bool jumpInput; // ������ ���������� ��� ���������� �������
    private bool isGameActive = false; // �� �������� ������� ���� ��� (���� ������� �������)

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

        // �������� ��� � ��������� ���������� �� "�����������" ������
        rb.gravityScale = 0f;
        rb.bodyType = RigidbodyType2D.Kinematic; // ������������� �����, �������� �����
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
        // �� ���������� ��������, ���� ��� �� �������� (�� ������� �������).
        if (!isGameActive)
        {
            return;
        }

        // ���� � �������� ��� �� ������, ������������ �������� �������.
        if (Mathf.Abs(horizontalInput) > 0.01f)
        {
            rb.linearVelocity = new Vector2(horizontalInput * moveSpeed, rb.linearVelocity.y);
        }
        // ���� ����� ����, � ����� ������� ��������.
        else if (useAirInertia)
        {
            // ������ ����������� ������������� ��������, ��������� ����� �������/�����.
            // �� ���� ��������� �� � �����, ��� � �� ����.
            // ��� �����������: ����� ������ �������� isGrounded, ��� ������� ��������� ���� � �����.
            float slowedVelocityX = rb.linearVelocity.x * airDrag;
            rb.linearVelocity = new Vector2(slowedVelocityX, rb.linearVelocity.y);
        }
        // ���� ����� ����, � ������� ��������, �� ����������� ������.
        else
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        }
    }

    /// <summary>
    /// ������ �������, ���� ��� ��������� ���.
    /// </summary>
    private void HandleJump()
    {
        if (jumpInput)
        {
            // ���� �� ������ �������, �������� ������ �� ���������.
            if (!isGameActive)
            {
                isGameActive = true;
                rb.bodyType = RigidbodyType2D.Dynamic; // "�����������" ������, ������� ��� ��� �� ���������
                rb.gravityScale = defaultGravityScale; // ������� ���������
            }

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




