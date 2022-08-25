using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallManager : MonoBehaviour
{
    [SerializeField] private int minWallsToRemove;
    [SerializeField] private int maxWallsToRemove;

    void Start()
    {
        int numOfWalls = transform.childCount; // Total number of wall blocks
        int numToRemove = Random.Range(minWallsToRemove, maxWallsToRemove); // Get random number of walls to remove

        for (int i = 0; i < numToRemove; i++)
        {
            int wallToRemove = Random.Range(0, numOfWalls);
            WallBlock wall = transform.GetChild(wallToRemove).GetComponent<WallBlock>();
            wall.gameObject.SetActive(false); // Set inactive before updating so grid won't see the block
            wall.UpdateGrid(); // Make sure to update grid, Grid3D creates Grid on Awake, enemies copy its grid on Start, so need to update here in Start
            Destroy(wall.gameObject);
        }
    }
}
