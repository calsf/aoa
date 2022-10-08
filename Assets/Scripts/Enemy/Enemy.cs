using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Enemy : MonoBehaviour
{
    protected const int POOL_NUM = 3;

    [SerializeField] protected PlayerStateObject playerState;

    [SerializeField] protected Transform minimapIconPrefab;
    protected Transform minimapIcon;

    [SerializeField] protected GameObject deathEffect;
    [SerializeField] protected GameObject explosiveShotEffect;
    protected List<GameObject> deathEffectPool;
    protected List<GameObject> explosiveShotEffectPool;
    protected const float EXPLOSIVE_DMG_MULTIPLIER = .2f;

    protected Animator anim;
    protected GameObject player;
    protected PlayerMoveController playerMoveController;

    public bool isAggro = false;
    protected Quaternion origRot;

    public bool canMove { get; set; }

    // Enemy props
    [SerializeField] protected EnemyObject enemy;
    protected float moveSpeedMax;
    protected float moveSpeedCurr;
    protected float healthMax;
    protected float healthCurr;
    protected float damageMax;
    public float damageCurr { get; set; }

    public bool isColdShotted { get; set; }
    protected float coldShotOffTime;

    public bool isWeakenShotted { get; set; }
    protected float weakenShotOffTime;

    public bool isTaunted { get; set; }
    public Transform currTarget { get; set; }

    public HealthBar healthBar;

    protected virtual void Awake()
    {

    }

    protected virtual void Start()
    {
        anim = GetComponent<Animator>();
        player = GameObject.FindGameObjectWithTag("Player");
        playerMoveController = player.GetComponent<PlayerMoveController>();

        healthBar.ownerEnemy = this;

        origRot = transform.rotation;

        // Initialize pool of death effects, these are in the (very rare) case an enemy is reused and killed again while previous death effect was still active
        deathEffectPool = new List<GameObject>();
        for (int i = 0; i < POOL_NUM; i++)
        {
            deathEffectPool.Add(Instantiate(deathEffect, Vector3.zero, deathEffect.transform.rotation));
            deathEffectPool[i].SetActive(false);
        }

        /* MOVED TO INITIALIZESTATS TO MAKE SURE IT USES HEALTH MAX
        // Initialize pool of explosive shot effects, these are in the (very rare) case an enemy is reused and killed again while previous explo shot effect was still active
        explosiveShotEffectPool = new List<GameObject>();
        for (int i = 0; i < POOL_NUM; i++)
        {
            explosiveShotEffectPool.Add(Instantiate(explosiveShotEffect, Vector3.zero, Quaternion.identity));
            
            Explosion explo = explosiveShotEffectPool[i].GetComponent<Explosion>();
            explo.SetSize(enemy.EXPLO_SIZE);    // Set explo size
            explo.damage = (healthMax * EXPLOSIVE_DMG_MULTIPLIER);    // Set explo dmg based on % enemy max health

            explosiveShotEffectPool[i].SetActive(false);
        }
        */

        currTarget = player.transform;

        minimapIcon = Instantiate(minimapIconPrefab);
    }

    // Getters
    public float GetBaseMoveSpeed()
    {
        return enemy.MOVE_SPEED_BASE;
    }

    public float GetBaseHealth()
    {
        return enemy.HEALTH_BASE;
    }

    public float GetBaseDamage()
    {
        return enemy.DAMAGE_BASE;
    }

    // Setters
    public void InitializeStats(float moveSpeed, float health, float damage)
    {
        moveSpeedMax = moveSpeed;
        moveSpeedCurr = moveSpeedMax;

        healthMax = health;
        healthCurr = healthMax;

        damageMax = damage;
        damageCurr = damageMax;

        // Initialize pool of explosive shot effects, these are in the (very rare) case an enemy is reused and killed again while previous explo shot effect was still active
        explosiveShotEffectPool = new List<GameObject>();
        for (int i = 0; i < POOL_NUM; i++)
        {
            explosiveShotEffectPool.Add(Instantiate(explosiveShotEffect, Vector3.zero, Quaternion.identity));

            Explosion explo = explosiveShotEffectPool[i].GetComponent<Explosion>();
            explo.SetSize(enemy.EXPLO_SIZE);    // Set explo size
            explo.damage = (healthMax * EXPLOSIVE_DMG_MULTIPLIER);    // Set explo dmg based on % enemy max health

            explosiveShotEffectPool[i].SetActive(false);
        }
    }

    protected abstract void Move();

    protected abstract void PathFind();

    public virtual void Damaged(float dmg)
    {
        if (healthCurr <= 0)
        {
            return;
        }
        else
        {
            healthCurr -= dmg;

            TriggerHealthBar();

            if (healthCurr <= 0)
            {
                // Death effect
                GameObject deathObj = GetFromPool(deathEffectPool, deathEffect);
                deathObj.transform.position = transform.position;
                deathObj.SetActive(true);

                // Explosive shot - create explosion on death
                if (playerState.powers["ExplosiveShot"].isActive)
                {
                    GameObject exploObj = GetFromPool(explosiveShotEffectPool, explosiveShotEffect);
                    exploObj.transform.position = transform.position;
                    exploObj.SetActive(true);
                }

                // Deactivate and reset enemy object
                ResetEnemy();
                playerState.AddBloodCurrency(enemy.CURRENCY_DROP); // Give player currency amount from killing
                gameObject.SetActive(false);
            }
        }
    }

    // Show health bar or reset timer if already active
    public void TriggerHealthBar()
    {
        healthBar.HealthBarOnHit(healthCurr / healthMax);
    }

    // Keep minimap icon at same position as enemy
    protected void MoveMinimapIcon()
    {
        if (!minimapIcon.gameObject.activeInHierarchy)
        {
            minimapIcon.gameObject.SetActive(true);
        }

        minimapIcon.transform.position = transform.position;
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

        // Deactivate minimap icon
        minimapIcon.gameObject.SetActive(false);

        // Deactivate health bar
        healthBar.gameObject.SetActive(false);

        // When resetting enemy, ignore nest collision since they will be respawned inside a nest
        SetIgnoreNestCollision(true);
    }

    private void OnTriggerEnter(Collider other)
    {
        // Attempt to damage player
        if (other.gameObject == player)
        {
            playerState.DamagePlayer(damageCurr);
        }

        // Immediately trigger aggro if not already aggro'd and look for a path once in player's aggro area
        if (!isAggro && other.gameObject.layer == LayerMask.NameToLayer("AggroArea"))
        {
            isAggro = true;
            PathFind();
        }
    }

    private void OnTriggerStay(Collider other)
    {
        OnTriggerEnter(other);
    }

    private void OnTriggerExit(Collider other)
    {
        // On exiting a nest, re-activate collision with nest
        if (other.gameObject.layer == LayerMask.NameToLayer("Nest"))
        {
            //Debug.Log("called");
            SetIgnoreNestCollision(false);
        }
    }

    // Set ignore layer collision with nest based on argument
    public void SetIgnoreNestCollision(bool ignore)
    {
        Physics.IgnoreLayerCollision(LayerMask.NameToLayer("EnemyCollision"), LayerMask.NameToLayer("Nest"), ignore);
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
