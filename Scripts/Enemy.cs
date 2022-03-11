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
        [SerializeField] private LayerMask discLayer = 7;

        private StateMachine enemyBehaviours;
        private PlayerController player;
        private Rigidbody2D rb;
        private float setFreeTime;
        private float liveTime;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
            enemyBehaviours = new StateMachineBuilder()
                .WithState(Stay)
                .WithOnRun(() => Pace())
                .WithTransition(Flee, () => DistanceFromPlayer() < 5)

                .WithState(Charge)
                .WithOnRun(() => MoveTowardsPlayer())
                .WithTransition(Flee, () => player.ReturnCurrentState() == Slam)
                .WithTransition(Stay, () => priority == false && DistanceFromPlayer() >= 6)

                .WithState(Flee)
                .WithOnRun(() => MoveAwayFromPlayer())
                .WithTransition(Charge, () => priority == true || DistanceFromPlayer() <= 3)
                .WithTransition(Stay, () => priority == false && DistanceFromPlayer() >= 5)

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
            Debug.Log(enemyBehaviours.CurrentState.Name);
        }

        private void MoveTowardsPlayer()
        {
            transform.position = transform.position + (player.transform.position - transform.position).normalized * moveSpeed * Time.deltaTime;
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
    }
}
