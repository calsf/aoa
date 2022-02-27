using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyCannon : EnemyGround
{
    protected const int PROJECTILE_POOL_NUM = 20;
    protected const float MIN_SHOOT_DELAY = 2;
    protected const float MAX_SHOOT_DELAY = 5;
    protected const float MIN_HEIGHT = 2;
    protected const float MAX_HEIGHT = 4;

    [SerializeField] protected GameObject projectile;
    protected List<GameObject> projectilePool;

    [SerializeField] protected Transform[] projectileSpawnPos;

    protected float nextShotTime;

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
        else if (Time.time > nextShotTime && canMove) // Periodically shoot up
        {
            ShootUp();
        }

        CheckColdShot();
        CheckWeakeningShot();
        MoveMinimapIcon();
    }

    protected void ShootUp()
    {
        GameObject obj = GetFromPool(projectilePool, projectile);

        Transform spawnPos = projectileSpawnPos[Random.Range(0, projectileSpawnPos.Length)];

        obj.transform.rotation = spawnPos.transform.rotation;
        obj.transform.position = spawnPos.transform.position;

        // Set projectile damage and direction
        Projectile newProjectile = obj.GetComponent<Projectile>();
        newProjectile.projectileDamage = damageCurr;

        // Set direction upwards a certain magnitude and random distance along the z and x axis
        newProjectile.projectileDir = ((Vector3.up * Random.Range(MIN_HEIGHT, MAX_HEIGHT)) + Vector3.right * (Random.Range(-1f, 1f)) + Vector3.forward * (Random.Range(-1f, 1f)));

        // Set next shot time
        nextShotTime = Time.time + Random.Range(MIN_SHOOT_DELAY, MAX_SHOOT_DELAY);

        obj.SetActive(true);
    }
}
