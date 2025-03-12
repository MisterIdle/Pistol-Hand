using UnityEngine;

public class CardsSpawn : MonoBehaviour
{
    public GameObject CardPrefab;
    public int NbCards = 3;
    private float BoundX;
    private float BoundY;
    private float Rotate = 35f;

    private void Awake()
    {
        BoundX = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, 0, 0)).x - CardPrefab.GetComponent<Renderer>().bounds.size.x / 2;
        BoundY = Camera.main.ScreenToWorldPoint(new Vector3(0, Screen.height, 0)).y - CardPrefab.GetComponent<Renderer>().bounds.size.y / 2;
    }

    private void Start()
    {
        spawnCards();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
        }
    }

    public void spawnCards()
    {
        for (int i = 0; i < NbCards; i++)
        {
            GameObject card = Instantiate(CardPrefab, new Vector3(Random.Range(-BoundX, BoundX), Random.Range(-BoundY, BoundY), 0), Quaternion.identity);
            card.transform.Rotate(0, 0, Random.Range(-Rotate, Rotate));
        }
    }
}
