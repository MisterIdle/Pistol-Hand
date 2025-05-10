using UnityEngine;

public class Spring : MonoBehaviour
{
    [SerializeField] private float _bounceForce = 10f;
    [SerializeField] private float _cooldown = 0.2f;
    [SerializeField] private Animator _animator;

    private bool _isOnCooldown = false;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (_isOnCooldown) return;
        if (collision.rigidbody == null) return;

        foreach (ContactPoint2D contact in collision.contacts)
        {
            if (contact.point.y > transform.position.y)
            {
                Bounce(collision.rigidbody);
                break;
            }
        }
    }

    private void Bounce(Rigidbody2D rb)
    {
        if (_animator != null)
        {
            _animator.SetTrigger("Spring");
        }

        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
        rb.AddForce(Vector2.up * _bounceForce, ForceMode2D.Impulse);

        AudioManager.Instance.PlaySFX(SFXType.Bounce);
        StartCoroutine(Cooldown());
    }

    private System.Collections.IEnumerator Cooldown()
    {
        _isOnCooldown = true;
        yield return new WaitForSeconds(_cooldown);
        _isOnCooldown = false;
    }
}
