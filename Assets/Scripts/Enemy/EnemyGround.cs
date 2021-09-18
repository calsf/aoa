using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyGround : Enemy
{
    [SerializeField] protected float maxDistLimit;

    protected NavMeshAgent navMeshAgent;

    protected bool canMove;

    protected override void Start()
    {
        base.Start();
        navMeshAgent = GetComponent<NavMeshAgent>();

        navMeshAgent.isStopped = true;
        canMove = true;
    }

    void LateUpdate()
    {
        Move();
    }

    protected override void Move()
    {
        navMeshAgent.destination = new Vector3(player.transform.position.x, transform.position.y, player.transform.position.z);
        navMeshAgent.speed = moveSpeedCurr;
        Debug.Log(navMeshAgent.remainingDistance);

        // Aggro when hit or when close enough
        if (!isAggro && ((navMeshAgent.remainingDistance < maxDistLimit && navMeshAgent.hasPath) || healthCurr < healthMax))
        {
            isAggro = true;
        }

        // Move
        if (isAggro && canMove)
        {
            navMeshAgent.isStopped = false;
        }
        else
        {
            navMeshAgent.isStopped = true;
        }
    }
}
