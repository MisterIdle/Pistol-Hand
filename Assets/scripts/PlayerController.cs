using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
    [Header("Player Movement")]
    [SerializeField] private float maxSpeed = 10f;
    [SerializeField] private float acceleration = 60f;
    [SerializeField] private float deceleration = 70f;
    [SerializeField] private float airControl = 0.6f;

    [Header("Jump")]
    [SerializeField] private float jumpForce = 14f;
    [SerializeField] private float gravityMultiplier = 2f;
    [SerializeField] private float coyoteTime = 0.15f;
    [SerializeField] private float jumpBufferTime = 0.1f;
    [SerializeField] private float maxFallSpeed = 10f;

    [Header("Ground Detection")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask groundLayer;

    [Header("Hand Control")]
    [SerializeField] private Transform hand;
    [SerializeField] private float handSpeed = 7f;
    [SerializeField] private float handMaxDistance = 4f;

    private Rigidbody2D rb;
    private SpriteRenderer sprite;
    private Vector2 movementInput;
    private Vector2 lookInput;
    private bool jumpRequested;
    private float coyoteTimeCounter;
    private float jumpBufferCounter;
    private bool isJumping;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sprite = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            RestartLevel();
        }

        UpdateTimers();
        MoveHand();
        ClampHandDistance();
    }

    private void FixedUpdate()
    {
        HandleMovement();
        HandleJump();
        ApplyGravity();
    }

    private void UpdateTimers()
    {
        if (IsGrounded()) 
        {
            coyoteTimeCounter = coyoteTime;
            isJumping = false;
        }
        else 
        {
            coyoteTimeCounter -= Time.deltaTime;
        }

        if (jumpRequested) 
            jumpBufferCounter = jumpBufferTime;
        else 
            jumpBufferCounter -= Time.deltaTime;
    }

    private void HandleMovement()
    {
        float targetSpeed = movementInput.x * maxSpeed;
        float speedDiff = targetSpeed - rb.linearVelocity.x;
        float accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? acceleration : deceleration;
        if (!IsGrounded()) accelRate *= airControl;

        rb.AddForce(Vector2.right * speedDiff * accelRate);

        sprite.flipX = movementInput.x < -0.1f;
    }

    private void HandleJump()
    {
        if (jumpBufferCounter > 0 && coyoteTimeCounter > 0 && !isJumping) 
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            isJumping = true;
            jumpRequested = false;
        }
        else if (jumpRequested && coyoteTimeCounter > 0) 
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            isJumping = true;
            jumpRequested = false;
        }
        else if (IsGrounded() && rb.linearVelocity.y < 0) 
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0);
        }
    }

    private void ApplyGravity()
    {
        if (!IsGrounded() && rb.linearVelocity.y < maxFallSpeed)
        {
            rb.gravityScale = gravityMultiplier;
        }
        else
        {
            rb.gravityScale = 1f;
        }
    }

    private void MoveHand()
    {
        Vector3 direction = new Vector3(lookInput.x, lookInput.y, 0).normalized;
        hand.position = Vector3.MoveTowards(hand.position, hand.position + direction * handSpeed, handSpeed * Time.deltaTime);
    }

    private void ClampHandDistance()
    {
        float distance = Vector3.Distance(hand.position, transform.position);
        if (distance > handMaxDistance)
        {
            Vector3 direction = (hand.position - transform.position).normalized;
            hand.position = transform.position + direction * handMaxDistance;
        }
    }

    private bool IsGrounded()
    {
        Vector2 start = new Vector2(groundCheck.position.x - sprite.bounds.extents.x, groundCheck.position.y);
        Vector2 end = new Vector2(groundCheck.position.x + sprite.bounds.extents.x, groundCheck.position.y);
        return Physics2D.OverlapArea(start, end, groundLayer);
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        movementInput = context.ReadValue<Vector2>();
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            jumpRequested = true;
            jumpBufferCounter = jumpBufferTime;
        }
        else if (context.performed)
        {
            jumpRequested = true;
        }
        else if (context.canceled)
        {
            jumpRequested = false;
        }
    }

    public void OnLook(InputAction.CallbackContext context)
    {
        lookInput = context.ReadValue<Vector2>();
    }

    public void RestartLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
