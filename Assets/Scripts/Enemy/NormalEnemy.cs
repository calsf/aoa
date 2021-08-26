using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NormalEnemy : Enemy
{
    protected NavMeshAgent navMeshAgent;

    void Start()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        navMeshAgent.isStopped = true;
    }

    void Update()
    {
        
    }

    // TODO: Pathfinding and movement (May need to clamp offset so enemy doesn't clip through ground/ceiling)
    override protected void Move()
    {
        navMeshAgent.speed = moveSpeedCurr;
        navMeshAgent.destination = player.transform.position;

        // Aggro only if path to take to player is of certain distance
        if (navMeshAgent.remainingDistance < 10 && !isAggro)
        {
            isAggro = true;
            navMeshAgent.isStopped = false;
        }

        // Adjust base offset to adjust Y position
        if (isAggro && navMeshAgent.remainingDistance < 10)
        {
            if (player.transform.position.y > transform.position.y)
            {
                navMeshAgent.baseOffset += .025f;
            }
            else if (player.transform.position.y < transform.position.y)
            {
                navMeshAgent.baseOffset -= .025f;
            }
        }
    }
}
