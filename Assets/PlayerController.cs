using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float _maxSpeed = 8f;
    [SerializeField] private float _acceleration = 50f;
    [SerializeField] private float _deceleration = 60f;
    [SerializeField] private float _airControlMultiplier = 0.5f;
    private SpriteRenderer _sR;
    private Rigidbody2D _rb;

    [Header("Input")]
    public bool IsKeyboard;
    private Vector2 movement;
    private bool jumpPressed = false;
    private Camera mainCamera;

    [Header("Jump")]
    public Transform groundCheck;
    [SerializeField] private float _jumpForce = 12f;
    [SerializeField] private float _coyoteTime = 0.15f;
    [SerializeField] private float _jumpBufferTime = 0.1f;
    private float _coyoteTimeCounter;
    private float _jumpBufferCounter;

    private void Awake()
    {
        mainCamera = Camera.main;
        _rb = GetComponent<Rigidbody2D>();
        _sR = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        InitializePlayer();
    }

    private void Update()
    {
        UpdateCoyoteTimeCounter();
        UpdateJumpBufferCounter();
    }

    private void FixedUpdate()
    {
        HandleMovement();
        HandleJump();
    }

    private void InitializePlayer()
    {
        Debug.Log("PlayerController Start");
        string controlScheme = GetComponent<PlayerInput>().currentControlScheme;
        Debug.Log("Control Scheme: " + controlScheme);

        IsKeyboard = controlScheme == "Keyboard&Mouse";
        Debug.Log("IsKeyboard: " + IsKeyboard);
    }

    private void UpdateCoyoteTimeCounter()
    {
        if (IsGrounded())
        {
            _coyoteTimeCounter = _coyoteTime;
        }
        else
        {
            _coyoteTimeCounter -= Time.deltaTime;
        }
    }

    private void UpdateJumpBufferCounter()
    {
        if (jumpPressed)
        {
            _jumpBufferCounter = _jumpBufferTime;
        }
        else
        {
            _jumpBufferCounter -= Time.deltaTime;
        }
    }

    private void HandleMovement()
    {
        float targetSpeed = movement.x * _maxSpeed;
        float speedDiff = targetSpeed - _rb.linearVelocity.x;

        float accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? _acceleration : _deceleration;

        if (!IsGrounded())
        {
            accelRate *= _airControlMultiplier;
        }

        float movementForce = speedDiff * accelRate;

        _rb.AddForce(new Vector2(movementForce, 0));

        if (movement.x > 0.1f) _sR.flipX = false;
        else if (movement.x < -0.1f) _sR.flipX = true;

        if (Mathf.Abs(_rb.linearVelocity.x) > _maxSpeed)
        {
            _rb.linearVelocity = new Vector2(Mathf.Sign(_rb.linearVelocity.x) * _maxSpeed, _rb.linearVelocity.y);
        }
    }

    private void HandleJump()
    {
        if (_jumpBufferCounter > 0 && _coyoteTimeCounter > 0)
        {
            _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, _jumpForce);
            _jumpBufferCounter = 0f;
            Debug.Log("Jumped with coyote time or buffer");
        }
    }

    private bool IsGrounded()
    {
        BoxCollider2D boxCollider = GetComponent<BoxCollider2D>();
        Vector2 boxSize = new Vector2(boxCollider.size.x - 0.1f, 0.1f);
        Collider2D[] colliders = Physics2D.OverlapBoxAll(groundCheck.position, boxSize, 0f);
        foreach (Collider2D collider in colliders)
        {
            if (collider.gameObject != gameObject)
            {
                return true;
            }
        }
        return false;
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        movement = context.ReadValue<Vector2>();
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            jumpPressed = true;
        }
        if (context.canceled)
        {
            jumpPressed = false;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        BoxCollider2D boxCollider = GetComponent<BoxCollider2D>();
        Vector2 boxSize = new Vector2(boxCollider.size.x - 0.1f, 0.1f);
        Gizmos.DrawWireCube(groundCheck.position, boxSize);
    }
}
