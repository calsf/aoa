using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawnManager : MonoBehaviour
{
    private const int OBJECT_SEPARATION = 3;
    private const int PLAYER_SEPARATION = 80;

    [SerializeField] private int startNum;
    [SerializeField] private GameObject enemy;
    [SerializeField] private bool isGrounded; // If true, spawn at original object y position
    private Grid3D grid;

    private LayerMask playerMask;
    private LayerMask objectMask;

    void Start()
    {
        grid = GameObject.FindGameObjectWithTag("GridAir").GetComponent<Grid3D>();
 
        playerMask = new LayerMask();
        playerMask = 1 << LayerMask.NameToLayer("Player");

        objectMask = new LayerMask();
        objectMask = (1 << LayerMask.NameToLayer("Enemy")
            | 1 << LayerMask.NameToLayer("Wall"));

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

            GameObject newEnemy = Instantiate(enemy, spawnPos, Quaternion.identity);
        }

    }
}
