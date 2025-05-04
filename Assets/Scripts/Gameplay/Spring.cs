using UnityEngine;

public class Spring : MonoBehaviour
{
    [SerializeField] private float _bounceForce = 10f;
    [SerializeField] private Animator _animator;

    private bool _isOnCooldown = false;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (_isOnCooldown) return;

        if (collision.gameObject.CompareTag("Player") && collision.rigidbody != null)
        {
            if (collision.rigidbody.linearVelocity.y <= 0)
            {
                Vector2 contactPoint = collision.contacts[0].point;

                if (contactPoint.y > transform.position.y)
                {
                    if (_animator != null)
                    {
                        _animator.SetTrigger("Spring");
                    }

                    collision.rigidbody.AddForce(Vector2.up * _bounceForce, ForceMode2D.Impulse);
                }
            }
        }
    }
}
