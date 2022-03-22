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
    [SerializeField] private float timeInvincible = 0.5f;
    [SerializeField] private int maxHealth = 5;
    [SerializeField] private GameObject discPrefab;
    [SerializeField] private GameObject hyperDiscPrefab;
    [SerializeField] private GameObject slamPrefab;
    [SerializeField] private LayerMask discLayer;
    [SerializeField] private LayerMask collisionLayer = 3;

    [SerializeField] public SpriteRenderer spriteRenderer;
    [SerializeField] public Sprite[] spriteArray;
    private Rigidbody2D body;

    private InputActionMap map;
    private InputAction mousePos;
    private InputAction movement;
    private InputAction toss;
    private InputAction slam;
    private InputAction pause;

    private Vector2 input;

    private StateMachine stateMachine;

    private float slamTimer;
    private float invincibleTimer;
    private int currentHealth;
    private int discCharge = 0;
    private bool isInvincible = false;
    private PauseMenu pauseMenu;

    private void Awake()
    {
        currentHealth = maxHealth;
        invincibleTimer = timeInvincible;
        map = GetComponent<PlayerInput>().currentActionMap;
        body = GetComponent<Rigidbody2D>();
        movement = map.FindAction("Movement");
        toss = map.FindAction("Toss");
        mousePos = map.FindAction("MousePosition");
        slam = map.FindAction("Slam");
        pause = map.FindAction("Pause");
        pauseMenu = GameObject.FindObjectOfType<PauseMenu>();
        pause.performed += ctx => pauseMenu.CheckPause();
        slamTimer = slamDelay;

        //See documentation in statemachine folder
        stateMachine = new StateMachineBuilder()
            .WithState(HasDisc)
            .WithOnEnter(() => toss.performed += ThrowDisc)
            .WithOnExit(() => toss.performed -= ThrowDisc)
            .WithTransition(NoDisc, () => { return toss.triggered; })
            .WithTransition(Slam, () => { return slam.triggered; })

            .WithState(NoDisc)
            .WithTransition(HasDisc, () => { return CheckForDisc(); })

            .WithState(Slam)
            .WithOnEnter(() => { OnSlam(); })
            .WithOnRun(() => slamTimer -= Time.deltaTime)
            .WithTransition(SlamHit, () => { return slamTimer <= 0; })

            .WithState(SlamHit)
            .WithOnEnter(() => { EndSlam(); Director.Instance.SuperSlams++; })
            .WithTransition(HasDisc, () => { return true; })

            .Build();
    }

    // Update is called once per frame
    private void Update()
    {
        stateMachine.RunStateMachine();
        input = movement.ReadValue<Vector2>();

        if (discCharge >= 100 && map.enabled)
            slam.Enable();
        else
            slam.Disable();

        if (isInvincible)
        {
            invincibleTimer -= Time.deltaTime;
            if (invincibleTimer < 0)
                isInvincible = false;
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

    private void ThrowDisc(InputAction.CallbackContext context)
    {
        Vector2 launchDirection = Camera.main.ScreenToWorldPoint(mousePos.ReadValue<Vector2>()) - transform.position;
        launchDirection.Normalize();

        if (Input.GetMouseButton(1) && discCharge >= 100)
        {
            GameObject discObject = Instantiate(hyperDiscPrefab, body.position, Quaternion.identity);
            DiscController disc = discObject.GetComponent<DiscController>();
            disc.Launch(launchDirection, launchSpeed, collisionLayer);
            Director.Instance.HyperTosses++;
        }
        else
        {
            GameObject discObject = Instantiate(discPrefab, body.position, Quaternion.identity);
            DiscController disc = discObject.GetComponent<DiscController>();
            disc.Launch(launchDirection, launchSpeed, collisionLayer);
        }
    }

    private void OnSlam()
    {
        speed = speed / 2.0f;
        slamTimer = slamDelay;
        isInvincible = true;
        invincibleTimer = slamDelay;
    }

    private void EndSlam()
    {
        discCharge = 0;
        speed = speed * 2;
        slamTimer = slamDelay;
        Instantiate(slamPrefab, body.position, Quaternion.identity);
    }

    private bool CheckForDisc()
    {
        Collider2D[] col = Physics2D.OverlapBoxAll(transform.position, transform.localScale, discLayer);
        foreach (Collider2D c in col)
        {
            DiscController disc = c.GetComponent<DiscController>();
            if (disc && disc.pickupReady)
            {
                if (disc.hyperDisc)
                    ChangeCharge(0);
                Destroy(disc.gameObject);
                return true;
            }
        }
        return false;
    }

    public void ChangeHealth(int amount)
    {
        if (amount < 0)
        {
            if (isInvincible)
                return;

            isInvincible = true;
            invincibleTimer = timeInvincible;
        }

        currentHealth = Mathf.Clamp(currentHealth + amount, 0, maxHealth);
        Debug.Log("Health: " + currentHealth + "/" + maxHealth);
    }

    public void ChangeCharge(int amount)
    {
        discCharge = Mathf.Clamp(discCharge + amount, 0, 100);
        if (amount == 0)
            discCharge = 0;
        Debug.Log("Disc Charge: " + discCharge + "/100");
    }

    public int GetCharge()
    {
        return discCharge;
    }


}
