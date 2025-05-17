using UnityEngine;
using Mirror;

public class Bullet : NetworkBehaviour
{
    public float lifetime = 5f;
    public PlayersController Shooter { get; set; }

    private Vector3 _direction;
    private float _speed;

    [SerializeField] private Rigidbody2D _rb;
    [SerializeField] private SpriteRenderer _spriteRenderer;
    [SerializeField] private TrailRenderer _trail;

    public void Launch(Vector3 dir, float speed)
    {
        _direction = dir.normalized;
        _speed = speed;

        if (isServer)
            _rb.linearVelocity = _direction * _speed;

        Destroy(gameObject, lifetime);
    }

    [ServerCallback]
    private void FixedUpdate()
    {
        _rb.linearVelocity = _direction * _speed;
    }

    public void SetColor(Color color)
    {
        if (_spriteRenderer != null) _spriteRenderer.color = color;
    }

    public void SetTrailColor(Color color)
    {
        if (_trail != null)
        {
            _trail.startColor = color;
            _trail.endColor = color;
        }
    }

    [ServerCallback]
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!isServer) return;

        if (collision.gameObject.CompareTag("Player"))
        {
            PlayersController target = collision.gameObject.GetComponent<PlayersController>();
            if (target != null && target != Shooter)
            {
                target.CmdTakeHit(1, Shooter.gameObject, true);
                NetworkServer.Destroy(gameObject);
            }
        }
        else
        {
            NetworkServer.Destroy(gameObject);
        }
    }
}
