using UnityEngine;

public class KillBlock : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            print("Player has entered the kill block!");
        }
    }
}
