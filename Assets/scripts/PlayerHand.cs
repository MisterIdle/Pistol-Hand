using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerHand : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float speed = 5f;
    [SerializeField] private float maxDistance = 5f;

    [Header("Input")]
    private Vector2 look;
    private PlayerController playerController;

    private void Start()
    {
        playerController = GetComponentInParent<PlayerController>();
    }

    private void Update()
    {
        HandleInput();
        ClampDistance();
    }

    private void HandleInput()
    {
        Move();
    }

    public void Move()
    {
        Vector3 direction = playerController.IsKeyboard ? KeyboardMove() : GamepadMove(look);
        transform.position = Vector3.MoveTowards(transform.position, transform.position + direction * speed, speed * Time.deltaTime);
    }

    private Vector3 GamepadMove(Vector3 look)
    {
        Vector3 direction = new Vector3(look.x, look.y, 0).normalized;
        return direction;
    }

    private Vector3 KeyboardMove()
    {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        mousePos.z = 0;

        Vector3 direction = (mousePos - transform.position).normalized;
        return direction;
    }

    private void ClampDistance()
    {
        float distance = Vector3.Distance(transform.position, playerController.transform.position);

        if (distance > maxDistance)
        {
            Vector3 direction = (transform.position - playerController.transform.position).normalized;
            transform.position = playerController.transform.position + direction * maxDistance;
        }
    }

    public void OnLook(InputAction.CallbackContext context)
    {
        look = context.ReadValue<Vector2>();
    }
}
