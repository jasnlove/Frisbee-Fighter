using UnityEngine;
using UnityEngine.InputSystem;
using States;
using static PlayerStateNames; //See new StateNames class under the StateMachine folder.

[RequireComponent(typeof(PlayerInput))]
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [SerializeField] private float speed = 3.0f;
    [SerializeField] private float launchSpeed = 6.0f;
    [SerializeField] private float slamDelay = 0.6f;
    [SerializeField] private GameObject discPrefab;
    [SerializeField] private GameObject slamPrefab;
    [SerializeField] private LayerMask discLayer;
    [SerializeField] private LayerMask collisionLayer = 3;

    private Rigidbody2D body;

    private InputActionMap map;
    private InputAction mousePos;
    private InputAction movement;
    private InputAction toss;

    private Vector2 input;

    private StateMachine stateMachine;

    private bool slam = false;
    private float slamTimer;

    private void Awake()
    {
        map = GetComponent<PlayerInput>().currentActionMap;
        body = GetComponent<Rigidbody2D>();
        movement = map.FindAction("Movement");
        toss = map.FindAction("Toss");
        mousePos = map.FindAction("MousePosition");

        slamTimer = slamDelay;

        //See documentation in statemachine folder
        stateMachine = new StateMachineBuilder()
            .WithState(HasDisc)
            .WithOnEnter( () => toss.performed += ThrowDisc)
            .WithOnExit( () => toss.performed -= ThrowDisc)
            .WithTransition(NoDisc, () => { return toss.triggered; })

            .WithState(NoDisc)
            .WithTransition(HasDisc, () => { return CheckForDisc(); })

            .Build();
    }

    // Update is called once per frame
    private void Update()
    {
        stateMachine.RunStateMachine();
        input = movement.ReadValue<Vector2>();

        if (slam)
        {
            slamTimer -= Time.deltaTime;
            if (slamTimer < 0)
            {
                Debug.Log("SLAM!");
                speed = speed * 2;
                slamTimer = slamDelay;
                Instantiate(slamPrefab, body.position, Quaternion.identity);
                slam = false;
            }
        }
    }

    private void FixedUpdate()
    {
        Move(input);
    }

    public string ReturnCurrentState()
    {
        return stateMachine.CurrentState.Name;
    }

    private void Move(Vector2 input)
    {
        input = Vector2.ClampMagnitude(input, 1f);
        body.MovePosition(body.position + speed * input * Time.deltaTime);
    }

    private void OnSlam()
    {
        GameObject disc = GameObject.Find("Disc(Clone)");
        if (disc == null && slam == false)
        {
            Debug.Log("JUMPING!");
            slam = true;
            speed = speed / 2.0f;
        }
    }

    //Rewrote throw just to include the new input system instead of a flag for disc held
    private void ThrowDisc(InputAction.CallbackContext context)
    {
        Vector2 launchDirection = Camera.main.ScreenToWorldPoint(mousePos.ReadValue<Vector2>()) - transform.position;
        launchDirection.Normalize();
        GameObject discObject = Instantiate(discPrefab, body.position, Quaternion.identity);
        DiscController disc = discObject.GetComponent<DiscController>();
        disc.Launch(launchDirection, launchSpeed, collisionLayer);
    }

    private bool CheckForDisc()
    {
        Collider2D[] col = Physics2D.OverlapBoxAll(transform.position, transform.localScale, discLayer);
        foreach (Collider2D c in col)
        {
            DiscController disc = c.GetComponent<DiscController>();
            if (disc && disc.pickupReady)
            {
                Destroy(disc.gameObject);
                return true;
            }
        }
        return false;
    }
}
