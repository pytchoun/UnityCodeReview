using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Which mask the GameObject can spawn")]
    [SerializeField] private LayerMask _spawnLayerMask;
    [Tooltip("Array of available spawners")]
    [SerializeField] private GameObject[] _spawnerArray;
    [Tooltip("Prefab of GameObject to spawn")]
    [SerializeField] private GameObject _pfEnemy;
    [Tooltip("List of spawned prefab GameObject")]
    [SerializeField] private List<GameObject> _enemyList = new List<GameObject>();
    [Tooltip("Waypoint to reach by enemies")]
    [SerializeField] private GameObject _waypoint;

    [Header("Variables")]
    [Tooltip("Current wave number")]
    [SerializeField] private int _waveNumber = 0;
    [Tooltip("Maximum wave number")]
    [SerializeField] private int _maxWaveNumber = 5;
    [Tooltip("Enemy is currently spawning")]
    [SerializeField] private bool _enemyIsSpawning;

    //public GameObject GetWaypoint()
    //{
    //    return _waypoint;
    //}

    private void Start()
    {
        _waveNumber = 0;
        _maxWaveNumber = 5;
        PlayerUI.Instance.SetZombieWaveNumberText(_waveNumber, _maxWaveNumber);
        ZombieController.OnAnyDead += ZombieController_OnAnyDead;
    }

    private void ZombieController_OnAnyDead(object sender, System.EventArgs e)
    {
        ZombieController zombieController = sender as ZombieController;
        RemoveGameobjectInList(zombieController.gameObject);
    }

    private void Update()
    {
        //return;
        TryToSpawnNextWave();
    }

    private void RemoveGameobjectInList(GameObject go)
    {
        _enemyList.Remove(go);
        PlayerUI.Instance.SetZombieRemainingText(_enemyList.Count);
    }

    private void TryToSpawnNextWave()
    {
        // Check if we have to do the next wave
        if (_enemyList.Count != 0 || _enemyIsSpawning || _waveNumber > _maxWaveNumber)
        {
            return;
        }

        _waveNumber++;
        if (_waveNumber > _maxWaveNumber)
        {
            string message = "Victory, the zombie invasion is over.";
            //Debug.Log(message);
            ShowWaveAnnouncementText(message);
        }
        else if (_waveNumber <= _maxWaveNumber)
        {
            _enemyIsSpawning = true;
            string message = "A new wave on the way.";
            ShowWaveAnnouncementText(message);
            StartCoroutine(SpawnZombie(2f));
        }
    }

    private void ShowWaveAnnouncementText(string message)
    {
        StartCoroutine(PlayerUI.Instance.EnableAnnouncement(true, message));
    }

    private void SpawnZombie()
    {
        for (int i = 0; i < 32; i++)
        {
            int randomSpawnerIndex = Random.Range(0, _spawnerArray.Length);
            Vector3 spawnLocation = _spawnerArray[randomSpawnerIndex].transform.position;
            Vector2 randomPointOnACircle = Random.insideUnitCircle;
            float radius = 5f;
            Vector3 randomPoint = new Vector3(randomPointOnACircle.x, 0f, randomPointOnACircle.y) * radius + spawnLocation;
            float maxDistance = 20f;
            if (!Physics.BoxCast(randomPoint, _pfEnemy.transform.localScale, transform.up, out RaycastHit hit, transform.rotation, maxDistance, _spawnLayerMask))
            {
                GameObject go = Instantiate(_pfEnemy, randomPoint, Quaternion.identity);
                go.GetComponent<ZombieController>().Setup(_waypoint.transform);
                _enemyList.Add(go);
                break;
            }
        }
    }

    private IEnumerator SpawnZombie(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        for (int i = 0; i < ZombieAmount(); i++)
        {
            SpawnZombie();
        }
        _enemyIsSpawning = false;
        PlayerUI.Instance.SetZombieWaveNumberText(_waveNumber, _maxWaveNumber);
        PlayerUI.Instance.SetZombieRemainingText(ZombieAmount());
    }

    private int ZombieAmount()
    {
        switch (_waveNumber)
        {
            case 1:
                return 10;
            case 2:
                return 12;
            case 3:
                return 15;
            case 4:
                return 18;
            case 5:
                return 20;
            default:
                return 10;
        }
    }
}