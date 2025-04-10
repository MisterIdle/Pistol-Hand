using UnityEngine;

public class DashResetBlock : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    [SerializeField] private bool isOnCooldown = false;
    [SerializeField] private float cooldownCount = 0;

    public float colorAlpha = 0.2f;

    public float cooldownCountMax = 5f;

    public float fadeSpeed = 1f;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void FixedUpdate()
    {
        HandleFade();
    }

    private void HandleFade()
    {
        if (isOnCooldown && cooldownCount < cooldownCountMax)
        {
            cooldownCount += Time.deltaTime;

            if (spriteRenderer.color.a > colorAlpha) FadeOut();
        }
        else if (!isOnCooldown && spriteRenderer.color.a < 1)
        {
            FadeIn();
        }

        if (spriteRenderer.color.a <= colorAlpha && cooldownCount >= cooldownCountMax)
        {
            isOnCooldown = false;
        }
        else if (spriteRenderer.color.a >= 1)
        {
            cooldownCount = 0;
        }
    }

    private void FadeOut()
    {
        Color color = spriteRenderer.color;
        color.a -= Time.deltaTime * fadeSpeed;
        spriteRenderer.color = color;
    }

    private void FadeIn()
    {
        Color color = spriteRenderer.color;
        color.a += Time.deltaTime * fadeSpeed;
        spriteRenderer.color = color;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            if (spriteRenderer.color.a < 1) return;
            PlayerController playerController = collision.gameObject.GetComponent<PlayerController>();
            if (! isOnCooldown) playerController.lastDashTime = 0;
            Debug.Log("reseted dash");
            isOnCooldown = true;
        }
    }
}
