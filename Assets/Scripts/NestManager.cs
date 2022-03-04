using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NestManager : MonoBehaviour
{
    private const int OBJECT_SEPARATION = 9;
    private const int FAR_SEPARATION = 12;
    private const float BASE_SPAWN_DELAY_MIN = 5;
    private const float BASE_SPAWN_DELAY_MAX = 10;
    private const float MIN_SPAWN_DELAY = 1;

    public int startNum;
    [SerializeField] private GameObject nest;
    public List<GameObject> nestList { get; set; }
    private List<EnemySpawnManager> enemySpawners;

    private Grid3D grid;

    private LayerMask farMask;
    private LayerMask objectMask;

    private float nextSpawnTime;
    private int maxActiveEnemies;

    void Start()
    {
        grid = GameObject.FindGameObjectWithTag("GridAir").GetComponent<Grid3D>();

        // Far mask includes Player and other Nests, should have more separation from each other
        farMask = new LayerMask();
        farMask = (1 << LayerMask.NameToLayer("Player")
            | 1 << LayerMask.NameToLayer("Nest"));

        objectMask = new LayerMask();
        objectMask = (1 << LayerMask.NameToLayer("Enemy")
            | 1 << LayerMask.NameToLayer("Boundary")
            | 1 << LayerMask.NameToLayer("Wall")
            | 1 << LayerMask.NameToLayer("Altar"));

        nestList = new List<GameObject>();
        enemySpawners = new List<EnemySpawnManager>();
        GameObject[] spawnerObjs = GameObject.FindGameObjectsWithTag("EnemySpawnManager");

        foreach (GameObject obj in spawnerObjs)
        {
            EnemySpawnManager enemySpawner = obj.GetComponent<EnemySpawnManager>();
            enemySpawners.Add(enemySpawner);

            maxActiveEnemies += enemySpawner.maxNum;
        }

        // Spawn within grid bounds
        for (int i = 0; i < startNum; i++)
        {
            Vector3 spawnPos = Vector3.zero;

            do
            {
                float x = Random.Range(-grid.gridBounds.x, grid.gridBounds.x);
                float y = 0;
                float z = Random.Range(-grid.gridBounds.z, grid.gridBounds.z);

                spawnPos = new Vector3(x, y, z);

            } while (Physics.CheckSphere(spawnPos, OBJECT_SEPARATION, objectMask) || Physics.CheckSphere(spawnPos, FAR_SEPARATION, farMask)); // Keep certain distance between objects

            GameObject newNest = Instantiate(nest, spawnPos, Quaternion.identity);
            newNest.GetComponent<Nest>().nestManager = this; // For all nests, reference this nest manager
            newNest.SetActive(true);

            nestList.Add(newNest);
        }

        // Initialize next spawn time
        nextSpawnTime = Time.time + Random.Range(BASE_SPAWN_DELAY_MIN, BASE_SPAWN_DELAY_MAX);
    }

    void Update()
    {
        nestList.RemoveAll(n => !n.activeInHierarchy); // Clear nests

        if (nextSpawnTime < Time.time)
        {
            // Calculate next spawn time based on total number of ALL enemies alive
            // The less enemies alive, the faster the next spawn, the more enemies alive, the slower the next spawn
            int currActiveEnemies = 0;
            foreach (EnemySpawnManager spawner in enemySpawners)
            {
                currActiveEnemies += spawner.activeEnemies.Count;
            }

            float activeEnemyPercent = (float) currActiveEnemies / maxActiveEnemies;

            nextSpawnTime = Time.time + (Random.Range(BASE_SPAWN_DELAY_MIN, BASE_SPAWN_DELAY_MAX) * activeEnemyPercent) + MIN_SPAWN_DELAY; // Add a min delay in case activeEnemyPercent is 0

            SpawnFromNest();
        }
    }

    // Spawn an enemy of random enemy type at one of the active nest positions
    public void SpawnFromNest()
    {
        nestList.RemoveAll(n => !n.activeInHierarchy); // Clear nest list before continuing

        // All nests destroyed
        if (nestList.Count == 0)
        {
            return;
        }

        // Get random enemy type
        EnemySpawnManager enemyType = enemySpawners[Random.Range(0, enemySpawners.Count)];
        
        Vector3 spawnPos = nestList[Random.Range(0, nestList.Count)].transform.position;

        bool spawned = enemyType.Spawn(spawnPos);
        
        // If failed to spawn because the selected enemy type is at max number of active enemies, wait a bit and then try to spawn an enemy type again
        if (!spawned)
        {
            nextSpawnTime = Time.time + 1;
        }
    }

    // Spawn an enemy of random type at given nest position when hit/damaged, will not retry
    public void SpawnFromNestHit(Transform nest)
    {
        // Get random enemy type
        EnemySpawnManager enemyType = enemySpawners[Random.Range(0, enemySpawners.Count)];

        Vector3 spawnPos = nest.position;

        // Spawning from nest on hit will auto aggro the spawned enemy
        enemyType.Spawn(spawnPos, true);
    }
}
