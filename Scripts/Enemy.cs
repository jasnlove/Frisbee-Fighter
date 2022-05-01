using UnityEngine;
using States;
using static EnemyStateNames;
using static PlayerStateNames;

[RequireComponent(typeof(Rigidbody2D))]
public class Enemy : MonoBehaviour
{
    public float Bravery = 1;
    public bool priority = false;

    [SerializeField] private float moveSpeed = 6.0f;
    [SerializeField] private float stunTimer = 2f;
    private LayerMask discLayer;
    private LayerMask collisionLayer;
    private LayerMask playerLayer;

    private StateMachine enemyBehaviours;
    private StateMachine stayMachine;
    private StateMachine chargeMachine;

    private PlayerController player;
    private Rigidbody2D rb;
    private float setFreeTime;

    [Header("Charge state information")]
    [SerializeField] private float chargeSpeed;
    [SerializeField] private Vector2 locationalError;
    [SerializeField] private float minimumChargeTime = 0.25f;
    private float timer;
    private Vector3 chargeDir;
    private Vector3 chargePoint;

    [Header("Patrol state information")]
    [SerializeField] private float patrolSpeed = 1.5f;
    [SerializeField] private float patrolRadius = 2f;

    [Header("Sprite List")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Sprite[] spriteList;

    private Vector3 originalPos;
    private Vector3 point;

    [SerializeField] private GameObject explosion;

    private int choice = 0;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
        discLayer = LayerMask.GetMask("Disc");
        collisionLayer = LayerMask.GetMask("Collision");
        playerLayer = LayerMask.GetMask("Player");
        stayMachine = PatrolStates();
        chargeMachine = ChargeStates();

        spriteRenderer.sprite = spriteList[1];

        enemyBehaviours = new StateMachineBuilder()
            .WithState(Stay)
            .WithOnEnter(() => originalPos = transform.position)
            .WithOnEnter(() => stayMachine.ResetStateMachine())
            .WithOnRun(() => stayMachine.RunStateMachine())
            .WithTransition(Flee, () => DistanceFromPlayer() < 5)
            .WithTransition(Charge, () => priority)

            .WithState(Charge)
            .WithOnEnter(() => choice = Random.Range(0,2))
            .WithOnEnter(() => chargeMachine.ResetStateMachine())
            .WithOnRun(() => AttackChoice(choice))
            .WithTransition(Stay, () => !priority && DistanceFromPlayer() >= 5)
            .WithTransition(Flee, () => priority && Bravery <= 0.5 && (player.ReturnCurrentState() == Slam || player.ReturnCurrentState() == SlamHit))

            .WithState(Flee)
            .WithOnRun(() => MoveAwayFromPlayer())
            .WithTransition(Charge, () => priority)
            .WithTransition(Stay, () => !priority && DistanceFromPlayer() >= 5)

            .WithState(Stunned)
            .WithOnEnter(() => priority = true)
            .WithOnEnter(() => setFreeTime = stunTimer)
            .WithOnRun(() => setFreeTime -= Time.deltaTime)
            .WithOnRun(() => DetectStun())
            .WithTransition(Flee, () => setFreeTime <= 0)
            .WithTransitionFromAnyState(() => enemyBehaviours.CurrentState.Name != Stunned && DetectStun())

            .Build();
    }

    // Update is called once per frame
    private void Update()
    {
        HurtPlayer();
        enemyBehaviours.RunStateMachine();

        if (enemyBehaviours.CurrentState.Name != Stunned)
        {
            spriteRenderer.sprite = spriteList[1];
        }

        if (LeftOfPlayer())
        {
            spriteRenderer.flipX = true;
        }
        else
        {
            spriteRenderer.flipX = false;
        }
    }

    private void HurtPlayer()
    {
        if (enemyBehaviours.CurrentState.Name == Stunned) return;
        Collider2D[] col = Physics2D.OverlapCircleAll(transform.position, transform.localScale.y / 2.0f, playerLayer);
        foreach (Collider2D c in col)
        {
            PlayerController player = c.GetComponent<PlayerController>();
            if (player != null)
                player.ChangeHealth(-1);
        }
    }

    private void MoveAwayFromPlayer()
    {
        transform.position = transform.position + (transform.position - player.transform.position).normalized * moveSpeed * Time.deltaTime;
    }

    private float DistanceFromPlayer()
    {
        return Mathf.Abs((player.transform.position - transform.position).magnitude);
    }

    // Returns true if this enemy is left of the player, false otherwise.  Useful for sprite flipping.
    private bool LeftOfPlayer()
    {
        return player.transform.position.x < transform.position.x;
    }
    private bool DetectStun()
    {
        Collider2D[] col = Physics2D.OverlapBoxAll(transform.position, transform.localScale, discLayer);
        foreach (Collider2D c in col)
        {
            DiscController disc = c.GetComponent<DiscController>();
            if (disc && disc.velocity.magnitude >= 0.5f)
            {
                disc.Charge(34);
                if (disc.hyperDisc)
                {
                    Explode();
                    Destroy(gameObject);
                }

                // Handle sprite update
                spriteRenderer.sprite = spriteList[3];
                if (LeftOfPlayer())
                {
                    spriteRenderer.flipX = true;
                }
                else
                {
                    spriteRenderer.flipX = false;
                }

                return true;
            }
        }
        return false;
    }

    private void OnDestroy()
    {
        Director.Instance.EnemiesSpawned.Remove(this.gameObject);
    }

    private Vector3 SetPointAroundSpotWithError(Vector3 point, Vector2 bounds)
    {
        return point + new Vector3(Random.Range(-bounds.x, bounds.x), Random.Range(-bounds.y, bounds.y), 0);
    }

    private Vector3 SetPointAroundSpotWithError(Vector3 point, float radius)
    {
        return point + new Vector3(Random.Range(-radius, radius), Random.Range(-radius, radius), 0);
    }

    private bool DetectWall()
    {
        return Physics2D.OverlapBox(transform.position, transform.localScale, 0, collisionLayer);
    }

    private StateMachine ChargeStates()
    {
        return new StateMachineBuilder()
            .WithState(SetPointCharge)
            .WithOnEnter(() => chargePoint = SetPointAroundSpotWithError(player.transform.position, locationalError))
            .WithOnEnter(() => chargeDir = (chargePoint - transform.position).normalized)
            .WithOnExit(() => timer = minimumChargeTime)
            .WithTransition(ChargeToPoint, () => true)

            .WithState(ChargeToPoint)
            .WithOnRun(() => timer -= Time.deltaTime)
            .WithOnRun(() => transform.position += chargeDir * chargeSpeed * Time.deltaTime)
            .WithTransition(SetPointCharge, () => ((timer <= 0 && Vector3.Magnitude(transform.position - player.transform.position) >= 5)) || DetectWall())
            .Build();
    }

    private void ChasePlayer(){
         transform.position = transform.position - (transform.position - player.transform.position).normalized * moveSpeed * Time.deltaTime;
    }

    private void AttackChoice(int choice){
        switch(choice){
            case 0:
                ChasePlayer();
                break;
            case 1:
                chargeMachine.RunStateMachine();
                break;
        }
    }

    private StateMachine PatrolStates()
    {
        return new StateMachineBuilder()
            .WithState(SetPoint)
            .WithOnEnter(() => point = SetPointAroundSpotWithError(originalPos, patrolRadius))
            .WithTransition(GoToPoint, () => true)

            .WithState(GoToPoint)
            .WithOnRun(() => transform.position = Vector3.MoveTowards(transform.position, point, patrolSpeed * Time.deltaTime))
            .WithTransition(SetPoint, () => Vector3.Magnitude(transform.position - originalPos) >= patrolRadius || DetectWall() || Vector3.Magnitude(transform.position - point) <= 0.05f)
            .Build();
    }

    private void OnDrawGizmos()
    {
        if (enemyBehaviours == null) return;
        if (enemyBehaviours.CurrentState.Name == Stay)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(originalPos, patrolRadius);
        }
    }

    public void Explode()
    {
        Instantiate(explosion);
    }
}
