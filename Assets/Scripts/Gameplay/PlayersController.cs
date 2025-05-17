using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using System;
using Mirror;

public class PlayersController : NetworkBehaviour
{
    [Header("Stats")]
    private int _baseHealth;
    public int Health;
    public int PlayerID;
    public int Wins;
    public bool IsDead = false;
    public PlayersController LastHitBy;

    [Header("Movement")]
    [SerializeField] private float _speed;
    [SerializeField] private float _acceleration = 60f;
    [SerializeField] private float _deceleration = 70f;
    [SerializeField] private float _airControl = 0.6f;

    [Header("Jump")]
    [SerializeField] private float _jump;
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
    [SerializeField] private float _dashSpeed;
    [SerializeField] private float _dashDuration;
    [SerializeField] private float _dashCooldown;

    [Header("Shoot")]
    [SerializeField] private GameObject _projectilePrefab;
    [SerializeField] private Transform _shootPoint;
    [SerializeField] private float _reloadTime;
    [SerializeField] private float _bulletSpeed;

    [Header("Hit")]
    [SerializeField] private float _hitDistance = 0.5f;
    [SerializeField] public float PistolHitForce;
    [SerializeField] private float PunchHitForce;

    [Header("Stun")]
    [SerializeField] private float _stun;
    [SerializeField] private float _stunRotationSpeed = 720f;


    [Header("Ground Check")]
    [SerializeField] private Transform _groundCheck;
    [SerializeField] private LayerMask _groundLayer;

    [Header("Feel")]
    [SerializeField] private TrailRenderer _dashTrail;
    [SerializeField] private GameObject _blastPrefab;

    [Header("Crown")]
    public SpriteRenderer CrownSprite;

    [Header("Animation")]
    [SerializeField] private Animator _animator;
    [SerializeField] private Animator _handAnimator;
    [SerializeField] private int maxScreenHeight = 1000;

    [SyncVar(hook = nameof(OnFlipChanged))]
    private bool _isFlipped;


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
    private bool _wasGrounded;
    private bool _isInvulnerable;
    private int _currentColorIndex;

    public void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
        _spriteRender = GetComponentInChildren<SpriteRenderer>();
        _collider = GetComponent<Collider2D>();
        _handSprite = _hand.GetComponentInChildren<SpriteRenderer>();

        _dashTrail.emitting = false;
        Health = _baseHealth;

        if (LobbyManager.Instance != null)
            PlayerID = LobbyManager.Instance.PlayerID;
        else if (MapTester.Instance != null)
            PlayerID = MapTester.Instance.PlayerID;

        _currentColorIndex = PlayerID - 1;

        bool assigned = SkinManager.Instance.AssignColor(PlayerID, _currentColorIndex);
        if (!assigned)
        {
            List<SkinColor> freeColors = SkinManager.Instance.GetAvailableColors();
            if (freeColors.Count > 0)
            {
                _currentColorIndex = SkinManager.Instance.AvailableColors.IndexOf(freeColors[0]);
                SkinManager.Instance.AssignColor(PlayerID, _currentColorIndex);
            }
        }

        _spriteRender.color = SkinManager.Instance.GetPlayerColor(PlayerID);

        name = $"Player {PlayerID}";

        HUDManager.Instance.DisplayPlayerCards(PlayerID);

        AudioManager.Instance.PlaySFX(SFXType.Join);

        _spriteRender.sortingOrder = PlayerID;
        _handSprite.sortingOrder = PlayerID;

        LoadPlayerSettings();

        DontDestroyOnLoad(gameObject);
    }

    public void LoadPlayerSettings()
    {
        var parameters = new Dictionary<GameParameterType, Action<float>>
        {
            { GameParameterType.Health, value => { _baseHealth = (int)value; Health = _baseHealth; } },
            { GameParameterType.Speed, value => _speed = value },
            { GameParameterType.Jump, value => _jump = value },
            { GameParameterType.PunchForce, value => PunchHitForce = value },
            { GameParameterType.CrossbowForce, value => PistolHitForce = value },
            { GameParameterType.BulletSpeed, value => _bulletSpeed = value },
            { GameParameterType.Reload, value => _reloadTime = value },
            { GameParameterType.DashCooldown, value => _dashCooldown = value },
            { GameParameterType.DashSpeed, value => _dashSpeed = value },
            { GameParameterType.DashDuration, value => _dashDuration = value },
            { GameParameterType.Stun, value => _stun = value }
        };

        foreach (var param in parameters)
        {
            var setting = SettingsManager.Instance.GetParameterByKey(param.Key);
            if (setting != null)
            {
                param.Value(setting.value);
            }
            else
            {
                Debug.LogError($"{param.Key} parameter not found!");
            }
        }
    }

    public void Update()
    {
        if (!isLocalPlayer) return;

        if (IsDead) return;

        UpdateJumpTimers();
        
        if (_canMoveHand) UpdateHand();
        if (_isDashing) Punch();

        _animator.SetFloat("Speed", _rb.linearVelocity.magnitude);

        if (!_isDashing && !_isReloading)
            _animator.SetBool("Jump", !IsGrounded());
        else
            _animator.SetBool("Jump", false);


        Death();
        CheckBounds();

        HUDManager.Instance.UpdatePlayerHealth(PlayerID, Health);
    }

    public void FixedUpdate()
    {
        if (!isLocalPlayer) return;
        
        if (_isDashing || _stunned || !_canJump) return;
        if (IsDead) return;

        HandleMovement();
        HandleJump();
        ApplyGravity();
        Shooting();

        bool groundedNow = IsGrounded();

        if (!_wasGrounded && groundedNow)
        {
            _animator.SetTrigger("Land");
            ResetDash();
        }

        _wasGrounded = groundedNow;

        if (groundedNow) _isJumping = false;
    }

    public void SetMovementState(bool canMove) {
        _isDashing = !canMove;
        _stunned = !canMove;

        _rb.constraints = canMove ? RigidbodyConstraints2D.FreezeRotation : RigidbodyConstraints2D.FreezeAll;
    }

    private void HandleMovement()
    {
        float targetSpeed = _movementInput.x * _speed;
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
            _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, _jump);
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
        Vector2 origin = _groundCheck.position;
        float rayLength = 0.1f;
        RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.down, rayLength, _groundLayer);
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

        bool handIsLeft = toHand.x < 0;
        if (handIsLeft && !_isFlipped) CmdSetFlip(true);
        else if (!handIsLeft && _isFlipped) CmdSetFlip(false);
    }

    [Command]
    private void CmdSetFlip(bool flipped)
    {
        _isFlipped = flipped;
    }

    private void OnFlipChanged(bool oldValue, bool newValue)
    {
        _spriteRender.flipX = newValue;
        _handSprite.flipY = newValue;
        if (!newValue) _handSprite.flipX = false;
    }

    private void Dash()
    {
        if (_isDashing || Time.time < _lastDashTime + _dashCooldown) return;

        _animator.SetBool("Jump", false);
        _animator.SetBool("Dash", true);
        _handAnimator.SetBool("Reload", false);

        AudioManager.Instance.PlaySFX(SFXType.Dash);

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
                PlayersController target = hit.GetComponent<PlayersController>();
                if (target.PlayerID != PlayerID)
                    target.TakeHit((int)PistolHitForce, gameObject, false);
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

        var bulletComponent = bullet.GetComponent<Bullet>();
        if (bulletComponent != null)
        {
            bulletComponent.Shooter = this;
            bulletComponent.SetColor(SkinManager.Instance.GetPlayerColor(PlayerID));
            bulletComponent.SetTrailColor(SkinManager.Instance.GetPlayerColor(PlayerID));
            bulletComponent.Launch(dir, _bulletSpeed);
        }

        AudioManager.Instance.PlaySFX(SFXType.Shoot);

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

        _rb.AddForce(knockback * (pistol ? PistolHitForce : PunchHitForce) * force, ForceMode2D.Impulse);

        if (source.TryGetComponent<PlayersController>(out var attacker))
            LastHitBy = attacker;

        Health--;

        AudioManager.Instance.PlaySFX(SFXType.Hit);
        StartCoroutine(Stun());
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
        _handSprite.enabled = false;

        float elapsedTime = 0f;
        float blinkTimer = 0f;
        float blinkInterval = 0.1f;
        bool useRed = true;

        _spriteRender.color = Color.red;

        Color normalColor = SkinManager.Instance.GetPlayerColor(PlayerID);
        Color stunColor = Color.white;

        while (elapsedTime < _stun)
        {
            float rotationAmount = _stunRotationSpeed * Time.deltaTime;
            transform.Rotate(0, 0, rotationAmount);

            blinkTimer += Time.deltaTime;
            if (blinkTimer >= blinkInterval)
            {
                blinkTimer = 0f;
                _spriteRender.color = useRed ? stunColor : normalColor;
                useRed = !useRed;
            }

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        _spriteRender.transform.rotation = Quaternion.identity;
        _spriteRender.color = normalColor;

        _animator.SetBool("Hit", false);

        if (Health <= 0) yield break;

        _stunned = false;
        _isInvulnerable = false;
        _canJump = true;
        _canMoveHand = true;
        _handSprite.enabled = true;
        _rb.linearVelocity = Vector2.zero;
    }

    public void SetHealth(int hp) {
        _baseHealth = hp;
        Health = hp;
    }
    
    public void KillPlayer() => Health = 0;
    
    private void Death()
    {
        if (Health > 0 || IsDead) return;

        GameObject blast = Instantiate(_blastPrefab, transform.position, Quaternion.identity);
        blast.GetComponent<Blast>().SetRedColor(SkinManager.Instance.GetPlayerColor(PlayerID));

        AudioManager.Instance.PlaySFX(SFXType.Death);

        _rb.simulated = false;
        _canJump = false;
        _canMoveHand = false;
        IsDead = true;

        _spriteRender.enabled = false;
        _collider.enabled = false;
        _handSprite.enabled = false;
        CrownSprite.enabled = false;

        Health = 0;

        _spriteRender.transform.rotation = Quaternion.identity;
        transform.rotation = Quaternion.identity;

        GameManager.Instance.PlayerDeath++;
    }

    public void Respawn()
    {
        Health = _baseHealth;
        IsDead = false;

        _rb.simulated = true;
        _rb.linearVelocity = Vector2.zero;
        _rb.angularVelocity = 0f;

        _canJump = true;
        _canMoveHand = true;
        _stunned = false;
        _isDashing = false;
        _isInvulnerable = false;

        _spriteRender.color = SkinManager.Instance.GetPlayerColor(PlayerID);
        _spriteRender.enabled = true;
        _collider.enabled = true;
        _handSprite.enabled = true;

        transform.rotation = Quaternion.identity;
        _spriteRender.transform.rotation = Quaternion.identity;
    }

    private void CheckBounds()
    {
        if (GameManager.Instance.CurrentState == GameState.Playing && MatchManager.Instance.IsLoading) return;
        if (IsDead || GameManager.Instance.CheckPlayer()) return;

        Vector3 screenPos = Camera.main.WorldToScreenPoint(transform.position);
        if (screenPos.x < 0 || screenPos.x > Screen.width || screenPos.y < 0 || screenPos.y > Screen.height + maxScreenHeight) 
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

    public void OnChangeColor(InputAction.CallbackContext context)
    {
        if (!context.performed) return;
        if (GameManager.Instance.CurrentState != GameState.WaitingForPlayers && GameManager.Instance.CurrentState != GameState.Editor) return;
    
        Vector2 dpadInput = context.ReadValue<Vector2>();
        int direction = dpadInput.x > 0.1f ? 1 : (dpadInput.x < -0.1f ? -1 : 0);
    
        if (direction == 0) return;
    
        int totalColors = SkinManager.Instance.AvailableColors.Count;
        int startIndex = _currentColorIndex;
    
        for (int i = 1; i <= totalColors; i++)
        {
            int newIndex = (_currentColorIndex + i * direction + totalColors) % totalColors;
    
            if (SkinManager.Instance.ChangeColor(PlayerID, newIndex))
            {
                _currentColorIndex = newIndex;
                _spriteRender.color = SkinManager.Instance.GetPlayerColor(PlayerID);
                break;
            }
    
            if (newIndex == startIndex) break;
        }

        HUDManager.Instance.UpdateColorPlayerCard(PlayerID, SkinManager.Instance.GetPlayerColor(PlayerID));
    }
}
