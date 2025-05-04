using UnityEngine;

public class Saw : MonoBehaviour
{
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            var playerController = collision.gameObject.GetComponent<PlayersController>();
            if (playerController != null)
            {
                playerController.KillPlayer();
            }
        }
    }
}
