using UnityEngine;

public class Saw : MonoBehaviour
{
    [SerializeField] private int _hitForce = 1;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            var playerController = collision.gameObject.GetComponent<PlayersController>();
            if (playerController != null)
            {
                playerController.CmdTakeHit(_hitForce, gameObject, false);
                AudioManager.Instance.PlaySFX(SFXType.Saw);
            }
        }
    }
}
