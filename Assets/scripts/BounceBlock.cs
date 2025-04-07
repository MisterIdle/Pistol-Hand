using UnityEngine;

public class BounceBlock : MonoBehaviour
{
    public float bounceForce = 10f;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Rigidbody2D playerRigidbody = collision.gameObject.GetComponent<Rigidbody2D>();
            if (playerRigidbody != null)
            {
                playerRigidbody.AddForce(Vector2.up * bounceForce, ForceMode2D.Impulse);
            }
        }
    }
}
