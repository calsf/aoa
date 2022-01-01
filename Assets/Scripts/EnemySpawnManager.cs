using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawnManager : MonoBehaviour
{
    private const int OBJECT_SEPARATION = 3;
    private const int PLAYER_SEPARATION = 80;

    [SerializeField] private int startNum;
    [SerializeField] private GameObject enemy;
    protected List<GameObject> enemyPool;
    [SerializeField] private bool isGrounded; // If true, spawn at original object y position
    private Grid3D grid;

    private LayerMask playerMask;
    private LayerMask objectMask;

    public List<GameObject> activeEnemies { get; set; }

    void Start()
    {
        grid = GameObject.FindGameObjectWithTag("GridAir").GetComponent<Grid3D>();
 
        playerMask = new LayerMask();
        playerMask = 1 << LayerMask.NameToLayer("Player");

        objectMask = new LayerMask();
        objectMask = (1 << LayerMask.NameToLayer("Enemy")
            | 1 << LayerMask.NameToLayer("Wall")
            | 1 << LayerMask.NameToLayer("Nest")
            | 1 << LayerMask.NameToLayer("Altar"));

        // Initialize pool of enemies
        enemyPool = new List<GameObject>();
        activeEnemies = new List<GameObject>();
        for (int i = 0; i < startNum * 2; i++)
        {
            enemyPool.Add(Instantiate(enemy, Vector3.zero, Quaternion.identity));
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
            newEnemy.SetActive(true);

            activeEnemies.Add(newEnemy);
        }
    }

    private void FixedUpdate()
    {
        // Clear inactive enemies from list
        activeEnemies.RemoveAll(enemy => !enemy.activeInHierarchy);
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
        GameObject newObj = Instantiate(obj, Vector3.zero, Quaternion.identity);
        pool.Add(newObj);
        return newObj;
    }

    public void Spawn(Vector3 pos)
    {
        // Cap number of enemies that can be spawned
        if (activeEnemies.Count > startNum * 2)
        {
            return;
        }

        GameObject newEnemy = GetFromPool(enemyPool, enemy);
        newEnemy.transform.position = pos;
        newEnemy.SetActive(true);
        activeEnemies.Add(newEnemy);

        Debug.Log(newEnemy + "spawned");
    }
}
