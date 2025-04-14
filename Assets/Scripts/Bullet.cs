using UnityEngine;

public class Bullet : MonoBehaviour
{
    public PlayerController shooter;

    void Start()
    {
        Destroy(gameObject, 3f);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            PlayerController players = collision.gameObject.GetComponent<PlayerController>();
            if (players != shooter)
            {
                players.TakeHit(5, gameObject, true);
            }
        }

        Destroy(gameObject);
    }
}