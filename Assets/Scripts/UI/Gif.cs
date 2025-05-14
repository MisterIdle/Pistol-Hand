using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Gif : MonoBehaviour
{
    [Header("Settings")]
    public List<Sprite> images;
    public float changeInterval = 0.5f;

    private Image uiImage;
    private int currentIndex = 0;
    private float timer = 0f;

    void Start()
    {
        uiImage = GetComponent<Image>();
    }

    void Update()
    {
        if (images == null || images.Count == 0 || uiImage == null)
            return;

        timer += Time.deltaTime;

        if (timer >= changeInterval)
        {
            timer = 0f;
            currentIndex = (currentIndex + 1) % images.Count;
            uiImage.sprite = images[currentIndex];
        }
    }
}