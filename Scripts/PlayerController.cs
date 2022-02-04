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

    private void CastReflectingRay()
    {
        float distanceTossed = 0;
        Vector2 launchDirection = (Camera.main.ScreenToWorldPoint(Input.mousePosition) - transform.position).normalized;
        Vector3 startPoint = transform.position;
        int iterations = 0;
        while (distanceTossed < tossDistance && iterations < 10)
        {
            RaycastHit2D hit = Physics2D.Raycast(startPoint, launchDirection, tossDistance, collisionLayer);
            if (hit)
            {
                Debug.DrawLine(startPoint, hit.point, Color.red, 2);
                distanceTossed += hit.distance;
                launchDirection = Vector2.Reflect(launchDirection, hit.normal);
                startPoint = hit.point + launchDirection * 0.1f;
            }
            else
            {
                Debug.DrawLine(startPoint, startPoint + (Vector3)launchDirection * tossDistance, Color.red, 2);
                distanceTossed = tossDistance;
            }
            iterations++;
        }
    }

    public void PickupDisc()
    {
        discHeld = true;
    }
}
