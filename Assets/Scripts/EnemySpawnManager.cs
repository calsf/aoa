using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawnManager : MonoBehaviour
{
    private const int OBJECT_SEPARATION = 3;
    private const int PLAYER_SEPARATION = 80;

    [SerializeField] private int startNum;
    [SerializeField] GameObject enemy;
    private Grid3D grid;

    private LayerMask playerMask;

    void Start()
    {
        grid = GameObject.FindGameObjectWithTag("Grid").GetComponent<Grid3D>();
        playerMask = new LayerMask();
        playerMask = 1 << LayerMask.NameToLayer("Player");

        // Spawn enemies within grid bounds
        for (int i = 0; i < startNum; i++)
        {
            Vector3 spawnPos = Vector3.zero;
 
            do
            {
                float x = Random.Range(-grid.gridBounds.x, grid.gridBounds.x);
                float y = Random.Range(0, grid.gridBounds.y); // Min Y bound should be 0
                float z = Random.Range(-grid.gridBounds.z, grid.gridBounds.z);

                spawnPos = new Vector3(x, y, z);

            } while (Physics.CheckSphere(spawnPos, OBJECT_SEPARATION) || Physics.CheckSphere(spawnPos, PLAYER_SEPARATION, playerMask)); // Keep certain distance between objects and player

            GameObject newEnemy = Instantiate(enemy, spawnPos, Quaternion.identity);
        }

    }
}
