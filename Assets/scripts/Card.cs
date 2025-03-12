using UnityEditor.Callbacks;
using UnityEngine;
using UnityEngine.UIElements;

public class Card : MonoBehaviour
{
    private Rigidbody2D Rb;
    private Color Color;
    private float BoundX;
    private float BoundY;
    Vector3 screenBounds;

    private void Start()
    {
        Rb = GetComponent<Rigidbody2D>();
        Color = GetComponent<SpriteRenderer>().color;
        BoundX = GetComponent<Renderer>().bounds.size.x / 2;
        BoundY = GetComponent<Renderer>().bounds.size.y / 2;
        screenBounds = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, Camera.main.transform.position.z));
    }
    private void FixedUpdate()
    {
        FadeIn();
    }

    private void Update()
    {
        PreventFromExitScreen();
        IsbeingClicked();
    }

    public void FadeIn()
    {
        if (Color.a < 1f) 
        {
            Color.a += 0.01f;
            GetComponent<SpriteRenderer>().color = Color;
        } 
    }

    public void PreventFromExitScreen()
    {
        if (transform.position.x + BoundX> screenBounds.x)
        {
            transform.Translate(Vector2.left * 0.1f);
        }
        else if (transform.position.x - BoundX< -screenBounds.x)
        {
            transform.Translate(Vector2.right * 0.1f);
        }

        if (transform.position.y + BoundY> screenBounds.y)
        {
            transform.Translate(Vector2.down * 0.1f);
        }
        else if (transform.position.y - BoundY< -screenBounds.y)
        {
            transform.Translate(Vector2.up * 0.1f);
        }
    }

    public void IsbeingClicked()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 MousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Collider2D Collider = Physics2D.OverlapPoint(MousePos);

            if (Collider != null && Collider.gameObject == gameObject)
            {
                Debug.Log("I'm heeeeeeere");
            }
        }
    }
}