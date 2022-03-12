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
    private Vector3 originalPos;
    private Vector3 point;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
        discLayer = LayerMask.GetMask("Disc");
        collisionLayer = LayerMask.GetMask("Collision");
        playerLayer = LayerMask.GetMask("Player");
        stayMachine = PatrolStates();
        chargeMachine = ChargeStates();

        enemyBehaviours = new StateMachineBuilder()
            .WithState(Stay)
            .WithOnEnter(() => originalPos = transform.position)
            .WithOnEnter(() => stayMachine.ResetStateMachine())
            .WithOnRun(() => stayMachine.RunStateMachine())
            .WithTransition(Flee, () => DistanceFromPlayer() < 5)
            .WithTransition(Charge, () => priority)

            .WithState(Charge)
            .WithOnEnter(() => chargeMachine.ResetStateMachine())
            .WithOnRun(() => chargeMachine.RunStateMachine())
            .WithTransition(Stay, () => !priority && DistanceFromPlayer() >= 5)

            .WithState(Flee)
            .WithOnRun(() => MoveAwayFromPlayer())
            .WithTransition(Charge, () => priority)
            .WithTransition(Stay, () => !priority && DistanceFromPlayer() >= 5)

            .WithState(Stunned)
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
    }

    private void HurtPlayer(){
        if(enemyBehaviours.CurrentState.Name == Stunned) return;
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
                    Destroy(gameObject);
                return true;
            }
        }
        return false;
    }

    private void OnDestroy(){
        Director.Instance.EnemiesSpawned.Remove(this.gameObject);
    }

    private Vector3 SetPointAroundSpotWithError(Vector3 point, Vector2 bounds){
        return point + new Vector3(Random.Range(-bounds.x, bounds.x), Random.Range(-bounds.y, bounds.y), 0);
    }

    private Vector3 SetPointAroundSpotWithError(Vector3 point, float radius){
        return point + new Vector3(Random.Range(-radius, radius), Random.Range(-radius, radius), 0);
    }

    private bool DetectWall(){
        return Physics2D.OverlapBox(transform.position, transform.localScale * 1.25f, 0, collisionLayer);
    }

    private StateMachine ChargeStates(){
        return new StateMachineBuilder()
            .WithState(SetPointCharge)
            .WithOnEnter(() => chargePoint = SetPointAroundSpotWithError(player.transform.position, locationalError))
            .WithOnEnter(() => chargeDir = (chargePoint - transform.position).normalized)
            .WithOnExit(() => timer = minimumChargeTime)
            .WithTransition(ChargeToPoint, () => true)

            .WithState(ChargeToPoint)
            .WithOnRun(() => timer -= Time.deltaTime)
            .WithOnRun(() => transform.position += chargeDir * chargeSpeed * Time.deltaTime)
            .WithTransition(SetPointCharge, () => ((timer <= 0 &&  Vector3.Magnitude(transform.position - player.transform.position) >= 5)) || DetectWall())
            .Build();
    }

    private StateMachine PatrolStates(){
        return new StateMachineBuilder()
            .WithState(SetPoint)
            .WithOnEnter(() => point = SetPointAroundSpotWithError(originalPos, patrolRadius))
            .WithTransition(GoToPoint, () => true)

            .WithState(GoToPoint)
            .WithOnRun(() => transform.position += (transform.position - point).normalized * patrolSpeed * Time.deltaTime)
            .WithTransition(SetPoint, () => Vector3.Magnitude(transform.position - originalPos) >= patrolRadius || DetectWall())
            .Build();
    }

    private void OnDrawGizmos(){
        if(enemyBehaviours == null) return;
        if(enemyBehaviours.CurrentState.Name == Stay){
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(originalPos, patrolRadius);
        }
    }
}
