using UnityEngine;

public class DashResetBlock : MonoBehaviour
{
    private SpriteRenderer _spriteRenderer;

    [SerializeField] private bool _isOnCooldown = false;
    [SerializeField] private float _cooldownCount = 0f;
    [SerializeField] private float _colorAlpha = 0.2f;
    [SerializeField] private float _cooldownCountMax = 5f;
    [SerializeField] private float _fadeSpeed = 1f;

    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void FixedUpdate()
    {
        HandleFade();
    }

    private void HandleFade()
    {
        if (_isOnCooldown)
        {
            _cooldownCount += Time.deltaTime;
            if (_cooldownCount >= _cooldownCountMax)
            {
                _isOnCooldown = false;
                _cooldownCount = 0f;
            }
            else if (_spriteRenderer.color.a > _colorAlpha)
            {
                AdjustAlpha(-_fadeSpeed);
            }
        }
        else if (_spriteRenderer.color.a < 1f)
        {
            AdjustAlpha(_fadeSpeed);
        }
    }

    private void AdjustAlpha(float adjustment)
    {
        Color color = _spriteRenderer.color;
        color.a = Mathf.Clamp01(color.a + adjustment * Time.deltaTime);
        _spriteRenderer.color = color;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && _spriteRenderer.color.a >= 1f)
        {
            PlayerController playerController = collision.GetComponent<PlayerController>();
            if (!_isOnCooldown)
            {
                playerController.ResetDash();
                Debug.Log("Dash reset");
                _isOnCooldown = true;
            }
        }
    }
}
