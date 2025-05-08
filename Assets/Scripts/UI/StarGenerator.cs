using UnityEngine;
using System.Collections.Generic;

public class StarGenerator : MonoBehaviour
{
    public static StarGenerator Instance { get; private set; }

    [Header("Star Generation Settings")]
    public GameObject starPrefab;
    public int minStars = 10;
    public int maxStars = 50;
    public float minSize = 0.1f;
    public float maxSize = 0.5f;
    public float spawnRadius = 10f;
    public float minRotationSpeed = 10f;
    public float maxRotationSpeed = 100f;
    public int maxAttempts = 10;

    [Header("Alpha Settings")]
    [Range(0f, 1f)] public float minAlpha = 0.2f;
    [Range(0f, 1f)] public float maxAlpha = 1f;

    private List<GameObject> stars = new List<GameObject>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void GenerateStars()
    {
        ClearStars();

        int starCount = Random.Range(minStars, maxStars + 1);
        for (int i = 0; i < starCount; i++)
        {
            Vector3 position;
            int attempts = 0;
            do
            {
                Vector3 randomPos = Random.insideUnitSphere * spawnRadius;
                position = new Vector3(randomPos.x, randomPos.y, 0f);
                attempts++;
            }
            while (Physics2D.OverlapCircle(position, maxSize) != null && attempts < maxAttempts);

            if (attempts >= maxAttempts) continue;

            GameObject star = Instantiate(starPrefab, position, Quaternion.Euler(0f, 0f, Random.Range(0f, 360f)), transform);

            float scale = Random.Range(minSize, maxSize);
            star.transform.localScale = Vector3.one * scale;

            float rotationSpeed = Random.Range(minRotationSpeed, maxRotationSpeed);
            var rotator = star.AddComponent<Star>();
            rotator.speed = rotationSpeed;

            SetAlpha(star, Random.Range(minAlpha, maxAlpha));

            stars.Add(star);
        }
    }


    void SetAlpha(GameObject star, float alpha)
    {
        var renderers = star.GetComponentsInChildren<SpriteRenderer>();
        foreach (var r in renderers)
        {
            Color c = r.color;
            c.a = alpha;
            r.color = c;
        }
    }

    public void ClearStars()
    {
        foreach (var star in stars)
        {
            if (star != null)
                Destroy(star);
        }
        stars.Clear();
    }
}
