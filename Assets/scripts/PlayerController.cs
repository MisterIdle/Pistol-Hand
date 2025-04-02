using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class PlayerController : MonoBehaviour
{
    [Header("Player Stats")]
    [SerializeField] private int lifes = 4;
    [SerializeField] public int playerID = 0;
    [SerializeField] public bool isDead;
    [SerializeField] public int wins = 0;

    [Header("Player Movement")]
    [SerializeField] private float maxSpeed = 10f;
    [SerializeField] private float acceleration = 60f;
    [SerializeField] private float deceleration = 70f;
    [SerializeField] private float airControl = 0.6f;
    [SerializeField] private bool stunned = false;
    [SerializeField] private bool canJump = true;

    [Header("Jump")]
    [SerializeField] private float jumpForce = 14f;
    [SerializeField] private float gravityMultiplier = 2f;
    [SerializeField] private float coyoteTime = 0.15f;
    [SerializeField] private float jumpBufferTime = 0.1f;
    [SerializeField] private float maxFallSpeed = 10f;

    [Header("Shooting")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform shootPoint;
    [SerializeField] private float shootForce = 20f;
    [SerializeField] private float shootCooldown = 0.5f;
    private float shootTimer = 0f;
    private bool shooting = false;

    [Header("Dashing")]
    [SerializeField] private float dashSpeed = 20f;
    [SerializeField] private float dashDuration = 0.2f;
    [SerializeField] private float dashCooldown = 1f;
    private bool isDashing = false;
    private float lastDashTime = 0f;
    private Vector2 hitedForce = new Vector2(0, 0);
    private bool canDash = true;

    [Header("Invulnerability")]
    [SerializeField] private float invulnerabilityDuration = 0.5f;
    private bool isInvulnerable = false;

    [Header("Ground Detection")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask groundLayer;

    [Header("Hand Control")]
    [SerializeField] private Transform hand;
    [SerializeField] private float handSpeed = 7f;
    [SerializeField] private float handMaxDistance = 1.5f;
    private bool canMoveHand = true;

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

        DontDestroyOnLoad(gameObject);

        gameObject.name = "Player " + playerID;
    }

    private void Update()
    {
        if (shootTimer > 0f)
        {
            shootTimer -= Time.deltaTime;
        }

        UpdateTimers();

        if (canMoveHand)
        {
            MoveHand();
            ClampHandDistance();
        }

        if (isDashing)
        {
            Punch();
        }

        Death();
        Shooting();

        LimitedZone();
    }

    private void FixedUpdate()
    {
        if (!isDashing && !stunned && canJump)
        {
            HandleMovement();
            HandleJump();
            ApplyGravity();
        }
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

        Vector3 directionToHand = hand.position - transform.position;
        float angle = Mathf.Atan2(directionToHand.y, directionToHand.x) * Mathf.Rad2Deg;

        hand.rotation = Quaternion.Euler(0, 0, angle);
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
        return Physics2D.OverlapCircle(groundCheck.position, 0.1f, groundLayer);
    }

    public void Shooting()
    {
        if (shooting && shootTimer <= 0f)
        {
            Vector3 direction = (hand.position - transform.position).normalized;

            GameObject bullet = Instantiate(projectilePrefab, shootPoint.position, hand.rotation);
            Rigidbody2D bulletRb = bullet.GetComponent<Rigidbody2D>();

            bulletRb.AddForce(direction * shootForce, ForceMode2D.Impulse);

            shooting = false;
            shootTimer = shootCooldown;
        }
    }

    private void Dash()
    {
        if (Time.time < lastDashTime + dashCooldown || isDashing) return;

        isDashing = true;
        lastDashTime = Time.time;
        canMoveHand = false;

        Vector2 dashDirection = (hand.position - transform.position).normalized;
        rb.linearVelocity = dashDirection * dashSpeed;

        StartCoroutine(StopDash());
    }

    private IEnumerator StopDash()
    {
        yield return new WaitForSeconds(dashDuration);

        isDashing = false;
        rb.linearVelocity = Vector2.zero;
        canMoveHand = true;
    }

    public void HitPlayer(int force, GameObject target, PlayerController player, bool pistol)
    {
        if (player.lifes <= 0) return;
        if (player.isDashing) return;
        if (player.stunned) return;
        if (player.isInvulnerable) return;

        Vector2 direction = target.transform.position - player.transform.position;
        direction.Normalize();

        player.StartCoroutine(player.Stun());
        player.StartCoroutine(player.Invulnerability());

         if (player.stunned && pistol)
            rb.AddForce(new Vector2(direction.x, 1f) * force, ForceMode2D.Impulse);
        else if (player.stunned && !pistol)
            rb.AddForce(new Vector2(-direction.x, 0.5f) * force, ForceMode2D.Impulse);

        player.lifes -= 1;
    }

    public IEnumerator Invulnerability()
    {
        isInvulnerable = true;
        yield return new WaitForSeconds(invulnerabilityDuration);
        isInvulnerable = false;
    }

    void Punch()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(hand.transform.position, 1f);
        foreach (Collider2D hit in hits)
        {
            if (hit.CompareTag("Player"))
            {
                PlayerController player = hit.GetComponent<PlayerController>();
                if (player.playerID != playerID)
                {
                    player.HitPlayer(10, gameObject, player, false);
                }
            }
        }
    }
    
    public IEnumerator Stun()
    {
        stunned = true;
        sprite.color = Color.red;
        yield return new WaitForSeconds(invulnerabilityDuration);
        sprite.color = Color.white;
        stunned = false;
        rb.linearVelocity = Vector2.zero;
        canJump = true;
    }

    void LimitedZone()
    {
        Vector3 screenPosition = Camera.main.WorldToScreenPoint(transform.position);

        bool isOutOfBounds = screenPosition.x < 0 || screenPosition.x > Screen.width || screenPosition.y < 0;

        if (isOutOfBounds)
        {
            lifes = 0;
        }
    }

    public void Death()
    {
        if (lifes <= 0)
        {
            isDead = true;
            canJump = false;
            canMoveHand = false;

            enabled = false;
            rb.simulated = false;

            sprite.color = Color.clear;

            GameManager.Instance.playersDeath++;
        }
    }

    public void Respawn()
    {
        isDead = false;
        canJump = true;
        canMoveHand = true;

        enabled = true;
        rb.simulated = true;

        sprite.color = Color.white;
        lifes = 4;

        Vector3 spawnPosition = new Vector3(0, 0, 0);
        transform.position = spawnPosition;
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
        else if (context.canceled)
        {
            jumpRequested = false;
        }
    }

    public void OnShoot(InputAction.CallbackContext context)
    {
        shooting = context.performed;
    }

    public void OnDash(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            Dash();
        }
    }

    public void OnLook(InputAction.CallbackContext context)
    {
        if (canMoveHand)
        {
            lookInput = context.ReadValue<Vector2>();
        }
    }
}
