using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawnManager : MonoBehaviour
{
    private const int OBJECT_SEPARATION = 3;
    private const int PLAYER_SEPARATION = 50;

    [SerializeField] private int startNum;
    [SerializeField] private GameObject enemy;
    protected List<GameObject> enemyPool;
    [SerializeField] private bool isGrounded; // If true, spawn at original object y position
    private Grid3D grid;

    private LayerMask playerMask;
    private LayerMask objectMask;

    public List<GameObject> activeEnemies { get; set; }

    public int maxNum { get { return startNum * 2; } }

    [SerializeField] public float scalingMoveSpeed;
    [SerializeField] public float scalingHealth;
    [SerializeField] public float scalingDamage;

    void Start()
    {
        grid = GameObject.FindGameObjectWithTag("GridAir").GetComponent<Grid3D>();
 
        playerMask = new LayerMask();
        playerMask = 1 << LayerMask.NameToLayer("Player");

        objectMask = new LayerMask();
        objectMask = (1 << LayerMask.NameToLayer("Enemy")
            | 1 << LayerMask.NameToLayer("Boundary")
            | 1 << LayerMask.NameToLayer("Wall")
            | 1 << LayerMask.NameToLayer("Nest")
            | 1 << LayerMask.NameToLayer("Altar"));

        // Initialize pool of enemies
        enemyPool = new List<GameObject>();
        activeEnemies = new List<GameObject>();
        for (int i = 0; i < maxNum; i++)
        {
            enemyPool.Add(Instantiate(enemy, Vector3.zero, Quaternion.identity));
            Enemy newEnemy = enemyPool[i].GetComponent<Enemy>();
            
            newEnemy.SetIgnoreNestCollision(true); // Initially ignore nest collision
            
            InitializeEnemyStats(newEnemy); // Initialize stats

            enemyPool[i].SetActive(false);
        }

        // Spawn enemies within grid bounds
        for (int i = 0; i < startNum; i++)
        {
            Vector3 spawnPos = Vector3.zero;
 
            do
            {
                float x = Random.Range(-grid.gridBounds.x, grid.gridBounds.x);
                float y = Random.Range(0, grid.gridBounds.y); // Min Y bound should be 0
                float z = Random.Range(-grid.gridBounds.z, grid.gridBounds.z);

                if (isGrounded)
                {
                    y = enemy.transform.position.y;
                }

                spawnPos = new Vector3(x, y, z);

            } while (Physics.CheckSphere(spawnPos, OBJECT_SEPARATION, objectMask) || Physics.CheckSphere(spawnPos, PLAYER_SEPARATION, playerMask)); // Keep certain distance between objects and player

            // Get unused enemy and set position
            GameObject newEnemy = GetFromPool(enemyPool, enemy);
            newEnemy.transform.position = spawnPos;
            newEnemy.GetComponent<Enemy>().SetIgnoreNestCollision(false); // Reset to collide with nest since these initial spawns will not spawn inside a nest
            newEnemy.SetActive(true);

            activeEnemies.Add(newEnemy);
        }
    }

    private void FixedUpdate()
    {
        // Clear inactive enemies from list
        activeEnemies.RemoveAll(enemy => !enemy.activeInHierarchy);
    }

    // Initialize enemy stats with scaling values
    private void InitializeEnemyStats(Enemy enemy)
    {
        enemy.InitializeStats(
            enemy.GetBaseMoveSpeed() * scalingMoveSpeed,
            enemy.GetBaseHealth() * scalingHealth,
            enemy.GetBaseDamage() * scalingDamage
            );
    }

    private GameObject GetFromPool(List<GameObject> pool, GameObject obj)
    {
        for (int i = 0; i < pool.Count; i++)
        {
            if (!pool[i].activeInHierarchy)
            {
                return pool[i];
            }
        }

        // If no object in the pool is available, create a new object and add to the pool
        // SHOULD NEVER REACH THIS SINCE WE CAP MAX NUMBER OF ENEMIES/INITIALIZED THE POOL WITH THE CAP
        GameObject newObj = Instantiate(obj, Vector3.zero, Quaternion.identity);
        pool.Add(newObj);
        return newObj;
    }

    // Spawn at enemy at given position (usually from nest)
    // By default, does not aggro on spawn but can specify to do so
    public bool Spawn(Vector3 pos, bool aggroOnSpawn = false)
    {
        // Cap number of enemies that can be spawned
        if (activeEnemies.Count > maxNum)
        {
            return false; // Return false, no enemy spawned
        }

        GameObject newEnemy = GetFromPool(enemyPool, enemy);
        Enemy enemyBehavior = newEnemy.GetComponent<Enemy>();
        enemyBehavior.SetIgnoreNestCollision(true);

        // Aggro on spawn if specified
        if (aggroOnSpawn)
        {
            enemyBehavior.isAggro = true;
        }

        newEnemy.transform.position = pos;
        newEnemy.SetActive(true);
        activeEnemies.Add(newEnemy);

        Debug.Log(newEnemy + "spawned");

        return true; // Return true, enemy spawned
    }
}
