using UnityEngine;
using States;
using static EnemyStateNames;
using static PlayerStateNames;

namespace FrisbeeThrow
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class Enemy : MonoBehaviour
    {
        [SerializeField] private float moveSpeed = 6.0f;
        [SerializeField] private float stunTimer = 6.0f;

        private StateMachine enemyBehaviours;
        private PlayerController player;
        private Rigidbody2D rb;
        private float setFreeTime;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
            enemyBehaviours = new StateMachineBuilder()
                .WithState(Flee)
                .WithOnRun(()=>{MoveAwayFromPlayer();})
                .WithTransition(Charge, ()=>{ return player.ReturnCurrentState() == NoDisc || DistanceFromPlayer() < 3;})
                .WithTransition(StoodStill, () => { return DistanceFromPlayer() > 5 && player.ReturnCurrentState() == HasDisc;})

                .WithState(Charge)
                .WithOnRun(()=>{MoveTowardsPlayer();})
                .WithTransition(Flee, ()=>{ return player.ReturnCurrentState() == HasDisc && DistanceFromPlayer() < 10 && DistanceFromPlayer() > 3;})

                .WithState(StoodStill)
                .WithTransition(Flee, () => { return DistanceFromPlayer() < 5 && player.ReturnCurrentState() == HasDisc;})
                .WithTransition(Charge, () => { return player.ReturnCurrentState() == NoDisc || DistanceFromPlayer() > 10;})

                .WithState(Stunned)
                .WithOnEnter(()=>{setFreeTime = stunTimer;})
                .WithOnRun(()=>{setFreeTime -= Time.deltaTime;})
                .WithTransition(Flee, ()=>{return setFreeTime <= 0;})
                .WithTransitionFromAnyState(()=>{ return DetectDamage(); })
                .Build();
        }

        // Update is called once per frame
        private void Update()
        {
            enemyBehaviours.RunStateMachine();
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

        private bool DetectDamage()
        {
            //Write code here for how the enemy knows its hit.
            //Probably just circlecast and check for either the disc or the slam attack radius.
            //Alternatively this could be set to true somewhere else.
            return false;
        }
    }
}
