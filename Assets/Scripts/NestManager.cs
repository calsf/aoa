using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NestManager : MonoBehaviour
{
    private const int OBJECT_SEPARATION = 5;
    private const int FAR_SEPARATION = 20;

    [SerializeField] private int startNum;
    [SerializeField] private GameObject nest;
    private List<GameObject> nestList;
    private List<EnemySpawnManager> enemySpawners;

    private Grid3D grid;

    private LayerMask farMask;
    private LayerMask objectMask;

    void Start()
    {
        grid = GameObject.FindGameObjectWithTag("GridAir").GetComponent<Grid3D>();

        // Far mask includes Player and other Nests, should have more separation from each other
        farMask = new LayerMask();
        farMask = (1 << LayerMask.NameToLayer("Player")
            | 1 << LayerMask.NameToLayer("Nest"));

        objectMask = new LayerMask();
        objectMask = (1 << LayerMask.NameToLayer("Enemy")
            | 1 << LayerMask.NameToLayer("Wall")
            | 1 << LayerMask.NameToLayer("Altar"));

        nestList = new List<GameObject>();
        enemySpawners = new List<EnemySpawnManager>();
        GameObject[] spawnerObjs = GameObject.FindGameObjectsWithTag("EnemySpawnManager");

        foreach (GameObject obj in spawnerObjs)
        {
            enemySpawners.Add(obj.GetComponent<EnemySpawnManager>());
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

            GameObject newNest = Instantiate(nest, Vector3.zero, Quaternion.identity);
            newNest.transform.position = spawnPos;
            newNest.SetActive(true);

            nestList.Add(newNest);
        }

        // TEMPORARY!!!!!
        InvokeRepeating("SpawnFromNest", 6, 3);
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

        enemyType.Spawn(spawnPos);
    }
}
