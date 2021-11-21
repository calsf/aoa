using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Enemy : MonoBehaviour
{
    [SerializeField] private PlayerStateObject playerState;

    [SerializeField] protected GameObject deathEffect;

    protected Animator anim;
    protected GameObject player;
    protected PlayerMoveController playerMoveController;

    protected bool isAggro = false;

    // Enemy props
    [SerializeField] protected EnemyObject enemy;
    protected float moveSpeedMax;
    protected float moveSpeedCurr;
    protected float healthMax;
    protected float healthCurr;
    protected float damageMax;
    protected float damageCurr;

    protected bool isColdShotted;
    protected float coldShotOffTime;

    protected bool isWeakenShotted;
    protected float weakenShotOffTime;

    protected virtual void Start()
    {
        anim = GetComponent<Animator>();
        player = GameObject.FindGameObjectWithTag("Player");
        playerMoveController = player.GetComponent<PlayerMoveController>();

        moveSpeedMax = enemy.MOVE_SPEED_BASE;
        moveSpeedCurr = moveSpeedMax;
        healthMax = enemy.HEALTH_BASE;
        healthCurr = healthMax;
        damageMax = enemy.DAMAGE_BASE;
        damageCurr = damageMax;
    }

    protected abstract void Move();

    public virtual void Damaged(float dmg)
    {
        if (healthCurr <= 0)
        {
            return;
        }
        else
        {
            healthCurr -= dmg;

            if (healthCurr <= 0)
            {
                GameObject obj = Instantiate(deathEffect, transform.position, Quaternion.identity);
                Destroy(gameObject);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Attempt to damage player
        if (other.gameObject == player)
        {
            playerState.DamagePlayer(damageCurr);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        OnTriggerEnter(other);
    }

    public void ApplyColdShot(float slowMultiplier, float delay)
    {
        moveSpeedCurr *= slowMultiplier;
        coldShotOffTime = Time.time + delay;
        isColdShotted = true;
    }

    protected void CheckColdShot()
    {
        if (isColdShotted && Time.time > coldShotOffTime)
        {
            isColdShotted = false;
            moveSpeedCurr = moveSpeedMax;
        }
    }

    public void ApplyWeakeningShot(float weakenMultiplier, float delay)
    {
        damageCurr *= weakenMultiplier;
        weakenShotOffTime = Time.time + delay;
        isWeakenShotted = true;
    }

    protected void CheckWeakeningShot()
    {
        if (isWeakenShotted && Time.time > weakenShotOffTime)
        {
            isWeakenShotted = false;
            damageCurr = damageMax;
        }
    }
}
