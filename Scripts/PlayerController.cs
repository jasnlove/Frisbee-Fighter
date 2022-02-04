using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    public float speed = 3.0f;
    public float launchSpeed = 6.0f;
    public GameObject discPrefab;

    private Rigidbody2D body;

    private InputActionMap map;
    private InputAction movement;
    private InputAction disc;
    private Vector2 input;

    bool discHeld = true;

    private void Awake()
    {
        map = GetComponent<PlayerInput>().currentActionMap;
        body = GetComponent<Rigidbody2D>();
        movement = map.FindAction("Movement");
        disc = map.FindAction("Toss");
    }

    // Start is called before the first frame update
    private void Start()
    {
        disc.performed += ThrowDisc;
    }

    // Update is called once per frame
    private void Update()
    {
        input = movement.ReadValue<Vector2>();
    }

    private void FixedUpdate()
    {
        Move(input);
    }

    private void Move(Vector2 input)
    {
        input = Vector2.ClampMagnitude(input, 1f);
        body.MovePosition(body.position + speed * input * Time.deltaTime);
    }

    private void ThrowDisc(InputAction.CallbackContext context)
    {
        if (discHeld)
        {
            Vector2 launchDirection = Camera.main.ScreenToWorldPoint(Input.mousePosition) - transform.position;
            launchDirection.Normalize();
            GameObject discObject = Instantiate(discPrefab, body.position, Quaternion.identity);
            DiscController disc = discObject.GetComponent<DiscController>();
            disc.Launch(launchDirection, launchSpeed);
            discHeld = false;
        }
    }

    public void PickupDisc()
    {
        discHeld = true;
    }
}
