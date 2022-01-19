using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyGroundUpright : EnemyGround
{
    protected const float MIN_ATK_DELAY = 2;
    protected const float MAX_ATK_DELAY = 4;

    [SerializeField] private GameObject head;
    [SerializeField] private GameObject mouthEnd;

    protected float nextAtkTime;
    protected bool isAttacking;

    protected LayerMask blockedLayerMask;
    protected LayerMask playerLayerMask;

    protected override void Start()
    {
        base.Start();

        blockedLayerMask = new LayerMask();
        blockedLayerMask = (
            1 << LayerMask.NameToLayer("Wall")
            | 1 << LayerMask.NameToLayer("Altar")
            | 1 << LayerMask.NameToLayer("Nest"));

        playerLayerMask = new LayerMask();
        playerLayerMask = 1 << LayerMask.NameToLayer("Player");
    }

    void Update()
    {
        if (!isAggro) // Not aggro, keep updating next attack time so it delays first attack on aggro
        {
            nextAtkTime = Time.time + Random.Range(MIN_ATK_DELAY, MAX_ATK_DELAY);
        }
        else if (Time.time > nextAtkTime && !isAttacking && canMove) // Is aggro and not already attacking and can move, attack
        {
            RaycastHit hit;
            bool hasHit = Physics.Linecast(head.transform.position, mouthEnd.transform.position, out hit, blockedLayerMask);

            if (hasHit && hit.collider != null) // If hit, means something is blocking between start and end of mouth attack, do not attack
            {
                return;
            }
            else
            {
                RaycastHit hitPlayer;
                bool hasHitPlayer = Physics.Linecast(head.transform.position, mouthEnd.transform.position, out hitPlayer, playerLayerMask);

                // Attack only if will hit player
                // This means enemy will only attack if not blocked by other objects AND if in range of hitting the player
                if (hasHitPlayer && hitPlayer.collider != null)
                {
                    StartAttacking();
                }
            }
        }

        CheckColdShot();
        CheckWeakeningShot();
    }

    // SAME AS GROUND ENEMY, BUT DO NOT LOOK AT PLAYER WHEN ATTACKING/STOPPED MOVING
    void FixedUpdate()
    {
        // Check for and update last valid start node position for enemy
        (int, int, int) startNodeIndex = grid.GetNodeIndexFromPosition(transform.position);
        if (startNodeIndex.Item1 <= grid.gridSizeX - 1 && startNodeIndex.Item2 <= grid.gridSizeY - 1 && startNodeIndex.Item3 <= grid.gridSizeZ - 1)
        {
            lastValidStartPos = transform.position;
        }

        // Check for next pathfind call
        if (isAggro && nextPathfind < Time.time)
        {
            // Determine next pathfind call
            pathfindDelay = Random.Range(1, 4);
            nextPathfind = Time.time + pathfindDelay;

            PathFind();
        }

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
            Vector3 velocity = Vector3.zero - rb.velocity;
            velocity.Normalize();

            velocity.y = 0; // Set y velocity to 0

            rb.AddForce(velocity, ForceMode.VelocityChange);

            // Freeze rotation if is attacking, else unfreeze
            if (!isAttacking)
            {
                rb.freezeRotation = false;
            }
            else
            {
                rb.freezeRotation = true;
            }
        }
    }

    protected void StartAttacking()
    {
        isAttacking = true;
        canMove = false; // Stop movement while attacking

        anim.Play("Attack");
    }

    protected void FinishAttacking()
    {
        isAttacking = false;
        canMove = true; // Resume movement after shooting

        // Set next time to attack
        nextAtkTime = Time.time + Random.Range(MIN_ATK_DELAY, MAX_ATK_DELAY);
    }

    public void OnDrawGizmos()
    {
        Debug.DrawLine(head.transform.position, mouthEnd.transform.position, Color.red);
    }
}
