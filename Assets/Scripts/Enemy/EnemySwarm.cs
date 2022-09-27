using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySwarm : EnemyAir
{
    protected const int PROJECTILE_POOL_NUM = 100;
    protected const float SHOOT_DELAY = .3f;

    private const float BASE_SPAWN_DELAY_MIN = 5;
    private const float BASE_SPAWN_DELAY_MAX = 10;
    private const float MIN_SPAWN_DELAY = 2;

    [SerializeField] private Transform deathPos;

    // Projectiles
    [SerializeField] protected GameObject projectile;
    protected List<GameObject> projectilePool;
    [SerializeField] protected GameObject projectileSpawnPosParent;
    protected List<Transform> projectileSpawnPos;
    protected float nextShotTime;

    // Enemies
    [SerializeField] protected GameObject enemySpawnPosParent;
    protected List<Transform> enemySpawnPos;
    private List<EnemySpawnManager> enemySpawners;
    private float nextSpawnTime;
    private int maxActiveEnemies;

    protected override void Start()
    {
        base.Start();
        InitializeStats(
            enemy.MOVE_SPEED_BASE,
            enemy.HEALTH_BASE,
            enemy.DAMAGE_BASE);

        isAggro = true;

        // Projectiles
        projectilePool = new List<GameObject>();
        for (int i = 0; i < PROJECTILE_POOL_NUM; i++)
        {
            projectilePool.Add(Instantiate(projectile, Vector3.zero, Quaternion.identity));
            projectilePool[i].SetActive(false);
        }

        projectileSpawnPos = new List<Transform>();
        foreach (Transform child in projectileSpawnPosParent.transform)
        {
            projectileSpawnPos.Add(child);
        }

        // Enemies
        enemySpawners = new List<EnemySpawnManager>();

        GameObject[] spawnerObjs = GameObject.FindGameObjectsWithTag("EnemySpawnManager");

        foreach (GameObject obj in spawnerObjs)
        {
            EnemySpawnManager enemySpawner = obj.GetComponent<EnemySpawnManager>();
            enemySpawners.Add(enemySpawner);

            maxActiveEnemies += enemySpawner.maxNum;
        }

        enemySpawnPos = new List<Transform>();
        foreach (Transform child in enemySpawnPosParent.transform)
        {
            enemySpawnPos.Add(child);
        }
    }

    void Update()
    {
        CheckColdShot();
        CheckWeakeningShot();
        MoveMinimapIcon();

        if (Time.time > nextShotTime) // Is aggro and not already shooting and can move, shoot
        {
            nextShotTime = Time.time + SHOOT_DELAY;
            Shoot();
        }

        if (nextSpawnTime < Time.time)
        {
            // Calculate next spawn time based on total number of ALL enemies alive
            // The less enemies alive, the faster the next spawn, the more enemies alive, the slower the next spawn
            int currActiveEnemies = 0;
            foreach (EnemySpawnManager spawner in enemySpawners)
            {
                currActiveEnemies += spawner.activeEnemies.Count;
            }

            float activeEnemyPercent = (float)currActiveEnemies / maxActiveEnemies;

            nextSpawnTime = Time.time + (Random.Range(BASE_SPAWN_DELAY_MIN, BASE_SPAWN_DELAY_MAX) * activeEnemyPercent) + MIN_SPAWN_DELAY; // Add a min delay in case activeEnemyPercent is 0

            SpawnEnemies();
        }
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

    // Same as Enemy.cs but set deathObj position higher
    public override void Damaged(float dmg)
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
                deathObj.transform.position = deathPos.position;
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

    protected override void Move()
    {
        transform.LookAt(currTarget);

        nextPathPos = player.transform.position;
        Vector3 moveDir = nextPathPos - transform.position;
        moveDir.Normalize();

        rb.AddForce((moveDir * moveSpeedCurr) - rb.velocity, ForceMode.VelocityChange);

    }

    protected override void PathFind()
    {
        return;
    }

    protected void Shoot()
    {
        foreach (Transform spawnPos in projectileSpawnPos)
        {
            GameObject obj = GetFromPool(projectilePool, projectile);

            obj.transform.rotation = spawnPos.transform.rotation;
            obj.transform.position = spawnPos.transform.position;
            obj.SetActive(true);

            // Set projectile damage and direction
            Projectile newProjectile = obj.GetComponent<Projectile>();
            newProjectile.projectileDamage = damageCurr;
            newProjectile.projectileDir = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f)); // Randomize projectile direction
        }
    }

    public void SpawnEnemies()
    {
        foreach (Transform spawnPos in enemySpawnPos)
        {
            // Get random enemy type
            EnemySpawnManager enemyType = enemySpawners[Random.Range(0, enemySpawners.Count)];

            bool spawned = enemyType.Spawn(spawnPos.position, true);
        }
    }
}
