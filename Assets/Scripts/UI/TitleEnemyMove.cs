using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TitleEnemyMove : MonoBehaviour
{
    private Grid3D grid;

    private Vector3 nextPathPos;
    private float nextPathfind;
    private int pathfindDelay;

    private Rigidbody rb;

    [SerializeField] private EnemyObject enemy;
    private float moveSpeedCurr;

    void Start()
    {
        grid = GameObject.FindGameObjectWithTag("GridAir").GetComponent<Grid3D>(); // Use air grid
        rb = GetComponent<Rigidbody>();

        moveSpeedCurr = enemy.MOVE_SPEED_BASE;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        // Find new position
        if (nextPathfind < Time.time)
        {
            nextPathPos = new Vector3(Random.Range(-grid.gridSizeX * grid.nodeRadius, grid.gridSizeX * grid.nodeRadius), Random.Range(0, grid.gridSizeY * grid.nodeRadius), Random.Range(-grid.gridSizeZ * grid.nodeRadius, grid.gridSizeZ * grid.nodeRadius));

            pathfindDelay = Random.Range(3, 5);
            nextPathfind = Time.time + pathfindDelay;
        }

        // Move
        Move();
    }

    private void Move()
    {
        transform.LookAt(nextPathPos);

        if (Vector3.Distance(transform.position, nextPathPos) > 1f)
        {
            Vector3 moveDir = nextPathPos - transform.position;
            moveDir.Normalize();

            rb.AddForce((moveDir * (moveSpeedCurr / 4)) - rb.velocity, ForceMode.VelocityChange);
        }
        else
        {
            nextPathPos = new Vector3(Random.Range(-grid.gridSizeX * grid.nodeRadius, grid.gridSizeX * grid.nodeRadius), Random.Range(0, grid.gridSizeY * grid.nodeRadius), Random.Range(-grid.gridSizeZ * grid.nodeRadius, grid.gridSizeZ * grid.nodeRadius));
        }
    }
}
