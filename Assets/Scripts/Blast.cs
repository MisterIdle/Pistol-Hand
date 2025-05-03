using UnityEngine;

public class Blast : MonoBehaviour
{
    public ParticleSystem ps;

    void Start()
    {
        Destroy(gameObject, 1f);
    }

    public void SetRedColor(Color newRedColor)
    {
        var main = ps.main;
        var gradient = main.startColor;

        if (gradient.mode == ParticleSystemGradientMode.TwoColors)
        {
            gradient.colorMax = newRedColor;
            gradient.colorMin = Color.white;
            main.startColor = gradient;
        }
    }
}
