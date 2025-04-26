using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

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

    [Header("Hand")]
    [SerializeField] private Transform _hand;
    [SerializeField] private SpriteRenderer _handSprite;
    [SerializeField] private float _handSpeed = 7f;
    [SerializeField] private float _handMaxDistance = 1f;

    [Header("Dash")]
    [SerializeField] private float _dashSpeed = 20f;
    [SerializeField] private float _dashDuration = 0.2f;
    [SerializeField] private float _dashCooldown = 1f;

    [Header("Shoot")]
    [SerializeField] private GameObject _projectilePrefab;
    [SerializeField] private Transform _shootPoint;
    [SerializeField] private float _shootForce = 20f;
    [SerializeField] private float _reloadTime = 0.5f;

    [Header("Hit")]
    [SerializeField] private float _hitDistance = 0.5f;
    [SerializeField] private float _pistolHitForce = 5f;
    [SerializeField] private float _punchHitForce = 10f;

    [Header("Stun")]
    [SerializeField] private float _stunDuration = 0.5f;
    [SerializeField] private float _stunRotationSpeed = 720f;


    [Header("Ground Check")]
    [SerializeField] private Transform _groundCheck;
    [SerializeField] private LayerMask _groundLayer;

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
    private bool _jumpRequested;
    private bool _shooting;

    private float _coyoteCounter;
    private float _jumpBufferCounter;
    private float _lastDashTime;

    private bool _isJumping;
    private bool _isDashing;
    private bool _stunned;
    private bool _canJump = true;
    private bool _canMoveHand = true;
    private bool _isShooting;
    private bool _isReloading;

    private bool _loaded;

    private bool _wasGrounded;

    private bool _isInvulnerable;

    public void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
        _spriteRender = GetComponentInChildren<SpriteRenderer>();
        _collider = GetComponent<Collider2D>();
        _handSprite = _hand.GetComponentInChildren<SpriteRenderer>();

        _dashTrail.emitting = false;
        Lifes = _baseHealth;

        DontDestroyOnLoad(gameObject);

        SkinManager.Instance.AssignColor(PlayerID, PlayerID);
        Color playerColor = SkinManager.Instance.GetPlayerColor(PlayerID);
        _spriteRender.color = playerColor;

        print($"Player {PlayerID} color: {playerColor}");
    }

    public void Update()
    {
        UpdateJumpTimers();

        if (_canMoveHand) UpdateHand();
        if (_isDashing) Punch();

        _animator.SetFloat("Speed", _rb.linearVelocity.magnitude);

        if (!_isDashing && !_isReloading)
            _animator.SetBool("Jump", !IsGrounded());
        else
            _animator.SetBool("Jump", false);


        Death();
        Shooting();
        CheckBounds();
    }

    public void FixedUpdate()
    {
        if (_isDashing || _stunned || !_canJump) return;

        HandleMovement();
        HandleJump();
        ApplyGravity();

        bool groundedNow = IsGrounded();

        if (!_wasGrounded && groundedNow)
        {
            _animator.SetTrigger("Land");
        }

        _wasGrounded = groundedNow;

        if (groundedNow) _isJumping = false;
    }


    private void HandleMovement()
    {
        float targetSpeed = _movementInput.x * _maxSpeed;
        float speedDiff = targetSpeed - _rb.linearVelocity.x;
        float accelRate = Mathf.Abs(targetSpeed) > 0.01f ? _acceleration : _deceleration;

        if (!IsGrounded()) accelRate *= _airControl;

        _rb.AddForce(Vector2.right * speedDiff * accelRate);

        _spriteRender.flipX = _movementInput.x < -0.01f;
        if (_movementInput.x > 0.01f) _spriteRender.flipX = false;
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

    private void UpdateJumpTimers()
    {
        _coyoteCounter = IsGrounded() ? _coyoteTime : _coyoteCounter - Time.deltaTime;
        _jumpBufferCounter = _jumpRequested ? _jumpBufferTime : _jumpBufferCounter - Time.deltaTime;
    }

    private bool IsGrounded()
    {
        Vector2 position = _groundCheck.position;
        Vector2 size = _spriteRender.bounds.size;
        float rayLength = 0.2f;
        float spacing = size.x * 0.4f;

        Vector2 left = position + Vector2.left * spacing;
        Vector2 center = position;
        Vector2 right = position + Vector2.right * spacing;

        return RaycastGround(left, rayLength) || RaycastGround(center, rayLength) || RaycastGround(right, rayLength);
    }

    private bool RaycastGround(Vector2 origin, float length)
    {
        RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.down, length, _groundLayer);
        Debug.DrawRay(origin, Vector2.down * length, hit ? Color.green : Color.red);
        return hit.collider != null;
    }

    private void UpdateHand()
    {
        Vector3 dir = new Vector3(_lookInput.x, _lookInput.y).normalized;
        _hand.position = Vector3.MoveTowards(_hand.position, _hand.position + dir * _handSpeed, _handSpeed * Time.deltaTime);

        Vector3 toHand = _hand.position - transform.position;
        
        _hand.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(toHand.y, toHand.x) * Mathf.Rad2Deg);

        _handSprite.flipY = toHand.x < 0;
        _spriteRender.flipX = toHand.x < 0;
        if (toHand.x > 0) _handSprite.flipX = false;

        if (toHand.magnitude > _handMaxDistance)
            _hand.position = transform.position + toHand.normalized * _handMaxDistance;
    }

    private void Dash()
    {
        if (_isDashing || Time.time < _lastDashTime + _dashCooldown) return;

        _animator.SetBool("Jump", false);
        _animator.SetBool("Dash", true);
        _handAnimator.SetBool("Reload", false);

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

        _spriteRender.transform.rotation = Quaternion.identity;

        _animator.SetBool("Dash", false);
    }

    public void ResetDash() => _lastDashTime = Time.time - _dashCooldown;

    private void Punch()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(_hand.position, _hitDistance);
        foreach (var hit in hits)
        {
            if (hit.CompareTag("Player"))
            {
                PlayerController target = hit.GetComponent<PlayerController>();
                if (target.PlayerID != PlayerID)
                    target.TakeHit(10, gameObject, false);
            }
        }
    }

    private void Shooting()
    {
        if (!_shooting || _isShooting || _isDashing || _isReloading) return;    

        _isShooting = true;
        _isReloading = true;   

        _animator.SetBool("Shoot", true);

        float reloadAnimDuration = 1f;
        _handAnimator.speed = reloadAnimDuration / _reloadTime;
        _handAnimator.SetTrigger("Shoot");

        Vector3 dir = (_hand.position - transform.position).normalized;
        GameObject bullet = Instantiate(_projectilePrefab, _shootPoint.position, _hand.rotation);

        var bulletRb = bullet.GetComponent<Rigidbody2D>();
        if (bulletRb != null)
            bulletRb.AddForce(dir * _shootForce, ForceMode2D.Impulse);  

        var bulletComponent = bullet.GetComponent<Bullet>();
        if (bulletComponent != null)
            bulletComponent.shooter = this; 

        StartCoroutine(ShootingCooldown());
    }   

    private IEnumerator ShootingCooldown()
    {
        _handAnimator.SetBool("Reload", true);

        yield return new WaitForSeconds(0.2f);

        _animator.SetBool("Shoot", false);

        yield return new WaitForSeconds(_reloadTime);  

        _handAnimator.SetBool("Reload", false);
        _handAnimator.speed = 1f;

        if (_shooting)
            _handAnimator.SetTrigger("Shoot"); 

        _isReloading = false;
        _isShooting = false;
    }


    public void TakeHit(int force, GameObject source, bool pistol)
    {
        if (_isInvulnerable) return;

        Vector2 dir = (transform.position - source.transform.position).normalized;
        Vector2 knockback = new Vector2(dir.x, Mathf.Abs(dir.y) * 0.5f).normalized;

        _rb.AddForce(knockback * (pistol ? _pistolHitForce : _punchHitForce) * force, ForceMode2D.Impulse);

        Lifes--;
        StartCoroutine(Stun());
        StartCoroutine(FlashRed());
    }

    private IEnumerator FlashRed()
    {
        Color original = _spriteRender.color;
        _spriteRender.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        _spriteRender.color = original;
    }

    private IEnumerator Stun()
    {
        CameraManager.Instance.ShakeCamera(0.5f, 0.5f);
        GameManager.Instance.StartCoroutine(CameraManager.Instance.SlowMotion());

        _animator.SetBool("Hit", true);

        _stunned = true;
        _isInvulnerable = true;
        _canJump = false;
        _canMoveHand = false;
        _spriteRender.color = Color.red;
        _handSprite.enabled = false;

        float elapsedTime = 0f;

        while (elapsedTime < _stunDuration)
        {
            float rotationAmount = _stunRotationSpeed * Time.deltaTime;
            transform.Rotate(0, 0, rotationAmount);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.rotation = Quaternion.identity;
        _spriteRender.color = Color.white;

        _animator.SetBool("Hit", false);

        if (Lifes <= 0) yield break;

        _stunned = false;
        _isInvulnerable = false;
        _canJump = true;
        _canMoveHand = true;
        _handSprite.enabled = true;
        _rb.linearVelocity = Vector2.zero;
        _canJump = true;
    }

    public void KillPlayer() => Lifes = 0;
    
    private void Death()
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

    public void Respawn()
    {
        Lifes = _baseHealth;
        IsDead = false;

        _rb.simulated = true;
        _rb.linearVelocity = Vector2.zero;
        _rb.angularVelocity = 0f;

        _canJump = true;
        _canMoveHand = true;
        _stunned = false;
        _isDashing = false;
        _isInvulnerable = false;

        _spriteRender.color = Color.white;
        _spriteRender.enabled = true;
        _collider.enabled = true;
        _handSprite.enabled = true;
    }

    private void CheckBounds()
    {
        if (GameManager.Instance.CurrentState == GameState.Playing && MatchManager.Instance.IsLoading) return;
        if (IsDead || GameManager.Instance.CheckPlayer()) return;

        Vector3 screenPos = Camera.main.WorldToScreenPoint(transform.position);
        if (screenPos.x < 0 || screenPos.x > Screen.width || screenPos.y < 0)
            KillPlayer();
    }

    public void SetPosition(Vector3 position) => _rb.position = position;

    public void OnMove(InputAction.CallbackContext ctx) => _movementInput = ctx.ReadValue<Vector2>();

    public void OnJump(InputAction.CallbackContext ctx)
    {
        _jumpRequested = ctx.ReadValue<float>() > 0;
        if (_jumpRequested) _jumpBufferCounter = _jumpBufferTime;
    }

    public void OnShoot(InputAction.CallbackContext ctx) => _shooting = ctx.ReadValue<float>() > 0;

    public void OnDash(InputAction.CallbackContext ctx)
    {
        if (ctx.performed) Dash();
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

    public void OnChangeColor(InputAction.CallbackContext context)
    {
        Vector2 dpadInput = context.ReadValue<Vector2>();
        if (context.canceled) dpadInput = Vector2.zero;

        SkinManager.Instance.AssignColor(PlayerID, dpadInput.y > 0 ? 1 : -1);
    }
}
