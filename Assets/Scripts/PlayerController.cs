using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D), typeof(SpriteRenderer))]
public class PlayerController : MonoBehaviour
{
    [Header("Stats")]
        private int _baseHealth = 3;

    [SerializeField] public int Lifes;
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

    [Header("Shoot")]
    [SerializeField] private GameObject _projectilePrefab;
    [SerializeField] private Transform _shootPoint;
    [SerializeField] private float _shootForce = 20f;
    private float _shootCooldown;

    [Header("Hit")]
    [SerializeField] private float _hitDistance = 0.5f;
    [SerializeField] private float _pistolHitForce = 5f;
    [SerializeField] private float _punchHitForce = 10f;

    [Header("Invulnerability")]
    [SerializeField] private float _invulnerabilityDuration = 0.5f;

    [Header("Ground Check")]
    [SerializeField] private Transform _groundCheck;
    [SerializeField] private LayerMask _groundLayer;

    [Header("Hand")]
    [SerializeField] private Transform _hand;
    [SerializeField] private SpriteRenderer _handSprite;
    [SerializeField] private float _handSpeed = 7f;
    [SerializeField] private float _handMaxDistance = 1.5f;
    
    [Header("Feel")]
    [SerializeField] private TrailRenderer _dashTrail;
    [SerializeField] private GameObject _blastPrefab;

    [Header("Animation")]
    [SerializeField] private Animator _animator;
    [SerializeField] private Animator _handAnimator;

    private Rigidbody2D _rb;
    private SpriteRenderer _spriteRender;
    private Collider2D _collider;

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
    private bool _canDash = true;
    private bool _isShooting;

    private bool _isInvulnerable;

    public void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _spriteRender = GetComponent<SpriteRenderer>();
        _handSprite = _hand.GetComponent<SpriteRenderer>();
        _collider = GetComponent<Collider2D>();

        _dashTrail.emitting = false;
        Lifes = _baseHealth;

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

        _animator.SetFloat("Speed", _rb.linearVelocity.magnitude);
        if (!_isDashing && !_shooting)
        {
            _animator.SetBool("Jump", !IsGrounded());
        }
        

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

        if (IsGrounded())
        {
            Vector2 pos = transform.position;
            RaycastHit2D hit = Physics2D.Raycast(pos, Vector2.down, 0.1f, LayerMask.GetMask("Ground"));
            if (!hit)
            {
                transform.position += Vector3.down * 0.01f;
            }
        }

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
        if (_movementInput.x < -0.01f)
        {
            _spriteRender.flipX = true;
        }
        else if (_movementInput.x > 0.01f)
        {
            _spriteRender.flipX = false;
        }
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
        _hand.position = Vector3.MoveTowards(_hand.position, _hand.position + dir * _handSpeed, _handSpeed * Time.deltaTime);

        Vector3 toHand = _hand.position - transform.position;
        float rotationOffset = _canDash ? 0 : -90;
        _hand.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(toHand.y, toHand.x) * Mathf.Rad2Deg + rotationOffset);

        if (toHand.magnitude > _handMaxDistance)
        {
            _hand.position = transform.position + toHand.normalized * _handMaxDistance;
        }
    }

    private bool IsGrounded()
    {
        Vector2 playerSize = _spriteRender.bounds.size;
        return Physics2D.OverlapBox(_groundCheck.position, new Vector2(playerSize.x * 0.9f, 0.1f), 0f, _groundLayer);
    }

    private void HandleShooting()
    {
        if (!_shooting || _isShooting || _shootTimer > 0f) return;  

        _isShooting = true;
        _animator.SetBool("Jump", false);
        _animator.SetBool("Dash", false);
        _animator.SetBool("Crossbow", true);
        _handAnimator.SetTrigger("Shoot");

        Vector3 dir = (_hand.position - transform.position).normalized; 

        GameObject bullet = Instantiate(_projectilePrefab, _shootPoint.position, _hand.rotation);
        bullet.GetComponent<Rigidbody2D>().AddForce(dir * _shootForce, ForceMode2D.Impulse);    

        Bullet bulletScript = bullet.GetComponent<Bullet>();
        bulletScript.shooter = this;    

        StartCoroutine(ShootingCooldown());
    }

    private IEnumerator ShootingCooldown()
    {
        AnimationClip shootClip = null;

        foreach (AnimationClip clip in _animator.runtimeAnimatorController.animationClips)
        {
            if (clip.name == "Shoot")
            {
                shootClip = clip;
                break;
            }
        }

        if (shootClip != null)
        {
            yield return new WaitForSeconds(shootClip.length);
        }
        else
        {
            yield return new WaitForSeconds(0.5f);
        }

        _isShooting = false;
        _canDash = true;
        _shootTimer = _shootCooldown;

        _animator.SetBool("Crossbow", false);
    }

    private void Dash()
    {
        if (_isDashing || Time.time < _lastDashTime + _dashCooldown || !_canDash) return;

        _animator.SetBool("Jump", false);
        _animator.SetBool("Dash", true);

        _isDashing = true;
        _dashTrail.emitting = true;
        _lastDashTime = Time.time;
        _canMoveHand = false;

        Vector2 dir = (_hand.position - transform.position).normalized;
        _rb.linearVelocity = dir * _dashSpeed;

        _spriteRender.flipX = dir.x < 0;

        StartCoroutine(StopDash());
    }

    private IEnumerator StopDash()
    {
        yield return new WaitForSeconds(_dashDuration);
        _isDashing = false;
        _rb.linearVelocity = Vector2.zero;
        _canMoveHand = true;
        _dashTrail.emitting = false;

        _animator.SetBool("Dash", false);
    }

    public void ResetDash()
    {
        _lastDashTime = Time.time - _dashCooldown;
    }

    private void Punch()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(_hand.position, _hitDistance);
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

        Vector2 knockbackDirection = new Vector2(dir.x, Mathf.Abs(dir.y) * 0.5f).normalized;

        if (pistol)
            _rb.AddForce(knockbackDirection * _pistolHitForce * force, ForceMode2D.Impulse);
        else
            _rb.AddForce(knockbackDirection * _punchHitForce * force, ForceMode2D.Impulse);

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
        CameraManager.Instance.ShakeCamera(0.5f, 0.5f);
        GameManager.Instance.StartCoroutine(CameraManager.Instance.SlowMotion());

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
        if (GameManager.Instance.CurrentState == GameState.Playing && MatchManager.Instance.IsLoading ) return;

        if (IsDead) return;

        if (GameManager.Instance.CheckPlayer()) return;

        Vector3 screenPos = Camera.main.WorldToScreenPoint(transform.position);
        if (screenPos.x < 0 || screenPos.x > Screen.width || screenPos.y < 0)
        {
            Lifes = 0;
        }
    }

    private void HandleDeath()
    {
        if (Lifes > 0 || IsDead) return;

        Instantiate(_blastPrefab, transform.position, Quaternion.identity);

        _rb.simulated = false;
        _canJump = false;
        _canMoveHand = false;
        IsDead = true;

        _spriteRender.enabled = false;
        _collider.enabled = false;
        _handSprite.enabled = false;

        GameManager.Instance.PlayerDeath++;
    }

    public void KillPlayer()
    {
        Lifes = 0;
    }

    public void Respawn()
    {
        Lifes = _baseHealth;
        IsDead = false; 

        _rb.simulated = true;
        _canJump = true;
        _canMoveHand = true;
        _stunned = false;
        _isDashing = false;
        _isInvulnerable = false;    

        _spriteRender.color = Color.white;
        _spriteRender.enabled = true;
        _collider.enabled = true;
        _handSprite.enabled = true;

        _rb.linearVelocity = Vector2.zero;
        _rb.angularVelocity = 0f;
    }

    public void SetPosition(Vector3 position)
    {
        _rb.position = position;
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

    public void OnAdjustVolume(InputAction.CallbackContext context)
    {
        Vector2 dpadInput = context.ReadValue<Vector2>();

        if (context.canceled) dpadInput = Vector2.zero;

        AudioManager.instance.OnAdjustVolumeFromPlayer(dpadInput);
    }
}