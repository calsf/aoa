using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NormalEnemy : Enemy
{
    private int currPathPos;
    private Vector3 nextPathPos;

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        InvokeRepeating("PathFind", 3, 1);
    }

    override protected void Move()
    {
        if (!isAggro && path != null && path.Count > 0)
        {
            isAggro = true;
        }
        else if (isAggro)
        {
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
