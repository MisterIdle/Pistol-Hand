using UnityEngine;

public class Firework : MonoBehaviour
{
    public ParticleSystem Ps;

    void Start()
    {
        Destroy(gameObject, 1f);
        AudioManager.Instance.PlaySFX(SFXType.Firework);
    }

    public void SetColor(Color newRedColor)
    {
        var main = Ps.main;
        var gradient = main.startColor;

        if (gradient.mode == ParticleSystemGradientMode.TwoColors)
        {
            gradient.colorMax = newRedColor;
            gradient.colorMin = Color.white;
            main.startColor = gradient;
        }
    }
}
