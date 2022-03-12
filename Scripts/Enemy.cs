using UnityEngine;
using States;
using static EnemyStateNames;
using static PlayerStateNames;

namespace FrisbeeThrow
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class Enemy : MonoBehaviour
    {
        public float Bravery = 1;
        public bool priority = false;

        [SerializeField] private float moveSpeed = 6.0f;
        [SerializeField] private float stunTimer = 2f;
        private LayerMask discLayer;
        private LayerMask collisionLayer;

        private StateMachine enemyBehaviours;
        private StateMachine stayMachine;
        private StateMachine chargeMachine;

        private PlayerController player;
        private Rigidbody2D rb;
        private float setFreeTime;
        private float liveTime;
        
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
                .WithTransition(Stay, () => !priority && DistanceFromPlayer() >= 6)

                .WithState(Flee)
                .WithOnRun(() => MoveAwayFromPlayer())
                .WithTransition(Charge, () => priority || DistanceFromPlayer() <= 3)
                .WithTransition(Stay, () => !priority && DistanceFromPlayer() >= 5)

                .WithState(Stunned)
                .WithOnEnter(() => setFreeTime = stunTimer)
                .WithOnRun(() => setFreeTime -= Time.deltaTime)
                .WithTransition(Flee, () => setFreeTime <= 0)
                .WithTransitionFromAnyState(() => enemyBehaviours.CurrentState.Name != Stunned && DetectStun())

                .Build();
        }

        // Update is called once per frame
        private void Update()
        {
            liveTime += 7 * Time.deltaTime;
            enemyBehaviours.RunStateMachine();
        }

        private void MoveAwayFromPlayer()
        {
            transform.position = transform.position + (transform.position - player.transform.position).normalized * moveSpeed * Time.deltaTime;
        }

        private float DistanceFromPlayer()
        {
            return Mathf.Abs((player.transform.position - transform.position).magnitude);
        }

        private void Pace(){
            Vector3 dir = (player.transform.position - transform.position);
            dir = Vector2.Perpendicular(dir).normalized * Mathf.Sign(Mathf.Cos(liveTime));
            transform.position += dir * moveSpeed/ 100;
        }

        private bool DetectStun()
        {
            Collider2D[] col = Physics2D.OverlapBoxAll(transform.position, transform.localScale, discLayer);
            foreach (Collider2D c in col)
            {
                DiscController disc = c.GetComponent<DiscController>();
                if (disc && disc.velocity.magnitude >= 0.25f)
                {
                    disc.Charge(34);
                    if (disc.hyperDisc)
                        Destroy(gameObject);
                    return true;
                }
            }
            return false;
        }

        private void OnCollisionEnter2D(Collision2D other)
        {
            PlayerController player = other.gameObject.GetComponent<PlayerController>();

            if (player != null)
            {
                player.ChangeHealth(-1);
            }
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
                .WithOnEnter(() => timer = minimumChargeTime)
                .WithOnEnter(() => chargePoint = SetPointAroundSpotWithError(player.transform.position, locationalError))
                .WithOnEnter(() => chargeDir = (chargePoint - transform.position).normalized)
                .WithTransition(ChargeToPoint, () => true)

                .WithState(ChargeToPoint)
                .WithOnRun(() => timer -= Time.deltaTime)
                .WithOnRun(() => transform.position += chargeDir * chargeSpeed * Time.deltaTime)
                .WithTransition(SetPointCharge, () => (timer <= 0 && (Vector3.Magnitude(transform.position - chargePoint) <= 0.05f || Vector3.Magnitude(transform.position - player.transform.position) >= 5)) || DetectWall())
                .Build();
        }

        private StateMachine PatrolStates(){
            return new StateMachineBuilder()
                .WithState(SetPoint)
                .WithOnEnter(() => point = SetPointAroundSpotWithError(originalPos, patrolRadius))
                .WithTransition(GoToPoint, () => true)

                .WithState(GoToPoint)
                .WithOnRun(() => transform.position = Vector3.MoveTowards(transform.position, point, patrolSpeed * Time.deltaTime))
                .WithTransition(SetPoint, () => Vector3.Magnitude(transform.position - originalPos) >= patrolRadius || DetectWall() || Vector3.Magnitude(transform.position - point) <= 0.2f)
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
}
