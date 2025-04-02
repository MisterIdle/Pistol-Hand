using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed = 40f;
    public int damage = 1;
    public Rigidbody2D rb;

    public PlayerController shooter;
    void Start()
    {
        Destroy(gameObject, 5f);

        rb.linearVelocity = transform.right * speed;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.gameObject.tag == "Player")
        {
            PlayerController players = collision.gameObject.GetComponent<PlayerController>();

            if (players != shooter)
            {
                players.HitPlayer(5, gameObject, players, true);
                Destroy(gameObject);
            } 
        }
    }
}