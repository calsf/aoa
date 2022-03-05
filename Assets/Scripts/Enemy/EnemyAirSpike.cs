using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAirSpike : Enemy
{
    protected Grid3D grid;

    protected Vector3 nextPathPos;

    protected Rigidbody rb;

    protected override void Start()
    {
        base.Start();
        rb = GetComponent<Rigidbody>();
        grid = GameObject.FindGameObjectWithTag("GridAir").GetComponent<Grid3D>(); // Use air grid

        canMove = true;

        // Get random position to start moving to
        nextPathPos = new Vector3(Random.Range(-grid.gridSizeX * grid.nodeRadius, grid.gridSizeX * grid.nodeRadius), Random.Range(0, grid.gridSizeY * grid.nodeRadius), Random.Range(-grid.gridSizeZ * grid.nodeRadius, grid.gridSizeZ * grid.nodeRadius));
    }

    // Reset enemy values
    protected override void ResetEnemy()
    {
        base.ResetEnemy();

        // Reset rigidbody
        rb.angularVelocity = Vector3.zero;
        rb.velocity = Vector3.zero;

        nextPathPos = new Vector3(Random.Range(-grid.gridSizeX * grid.nodeRadius, grid.gridSizeX * grid.nodeRadius), Random.Range(0, grid.gridSizeY * grid.nodeRadius), Random.Range(-grid.gridSizeZ * grid.nodeRadius, grid.gridSizeZ * grid.nodeRadius));
    }

    void Update()
    {
        CheckColdShot();
        CheckWeakeningShot();
        MoveMinimapIcon();
    }

    void FixedUpdate()
    {
        // Move
        if (isTaunted)
        {
            Taunted();
        }
        else if (canMove)
        {
            Move();
        }
        else // Stop if cannot move
        {
            rb.AddForce((Vector3.zero - rb.velocity).normalized, ForceMode.VelocityChange);

            // Keep looking at player
            transform.LookAt(currTarget);
        }
    }

    // Override enemy movement to move directly to player when aggro, else move to random positions
    protected override void Move()
    {
        if (!isAggro && (healthCurr < healthMax)) // Aggro when hit or when path to player is close enough (proximity checked by player's aggro area collider)
        {
            isAggro = true;
        }
        else if (isAggro) // Aggro on player
        {
            nextPathPos = player.transform.position;
            transform.LookAt(nextPathPos);

            Vector3 moveDir = nextPathPos - transform.position;
            moveDir.Normalize();

            rb.AddForce((moveDir * moveSpeedCurr) - rb.velocity, ForceMode.VelocityChange);
        }
        else // Move around to random positions
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

    // Movement when taunted by decoy shot
    protected void Taunted()
    {
        transform.LookAt(currTarget);

        Vector3 moveDir = currTarget.position - transform.position;
        moveDir.Normalize();

        rb.AddForce((moveDir * moveSpeedCurr) - rb.velocity, ForceMode.VelocityChange);
    }

    protected override void PathFind()
    {
        return;
    }

    // Break wall blocks on collision
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Wall"))
        {
            collision.gameObject.GetComponent<WallBlock>().Damaged(500);
        }
    }
}
