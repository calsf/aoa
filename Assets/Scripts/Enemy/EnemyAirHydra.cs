using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAirHydra : Enemy
{
    protected const int PROJECTILE_POOL_NUM = 80;
    protected const float SHOOT_DELAY = .3f;

    protected int minY;

    protected Grid3D grid;

    protected Vector3 nextPathPos;
    protected float nextPosTime;

    protected Rigidbody rb;

    [SerializeField] protected GameObject projectile;
    protected List<GameObject> projectilePool;

    [SerializeField] protected Transform projectileSpawnPos;
    protected float nextShotTime;

    protected override void Awake()
    {
        base.Awake();
        grid = GameObject.FindGameObjectWithTag("GridAir").GetComponent<Grid3D>(); // Use air grid

        projectilePool = new List<GameObject>();
        for (int i = 0; i < PROJECTILE_POOL_NUM; i++)
        {
            projectilePool.Add(Instantiate(projectile, Vector3.zero, Quaternion.identity));
            projectilePool[i].SetActive(false);
        }
    }

    protected override void Start()
    {
        base.Start();
        rb = GetComponent<Rigidbody>();

        canMove = true;

        minY = 10; // Min y position enemy can be at when finding position to move to

        // Get random position to start moving to
        nextPathPos = new Vector3(Random.Range(-grid.gridSizeX * grid.nodeRadius, grid.gridSizeX * grid.nodeRadius), Random.Range(minY, minY * 5), Random.Range(-grid.gridSizeZ * grid.nodeRadius, grid.gridSizeZ * grid.nodeRadius));
    }

    // Reset enemy values
    protected override void ResetEnemy()
    {
        base.ResetEnemy();

        // Reset rigidbody
        rb.angularVelocity = Vector3.zero;
        rb.velocity = Vector3.zero;

        nextPathPos = new Vector3(Random.Range(-grid.gridSizeX * grid.nodeRadius, grid.gridSizeX * grid.nodeRadius), Random.Range(minY, minY * 5), Random.Range(-grid.gridSizeZ * grid.nodeRadius, grid.gridSizeZ * grid.nodeRadius));
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

    // Override enemy movement to always move to random position
    protected override void Move()
    {
        // Check if should get new random position
        if (nextPosTime < Time.time)
        {
            // Determine next time to get new random position
            float delay = Random.Range(2, 5);
            nextPosTime = Time.time + delay;

            nextPathPos = new Vector3(Random.Range(-grid.gridSizeX * grid.nodeRadius, grid.gridSizeX * grid.nodeRadius), Random.Range(minY, minY * 5), Random.Range(-grid.gridSizeZ * grid.nodeRadius, grid.gridSizeZ * grid.nodeRadius));
        }

        // Move around to random positions
        transform.LookAt(new Vector3(nextPathPos.x, transform.position.y, nextPathPos.z));

        if (Vector3.Distance(transform.position, nextPathPos) > 1f)
        {
            Vector3 moveDir = nextPathPos - transform.position;
            moveDir.Normalize();

            rb.AddForce((moveDir * (moveSpeedCurr / 4)) - rb.velocity, ForceMode.VelocityChange);
        }
        else
        {
            nextPathPos = new Vector3(Random.Range(-grid.gridSizeX * grid.nodeRadius, grid.gridSizeX * grid.nodeRadius), Random.Range(minY, minY * 5), Random.Range(-grid.gridSizeZ * grid.nodeRadius, grid.gridSizeZ * grid.nodeRadius));
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

    protected void Shoot()
    {
        GameObject obj = GetFromPool(projectilePool, projectile);

        obj.transform.rotation = projectileSpawnPos.transform.rotation;
        obj.transform.position = projectileSpawnPos.transform.position;
        obj.SetActive(true);

        // Set projectile damage and direction
        Projectile newProjectile = obj.GetComponent<Projectile>();
        newProjectile.projectileDamage = damageCurr;
        newProjectile.projectileDir = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f)); // Randomize projectile direction
    }
}
