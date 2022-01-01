using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AltarSpawnManager : MonoBehaviour
{
    private const int OBJECT_SEPARATION = 10;
    private const int PLAYER_SEPARATION = 25;

    [SerializeField] private int startNum;
    [SerializeField] private GameObject altarSmall;
    [SerializeField] private GameObject altarLarge;
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
            | 1 << LayerMask.NameToLayer("Wall")
            | 1 << LayerMask.NameToLayer("Nest")
            | 1 << LayerMask.NameToLayer("Altar"));

        // Set number of large altars to spawn
        int largeNum = Random.Range(2, 4);

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

            } while (Physics.CheckSphere(spawnPos, OBJECT_SEPARATION, objectMask) || Physics.CheckSphere(spawnPos, PLAYER_SEPARATION, playerMask)); // Keep certain distance between objects and player

            GameObject newAltar;
            if (largeNum > 0) // Spawn large altars
            {
                newAltar = Instantiate(altarLarge);
                largeNum--;
            }
            else // Spawn small altars for rest
            {
                newAltar = Instantiate(altarSmall);
            }

            newAltar.transform.position = spawnPos;
            newAltar.SetActive(true);
        }
    }
}
