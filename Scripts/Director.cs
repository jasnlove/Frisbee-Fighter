using UnityEngine;
using System.Collections.Generic;
using States;
using static DirectorSpawnStateNames;

namespace FrisbeeThrow
{
    public class Director : MonoBehaviour
    {
        public List<Transform> spawnpoints;
        [SerializeField] private float _durationBetweenWaves = 15;
        [SerializeField] private int _enemiesToSpawn = 2;
        [SerializeField] private float _spawnDistanceFromPlayer = 5;
        [Header("Enemies")]
        [SerializeField] private GameObject[] _enemies;
        [Header("Weights")]
        [SerializeField] private int[] _weights;

        public List<GameObject> _enemiesSpawned = new List<GameObject>();
        private StateMachine _waveStateMachine;
        private float _timer;
        private GameObject _player;

        private void Awake(){
            _player = GameObject.FindGameObjectWithTag("Player");
            _waveStateMachine = new StateMachineBuilder()
                .WithState(InWave)
                .WithOnEnter(() => _timer = _durationBetweenWaves)
                .WithOnRun(() => _timer -= Time.deltaTime)
                .WithTransition(SpawnWave, () => _timer <= 0 || _enemiesSpawned.Count == 0)

                .WithState(SpawnWave)
                .WithOnEnter(() => SpawnEnemies())
                .WithTransition(InWave, () => true)
                .Build();
        }

        private void Update(){
            _waveStateMachine.RunStateMachine();
        }

        private void SpawnEnemies(){
            for(int i = 0; i < _enemiesToSpawn; i++){
                GameObject temp = Instantiate<GameObject>(Roll());
                _enemiesSpawned.Add(temp);
                int spawnRoll;
                do{
                    spawnRoll = Random.Range(0, spawnpoints.Count);
                }while(Vector2.SqrMagnitude(spawnpoints[spawnRoll].position - _player.transform.position) < _spawnDistanceFromPlayer);
                temp.transform.position = spawnpoints[spawnRoll].position;
                temp.GetComponent<Enemy>().d = this;
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

        private void OnDrawGizmos(){
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(GameObject.FindGameObjectWithTag("Player").transform.position, _spawnDistanceFromPlayer);
        }

    }
}
