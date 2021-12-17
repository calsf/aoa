using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Enemy : MonoBehaviour
{
    protected const int POOL_NUM = 3;

    [SerializeField] private PlayerStateObject playerState;

    [SerializeField] protected GameObject deathEffect;
    [SerializeField] protected GameObject explosiveShotEffect;
    protected List<GameObject> deathEffectPool;
    protected List<GameObject> explosiveShotEffectPool;
    protected const float EXPLOSIVE_DMG_MULTIPLIER = .2f;

    protected Animator anim;
    protected GameObject player;
    protected PlayerMoveController playerMoveController;

    protected bool isAggro = false;
    protected Quaternion origRot;

    public bool canMove { get; set; }

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

    public bool isTaunted { get; set; }
    public Transform currTarget { get; set; }

    protected virtual void Start()
    {
        anim = GetComponent<Animator>();
        player = GameObject.FindGameObjectWithTag("Player");
        playerMoveController = player.GetComponent<PlayerMoveController>();

        origRot = transform.rotation;

        moveSpeedMax = enemy.MOVE_SPEED_BASE;
        moveSpeedCurr = moveSpeedMax;
        healthMax = enemy.HEALTH_BASE;
        healthCurr = healthMax;
        damageMax = enemy.DAMAGE_BASE;
        damageCurr = damageMax;

        // Initialize pool of death effects, these are in the (very rare) case an enemy is reused and killed again while previous death effect was still active
        deathEffectPool = new List<GameObject>();
        for (int i = 0; i < POOL_NUM; i++)
        {
            deathEffectPool.Add(Instantiate(deathEffect, Vector3.zero, Quaternion.identity));
            deathEffectPool[i].SetActive(false);
        }

        // Initialize pool of explosive shot effects, these are in the (very rare) case an enemy is reused and killed again while previous explo shot effect was still active
        explosiveShotEffectPool = new List<GameObject>();
        for (int i = 0; i < POOL_NUM; i++)
        {
            explosiveShotEffectPool.Add(Instantiate(explosiveShotEffect, Vector3.zero, Quaternion.identity));
            
            Explosion explo = explosiveShotEffectPool[i].GetComponent<Explosion>();
            explo.SetSize(enemy.EXPLO_SIZE);    // Set explo size
            explo.damage = (healthMax * EXPLOSIVE_DMG_MULTIPLIER) + playerState.damageBonus;    // Set explo dmg based on % enemy max health and player bonus dmg

            explosiveShotEffectPool[i].SetActive(false);
        }

        currTarget = player.transform;
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
                // Death effect
                GameObject deathObj = GetFromPool(deathEffectPool, deathEffect);
                deathObj.transform.position = transform.position;
                deathObj.SetActive(true);

                // Explosive shot - create explosion on death
                if (playerState.explosiveShot)
                {
                    GameObject exploObj = GetFromPool(explosiveShotEffectPool, explosiveShotEffect);
                    exploObj.transform.position = transform.position;
                    exploObj.SetActive(true);
                }

                // Deactivate and reset enemy object
                ResetEnemy();
                gameObject.SetActive(false);
            }
        }
    }

    // Reset enemy values
    protected virtual void ResetEnemy()
    {
        transform.rotation = origRot;
        
        moveSpeedCurr = moveSpeedMax;
        healthCurr = healthMax;
        damageCurr = damageMax;

        isAggro = false;
        canMove = true;
        isTaunted = false;
        isColdShotted = false;
        isWeakenShotted = false;
        currTarget = player.transform;
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

    public void RemoveTaunt()
    {
        isTaunted = false;
        currTarget = player.transform;
    }

    protected GameObject GetFromPool(List<GameObject> pool, GameObject obj)
    {
        for (int i = 0; i < pool.Count; i++)
        {
            if (!pool[i].activeInHierarchy)
            {
                return pool[i];
            }
        }

        // If no object in the pool is available, create a new object and add to the pool
        GameObject newObj = Instantiate(obj, Vector3.zero, Quaternion.identity);
        pool.Add(newObj);
        return newObj;
    }
}
