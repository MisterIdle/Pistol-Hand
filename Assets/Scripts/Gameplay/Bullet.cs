using UnityEngine;

public class Bullet : MonoBehaviour
{
    public PlayersController Shooter;

    void Start()
    {
        Destroy(gameObject, 3f);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            PlayersController players = collision.gameObject.GetComponent<PlayersController>();
            if (players != Shooter)
            {
                players.TakeHit(5, gameObject, true);
                players.LastHitBy = Shooter;
            }
        }

        Destroy(gameObject);
    }
}