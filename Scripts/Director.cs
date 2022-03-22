using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using States;
using static DirectorSpawnStateNames;

public class Director : MonoBehaviour
{
    public static Director Instance {get; private set;}
    public List<Transform> spawnpoints;
    public List<GameObject> EnemiesSpawned = new List<GameObject>();
    public int HyperTosses = 0;
    public int SuperSlams = 0;

    [SerializeField] private float _durationBetweenWaves = 15;
    [SerializeField] private int _enemiesToSpawn = 2;
    [SerializeField] private float _spawnDistanceFromPlayer = 5;
    [SerializeField] private int _enemiesWithPriority = 1;
    [SerializeField] private float _priorityCalculationDelay = 5;

    [Header("Enemies")]
    [SerializeField] private GameObject[] _enemies;

    [Header("Weights")]
    [SerializeField] private int[] _weights;

    private StateMachine _waveStateMachine;
    private float _timer;
    private GameObject _player;
    private float _priorityTimer = 0;

    private void Awake(){
        if(Instance == null)
            Instance = this;
        else
            Destroy(this);
        _player = GameObject.FindGameObjectWithTag("Player");
        _waveStateMachine = new StateMachineBuilder()
            .WithState(InWave)
            .WithOnEnter(() => _timer = _durationBetweenWaves)
            .WithOnRun(() => {_timer -= Time.deltaTime; if(EnemiesSpawned.Count == 0) _timer -= Time.deltaTime; })
            .WithTransition(SpawnWave, () => _timer <= 0)

            .WithState(SpawnWave)
            .WithOnEnter(() => HandlePriority())
            .WithOnEnter(() => SpawnEnemies())
            .WithTransition(InWave, () => true)
            .Build();
    }

    private void Update(){
        if(_priorityTimer <= 0){
            HandlePriority();
            _priorityTimer = _priorityCalculationDelay;
        }
        _priorityTimer -= Time.deltaTime;
        _waveStateMachine.RunStateMachine();
    }

    private void SpawnEnemies(){
        for(int i = 0; i < _enemiesToSpawn; i++){
            int spawnRoll;
            do{
                spawnRoll = Random.Range(0, spawnpoints.Count);
            }while(Vector2.SqrMagnitude(spawnpoints[spawnRoll].position - _player.transform.position) < _spawnDistanceFromPlayer);
            GameObject temp = Instantiate<GameObject>(Roll(), spawnpoints[spawnRoll].position, Quaternion.identity);
            EnemiesSpawned.Add(temp);
            temp.GetComponent<Enemy>().Bravery = Random.Range(0, 0.5f) + 0.25f + 0.25f* (HyperTosses + 1)/(SuperSlams + 1);
        }
    }

    private int GetWeight(){
        int sum = 0;
        foreach(int x in _weights){
            sum += x;
        }
        return sum;
    }

    private GameObject Roll(){
        int totalWeight = GetWeight();
        int roll = Random.Range(0, totalWeight);
        int currWeight = 0;
        for(int i = 0; i < _weights.Length; i++){
            if(roll < _weights[i] + currWeight){
                return _enemies[i];
            }
            currWeight += _weights[i];
        }
        Debug.Log("Failsafe spawn");
        return _enemies[0];
    }

    private void HandlePriority(){
        EnemiesSpawned = EnemiesSpawned.OrderBy(x => Vector3.SqrMagnitude(x.transform.position - _player.transform.position)).ToList<GameObject>();
        if(EnemiesSpawned.Count == 0){
            return;
        }
        for(int i = 0; i < _enemiesWithPriority; i++){
            EnemiesSpawned[i].GetComponent<Enemy>().priority = true;
        }
    }

    private void OnDrawGizmos(){
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(GameObject.FindGameObjectWithTag("Player").transform.position, _spawnDistanceFromPlayer);
    }

}

