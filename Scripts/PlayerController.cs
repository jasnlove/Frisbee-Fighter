using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [SerializeField] private float speed = 3.0f;
    [SerializeField] private float launchSpeed = 6.0f;
    [SerializeField] private float tossDistance = 10.0f;
    [SerializeField] private LayerMask collisionLayer = 3;
    [SerializeField] private GameObject discPrefab;

    private Rigidbody2D body;

    private InputActionMap map;
    private InputAction mousePos;
    private InputAction movement;
    private InputAction toss;
    private Vector2 input;

    private void Awake()
    {
        map = GetComponent<PlayerInput>().currentActionMap;
        body = GetComponent<Rigidbody2D>();
        movement = map.FindAction("Movement");
        toss = map.FindAction("Toss");
        mousePos = map.FindAction("MousePosition");
    }

    // Start is called before the first frame update
    private void Start()
    {
        toss.performed += ThrowDisc;
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

    //Rewrote throw just to include the new input system instead of a flag for disc held
    private void ThrowDisc(InputAction.CallbackContext context)
    {
        Vector2 launchDirection = Camera.main.ScreenToWorldPoint(mousePos.ReadValue<Vector2>()) - transform.position;
        launchDirection.Normalize();
        GameObject discObject = Instantiate(discPrefab, body.position, Quaternion.identity);
        DiscController disc = discObject.GetComponent<DiscController>();
        disc.Launch(launchDirection, tossDistance, launchSpeed, collisionLayer);
        toss.performed -= ThrowDisc;
    }

    public void PickupDisc()
    {
        toss.performed += ThrowDisc;
    }
}
