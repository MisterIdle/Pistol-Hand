using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D), typeof(SpriteRenderer))]
public class PlayerController : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] public int Lifes = 4;
    [SerializeField] public int PlayerID;
    [SerializeField] public int Wins;
    [SerializeField] public bool IsDead;

    [Header("Movement")]
    [SerializeField] private float _maxSpeed = 10f;
    [SerializeField] private float _acceleration = 60f;
    [SerializeField] private float _deceleration = 70f;
    [SerializeField] private float _airControl = 0.6f;

    [Header("Jump")]
    [SerializeField] private float _jumpForce = 14f;
    [SerializeField] private float _gravityMultiplier = 2f;
    [SerializeField] private float _maxFallSpeed = 10f;
    [SerializeField] private float _coyoteTime = 0.15f;
    [SerializeField] private float _jumpBufferTime = 0.1f;

    [Header("Dash")]
    [SerializeField] private float _dashSpeed = 20f;
    [SerializeField] private float _dashDuration = 0.2f;
    [SerializeField] private float _dashCooldown = 1f;
    [SerializeField] private float _hitDistance = 0.5f;

    [Header("Shoot")]
    [SerializeField] private GameObject _projectilePrefab;
    [SerializeField] private Transform _shootPoint;
    [SerializeField] private float _shootForce = 20f;
    [SerializeField] private float _shootCooldown = 0.5f;

    [Header("Invulnerability")]
    [SerializeField] private float _invulnerabilityDuration = 0.5f;

    [Header("Ground Check")]
    [SerializeField] private Transform _groundCheck;
    [SerializeField] private LayerMask _groundLayer;

    [Header("Hand")]
    [SerializeField] private Transform hand;
    [SerializeField] private float _handSpeed = 7f;
    [SerializeField] private float _handMaxDistance = 1.5f;
    
    [Header("Feel")]
    [SerializeField] private TrailRenderer _dashTrail;
    [SerializeField] private GameObject _blastPrefab;

    private Rigidbody2D _rb;
    private SpriteRenderer _spriteRender;

    private Vector2 _movementInput;
    private Vector2 _lookInput;
    private float _coyoteCounter;
    private float _jumpBufferCounter;
    private float _shootTimer;
    private float _lastDashTime;

    private bool _isJumping;
    private bool _isDashing;
    private bool _stunned;
    private bool _canJump = true;
    private bool _canMoveHand = true;
    private bool _jumpRequested;
    private bool _shooting;
    private bool _isInvulnerable;

    public void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _spriteRender = GetComponent<SpriteRenderer>();

        _dashTrail.emitting = false;

        DontDestroyOnLoad(gameObject);
    }

    public void Update()
    {
        _shootTimer = Mathf.Max(0f, _shootTimer - Time.deltaTime);

        UpdateJumpTimers();

        if (_canMoveHand)
        {
            UpdateHand();
        }

        if (_isDashing) Punch();

        HandleDeath();
        HandleShooting();
        CheckBounds();
    }

    public void FixedUpdate()
    {
        if (_isDashing || _stunned || !_canJump) return;

        HandleMovement();
        HandleJump();
        ApplyGravity();

        if (IsGrounded()) _isJumping = false;
    }

    private void UpdateJumpTimers()
    {
        _coyoteCounter = IsGrounded() ? _coyoteTime : _coyoteCounter - Time.deltaTime;
        _jumpBufferCounter = _jumpRequested ? _jumpBufferTime : _jumpBufferCounter - Time.deltaTime;
    }

    private void HandleMovement()
    {
        float targetSpeed = _movementInput.x * _maxSpeed;
        float speedDiff = targetSpeed - _rb.linearVelocity.x;
        float accelRate = Mathf.Abs(targetSpeed) > 0.01f ? _acceleration : _deceleration;
        if (!IsGrounded()) accelRate *= _airControl;

        _rb.AddForce(Vector2.right * speedDiff * accelRate);
        _spriteRender.flipX = _movementInput.x < -0.1f;
    }

    private void HandleJump()
    {
        if (_jumpBufferCounter > 0 && _coyoteCounter > 0 && !_isJumping)
        {
            _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, _jumpForce);
            _isJumping = true;
            _jumpBufferCounter = 0;
        }
    }

    private void ApplyGravity()
    {
        _rb.gravityScale = (!IsGrounded() && _rb.linearVelocity.y < _maxFallSpeed) ? _gravityMultiplier : 1f;
    }

    private void UpdateHand()
    {
        Vector3 dir = new Vector3(_lookInput.x, _lookInput.y).normalized;
        hand.position = Vector3.MoveTowards(hand.position, hand.position + dir * _handSpeed, _handSpeed * Time.deltaTime);

        Vector3 toHand = hand.position - transform.position;
        hand.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(toHand.y, toHand.x) * Mathf.Rad2Deg);

        if (toHand.magnitude > _handMaxDistance)
        {
            hand.position = transform.position + toHand.normalized * _handMaxDistance;
        }
    }

    private bool IsGrounded()
    {
        Vector2 playerSize = _spriteRender.bounds.size;
        return Physics2D.OverlapBox(_groundCheck.position, new Vector2(playerSize.x * 0.9f, 0.1f), 0f, _groundLayer);
    }

    private void HandleShooting()
    {
        if (!_shooting || _shootTimer > 0f) return; 

        Vector3 dir = (hand.position - transform.position).normalized;
        GameObject bullet = Instantiate(_projectilePrefab, _shootPoint.position, hand.rotation);
        bullet.GetComponent<Rigidbody2D>().AddForce(dir * _shootForce, ForceMode2D.Impulse);

        Bullet bulletScript = bullet.GetComponent<Bullet>();
        bulletScript.shooter = this;

        _shootTimer = _shootCooldown;
    }


    private void Dash()
    {
        if (_isDashing || Time.time < _lastDashTime + _dashCooldown) return;

        _isDashing = true;
        _dashTrail.emitting = true;
        _lastDashTime = Time.time;
        _canMoveHand = false;

        Vector2 dir = (hand.position - transform.position).normalized;
        _rb.linearVelocity = dir * _dashSpeed;

        StartCoroutine(StopDash());
    }

    private IEnumerator StopDash()
    {
        yield return new WaitForSeconds(_dashDuration);
        _isDashing = false;
        _rb.linearVelocity = Vector2.zero;
        _canMoveHand = true;
        _dashTrail.emitting = false;
    }

    public void ResetDash()
    {
        _isDashing = false;
        _rb.linearVelocity = Vector2.zero;
        _canMoveHand = true;
    }

    private void Punch()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(hand.position, _hitDistance);
        foreach (var hit in hits)
        {
            if (hit.CompareTag("Player"))
            {
                PlayerController target = hit.GetComponent<PlayerController>();
                if (target.PlayerID != PlayerID)
                {
                    target.TakeHit(10, gameObject, false);
                }
            }
        }
    }

    public void TakeHit(int force, GameObject source, bool pistol)
    {
        if (_isInvulnerable) return;

        Vector2 dir = (transform.position - source.transform.position).normalized;

        StartCoroutine(Stun());
        StartCoroutine(Invulnerability());

        StartCoroutine(FlashRed());

        if (pistol)
            _rb.AddForce(new Vector2(-dir.x, 1f) * force, ForceMode2D.Impulse);
        else
            _rb.AddForce(new Vector2(dir.x, 0.5f) * force, ForceMode2D.Impulse);

        Lifes--;
    }

    private IEnumerator FlashRed()
    {
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        Color originalColor = spriteRenderer.color;
        spriteRenderer.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        spriteRenderer.color = originalColor;
    }

    private IEnumerator Invulnerability()
    {
        _isInvulnerable = true;
        yield return new WaitForSeconds(_invulnerabilityDuration);
        _isInvulnerable = false;
    }

    private IEnumerator Stun()
    {
        GameManager.Instance.ShakeCamera(0.5f, 0.5f);
        GameManager.Instance.StartCoroutine(GameManager.Instance.StunAndSlowMotion());

        _stunned = true;
        _spriteRender.color = Color.red;
        yield return new WaitForSeconds(_invulnerabilityDuration);
        _spriteRender.color = Color.white;
        _stunned = false;
        _rb.linearVelocity = Vector2.zero;
        _canJump = true;
    }

    private void CheckBounds()
    {
        Vector3 screenPos = Camera.main.WorldToScreenPoint(transform.position);
        if (screenPos.x < 0 || screenPos.x > Screen.width || screenPos.y < 0)
        {
            Lifes = 0;
        }
    }

    private void HandleDeath()
    {
        if (Lifes > 0) return;

        Instantiate(_blastPrefab, transform.position, Quaternion.identity);
        _rb.simulated = false;
        _canJump = false;
        _canMoveHand = false;
        IsDead = true;

        gameObject.SetActive(false);

        if(GameManager.Instance.CurrentState == GameManager.GameState.WaitingForPlayers)
        {
            GameManager.Instance.PlayerReadyCount++;
        }
    }

    public void Respawn()
    {
        Lifes = 4;
        _rb.simulated = true;
        _canJump = true;
        _canMoveHand = true;
        IsDead = false;

        gameObject.SetActive(true);
    }

    public void OnMove(InputAction.CallbackContext ctx) 
    {
        _movementInput = ctx.ReadValue<Vector2>();
    }

    public void OnJump(InputAction.CallbackContext ctx)
    {
        _jumpRequested = ctx.ReadValue<float>() > 0;
        if (_jumpRequested)
        {
            _jumpBufferCounter = _jumpBufferTime;
        }
    }

    public void OnShoot(InputAction.CallbackContext ctx)
    {
        _shooting = ctx.ReadValue<float>() > 0;
    }

    public void OnDash(InputAction.CallbackContext ctx) 
    {
        if (ctx.performed) 
        {
            Dash(); 
        }
    }

    public void OnLook(InputAction.CallbackContext ctx) 
    { 
        if (_canMoveHand) _lookInput = ctx.ReadValue<Vector2>(); 
    }
}
