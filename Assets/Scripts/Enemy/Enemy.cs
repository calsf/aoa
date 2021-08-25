using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
    private NavMeshAgent navMeshAgent;

    private bool isAggro = false;

    [SerializeField] private Transform player;

    private int damage = 5;

    void Start()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();

        navMeshAgent.isStopped = true;
    }

    void FixedUpdate()
    {
        navMeshAgent.destination = player.position;

        // Aggro only if path to take to player is of certain distance
        if (navMeshAgent.remainingDistance < 10 && !isAggro)
        {
            isAggro = true;
            navMeshAgent.isStopped = false;
        }

        // Adjust base offset to adjust Y position
        if (isAggro && navMeshAgent.remainingDistance < 5)
        {
            if (player.position.y > transform.position.y)
            {
                navMeshAgent.baseOffset += .025f;
            }
            else if (player.position.y < transform.position.y)
            {
                navMeshAgent.baseOffset -= .025f;
            }

            // TODO: May need to clamp offset so enemy doesn't clip through ground/ceiling
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Attempt to damage player
        if (other.gameObject.tag == "Player")
        {
            other.gameObject.GetComponent<PlayerHealth>().DamagePlayer(damage);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        OnTriggerEnter(other);
    }
}
