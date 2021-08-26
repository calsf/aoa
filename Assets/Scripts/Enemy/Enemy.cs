using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Enemy : MonoBehaviour
{
    [SerializeField] GameObject deathEffect;
 
    protected GameObject player;
    protected PlayerHealth playerHealth;
    protected GameObject enemyGameObject;

    protected bool isAggro = false;

    // Enemy props
    [SerializeField] protected EnemyObject enemy;
    protected float moveSpeedMax;
    protected float moveSpeedCurr;
    protected float healthMax;
    protected float healthCurr;
    protected float damage;

    void Awake()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        playerHealth = player.GetComponent<PlayerHealth>();
        enemyGameObject = transform.GetChild(0).gameObject;

        moveSpeedMax = enemy.MOVE_SPEED_BASE;
        moveSpeedCurr = moveSpeedMax;
        healthMax = enemy.HEALTH_BASE;
        healthCurr = healthMax;
        damage = enemy.DAMAGE_BASE;
    }

    void FixedUpdate()
    {
        Move();
    }

    public void Damaged(float dmg)
    {
        healthCurr -= dmg;

        if (healthCurr <= 0)
        {
            GameObject obj = Instantiate(deathEffect);
            obj.transform.position = enemyGameObject.transform.position;
            Destroy(gameObject);
        }
    }

    protected abstract void Move();

    private void OnTriggerEnter(Collider other)
    {
        // Attempt to damage player
        if (other.gameObject == player)
        {
            playerHealth.DamagePlayer(damage);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        OnTriggerEnter(other);
    }
}
