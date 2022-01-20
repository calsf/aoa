using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyGroundUpright : EnemyGround
{
    protected const float MIN_ATK_DELAY = 2;
    protected const float MAX_ATK_DELAY = 4;

    protected float nextAtkTime;
    protected bool isAttacking;

    protected Vector3 attackStartPos;
    protected Vector3 attackTargetPos;
    protected float delayTime;

    protected LayerMask groundLayerMask;

    protected override void Start()
    {
        base.Start();
        groundLayerMask = new LayerMask();
        groundLayerMask.value = 1 << LayerMask.NameToLayer("Ground")
            | 1 << LayerMask.NameToLayer("Wall");
    }

    void Update()
    {
        if (!isAggro) // Not aggro, keep updating next attack time so it delays first attack on aggro
        {
            nextAtkTime = Time.time + Random.Range(MIN_ATK_DELAY, MAX_ATK_DELAY);
        }
        else if (Time.time > nextAtkTime && !isAttacking && canMove) // Is aggro and not already attacking and can move, attack
        {
            StartAttacking();
        }

        CheckColdShot();
        CheckWeakeningShot();
    }

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
        else if (!isAttacking && canMove) // Move normally if not attacking and can move
        {
            Move();
        }
        else if(isAttacking && canMove) // Check for attack finish if attacking and moving
        {
            CheckAttackFinish();
        }
        else // Stop if cannot move
        {
            Vector3 velocity = Vector3.zero - rb.velocity;
            velocity.Normalize();

            velocity.y = 0; // Set y velocity to 0

            rb.AddForce(velocity, ForceMode.VelocityChange);

            // Keep looking at player
            transform.LookAt(new Vector3(currTarget.position.x, transform.position.y, currTarget.position.z));
        }
    }

    protected void CheckAttackFinish()
    {
        bool hasHit = Physics.Raycast(transform.position, Vector3.down, .2f, groundLayerMask);

        if (hasHit && Time.time > delayTime)
        {
            FinishAttacking();
        }
    }

    protected void StartAttacking()
    {
        isAttacking = true;
        canMove = false; // Stop movement while starting attack

        anim.Play("Attack");
    }

    protected void Attack()
    {
        // Apply initial force to 'jump' to player
        attackTargetPos = player.transform.position;
        attackStartPos = transform.position;
        Vector3 moveDir = attackTargetPos - attackStartPos;
        moveDir.y = 0;
        moveDir.Normalize();
        rb.AddForce((moveDir * (moveSpeedCurr * 2)) + Vector3.up * 15, ForceMode.VelocityChange);

        // Reactivate movement
        canMove = true;

        // Freeze rotation during attack
        rb.freezeRotation = true;

        // Delay time before checking for grounded raycast
        delayTime = Time.time + .5f;
    }

    protected void FinishAttacking()
    {
        isAttacking = false;
        rb.freezeRotation = false;

        anim.Play("Move");

        // Set next time to attack
        nextAtkTime = Time.time + Random.Range(MIN_ATK_DELAY, MAX_ATK_DELAY);
    }
}
