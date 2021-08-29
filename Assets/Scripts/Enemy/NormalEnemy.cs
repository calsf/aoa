using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NormalEnemy : Enemy
{
    private int currPathPos;
    private Vector3 nextPathPos;

    private Rigidbody rb;

    private float nextPathfind;
    private int pathfindDelay;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        if (nextPathfind < Time.time)
        {
            // Determine next pathfind call depending on enemy aggro state
            pathfindDelay = isAggro ? Random.Range(1, 3) : 1;
            nextPathfind = Time.time + pathfindDelay;

            PathFind();
        }
    }

    override protected void Move()
    {
        if (!isAggro && path != null && path.Count > 0)
        {
            isAggro = true;
        }
        else if (isAggro)
        {
            transform.LookAt(player.transform);

            if (path != null)
            {
                // Reset if path was changed
                if (pathChanged)
                {
                    pathChanged = false;
                    currPathPos = 0;
                }

                // Once path has been fulfilled, try to go to player
                if (currPathPos > path.Count - 1)
                {
                    nextPathPos = player.transform.position;
                }
                else
                {
                    nextPathPos = path[currPathPos].position;
                }

                // Move to next path position
                if (Vector3.Distance(transform.position, nextPathPos) > 1f)
                {
                    Vector3 moveDir = nextPathPos - transform.position;
                    moveDir.Normalize();

                    rb.velocity = moveDir * moveSpeedCurr;
                }
                else
                {
                    currPathPos++;
                }
            }
        }
    }
}
