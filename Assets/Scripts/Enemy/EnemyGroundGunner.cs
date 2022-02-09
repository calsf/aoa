using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyGroundGunner : EnemyGround
{
    protected const int PROJECTILE_POOL_NUM = 20;
    protected const float MIN_SHOOT_DELAY = 1;
    protected const float MAX_SHOOT_DELAY = 5;

    [SerializeField] protected GameObject projectile;
    protected List<GameObject> projectilePool;

    [SerializeField] protected Transform projectileSpawnPos;
    protected float nextShotTime;
    protected bool isShooting;

    protected override void Start()
    {
        base.Start();
        projectilePool = new List<GameObject>();
        for (int i = 0; i < PROJECTILE_POOL_NUM; i++)
        {
            projectilePool.Add(Instantiate(projectile, Vector3.zero, Quaternion.identity));
            projectilePool[i].SetActive(false);
        }
    }

    void Update()
    {
        if (!isAggro) // Not aggro, keep updating next shot time so it delays first shot on aggro
        {
            nextShotTime = Time.time + Random.Range(MIN_SHOOT_DELAY, MAX_SHOOT_DELAY);
        }
        else if (Time.time > nextShotTime && !isShooting && canMove) // Is aggro and not already shooting and can move, shoot
        {
            StartShooting();
        }

        CheckColdShot();
        CheckWeakeningShot();
    }

    // Shoots while moving
    protected void StartShooting()
    {
        isShooting = true;

        anim.Play("Shoot"); // Shoot animation should have anim event to actually shoot
    }

    protected void FinishShooting()
    {
        isShooting = false;
        canMove = true; // Resume movement after shooting

        // Set next time to shoot
        nextShotTime = Time.time + Random.Range(MIN_SHOOT_DELAY, MAX_SHOOT_DELAY);
    }

    protected void ShootAtPlayer()
    {
        GameObject obj = GetFromPool(projectilePool, projectile);

        obj.transform.rotation = projectileSpawnPos.transform.rotation;
        obj.transform.position = projectileSpawnPos.transform.position;
        obj.SetActive(true);

        // Set projectile damage and direction
        Projectile newProjectile = obj.GetComponent<Projectile>();
        newProjectile.projectileDamage = damageCurr;
        newProjectile.projectileDir = (currTarget.position + (Vector3.up * 2) - transform.position).normalized; // Offset player position
        newProjectile.projectileDir = new Vector3(newProjectile.projectileDir.x, 0, newProjectile.projectileDir.z); // Only shoots forward along x/z, never rotates up and down to face player
    }
}