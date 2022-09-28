using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAirGunner : EnemyAir
{
    protected const int PROJECTILE_POOL_NUM = 60;
    protected const float MIN_SHOOT_DELAY = 2;
    protected const float MAX_SHOOT_DELAY = 5;

    [SerializeField] protected GameObject projectile;
    protected List<GameObject> projectilePool;

    [SerializeField] protected Transform projectileSpawnPos;
    protected float nextShotTime;
    protected bool isShooting;

    [SerializeField] private AudioSource windUpAudioSrc;
    [SerializeField] private AudioSource shootAudioSrc;
    private bool hasShot = false;

    protected override void Awake()
    {
        base.Awake();

        // Set up audio
        SoundManager soundManager = GameObject.FindGameObjectWithTag("SoundManager").GetComponent<SoundManager>();
        soundManager.AddAudioSource(shootAudioSrc);
        soundManager.AddAudioSource(windUpAudioSrc);

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
        MoveMinimapIcon();
    }

    // Shoots while moving
    protected void StartShooting()
    {
        isShooting = true;

        anim.Play("Shoot"); // Shoot animation should have anim event to actually shoot

        windUpAudioSrc.Play();
        hasShot = false;
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
        if (!hasShot)
        {
            hasShot = true;
            shootAudioSrc.Play();
        }

        GameObject obj = GetFromPool(projectilePool, projectile);

        obj.transform.rotation = projectileSpawnPos.transform.rotation;
        obj.transform.position = projectileSpawnPos.transform.position;
        obj.SetActive(true);

        // Set projectile damage and direction
        Projectile newProjectile = obj.GetComponent<Projectile>();
        newProjectile.projectileDamage = damageCurr;
        newProjectile.projectileDir = (currTarget.position + (Vector3.up * 2) - transform.position).normalized; // Offset player position
    }
}