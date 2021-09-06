using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Enemy : MonoBehaviour
{
    [SerializeField] protected GameObject deathEffect;

    protected GameObject player;
    protected PlayerMoveController playerMoveController;
    protected PlayerHealth playerHealth;

    protected bool isAggro = false;

    // Enemy props
    [SerializeField] protected EnemyObject enemy;
    protected float moveSpeedMax;
    protected float moveSpeedCurr;
    protected float healthMax;
    protected float healthCurr;
    protected float damageMax;
    protected float damageCurr;

    protected virtual void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        playerMoveController = player.GetComponent<PlayerMoveController>();
        playerHealth = player.GetComponent<PlayerHealth>();

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
            playerHealth.DamagePlayer(damageCurr);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        OnTriggerEnter(other);
    }
}
