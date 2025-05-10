using UnityEngine;

public class Crate : MonoBehaviour
{
    public void OnCollisionEnter2D(Collision2D collision)
    {
        AudioManager.Instance.PlaySFX(SFXType.Crate);
    }
}
