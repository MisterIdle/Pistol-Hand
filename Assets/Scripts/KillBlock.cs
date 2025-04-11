using UnityEngine;

public class KillBlock : MonoBehaviour
{
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            var playerController = collision.gameObject.GetComponent<PlayerController>();
            if (playerController != null)
            {
                playerController.KillPlayer();
            }
        }
    }
}
