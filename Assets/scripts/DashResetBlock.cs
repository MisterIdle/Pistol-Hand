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
            FadeOut();
        }
        else if (isOnCooldown && cooldownCount >= cooldownCountMax && spriteRenderer.color.a < colorAlpha)
        {
            FadeIn();
        }

        spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, Mathf.Clamp(spriteRenderer.color.a, 0f, 1f));

        if (spriteRenderer.color.a >= colorAlpha)
        {
            isOnCooldown = false;
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
            Debug.Log("reseted dash");
            isOnCooldown = true;
        }
    }
}
