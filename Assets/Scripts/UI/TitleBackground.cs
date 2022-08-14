using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TitleBackground : MonoBehaviour
{
    private const int OBJECT_SEPARATION = 3;

    private Grid3D grid;

    private int startNum = 20;

    [SerializeField] private GameObject enemy;

    void Start()
    {
        grid = GameObject.FindGameObjectWithTag("GridAir").GetComponent<Grid3D>(); // Use air grid

        LayerMask objectMask = new LayerMask();
        objectMask = (1 << LayerMask.NameToLayer("Enemy")
            | 1 << LayerMask.NameToLayer("Boundary")
            | 1 << LayerMask.NameToLayer("Wall")
            | 1 << LayerMask.NameToLayer("Nest")
            | 1 << LayerMask.NameToLayer("Altar"));

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

            } while (Physics.CheckSphere(spawnPos, OBJECT_SEPARATION, objectMask));

            GameObject newEnemy = Instantiate(enemy, Vector3.zero, Quaternion.identity);
            newEnemy.transform.position = spawnPos;
            newEnemy.SetActive(true);
        }
    }

    void Update()
    {
        
    }
}
