using UnityEngine;
using Mirror;

public class Bullet : NetworkBehaviour
{
    public PlayersController Shooter;
    public SpriteRenderer BulletSprite;
    public TrailRenderer Trail;

    void Start()
    {
        Destroy(gameObject, 3f);
    }

    public void SetColor(Color color)
    {
        if (BulletSprite != null)
        {
            BulletSprite.color = color;
        }
    }

    public void SetTrailColor(Color color)
    {
        if (Trail != null)
        {
            Trail.startColor = color;
            Trail.endColor = Color.white;
        }
    }

    public void Launch(Vector3 direction, float force)
    {
        var rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.AddForce(direction * force, ForceMode2D.Impulse);
        }
    }

    [ServerCallback]
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            PlayersController players = collision.gameObject.GetComponent<PlayersController>();
            if (players != Shooter)
            {
                players.TakeHit((int)players.PistolHitForce, gameObject, true);
                players.LastHitBy = Shooter;
                Destroy(gameObject);
            }
        }

        if (collision.gameObject.tag == "Bullet")
        {
            AudioManager.Instance.PlaySFX(SFXType.BulletHit);
        }

        if (collision.gameObject.layer == 3)
        {
            Destroy(gameObject);
        }
    }
}
