using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Nest : Enemy
{
    private Vector3 DEATH_OFFSET = Vector3.up * 2;

    public NestManager nestManager { get; set; }

    protected override void Move()
    {
        return;
    }

    protected override void PathFind()
    {
        return;
    }

    // Override default Enemy Damaged
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
                deathObj.transform.position = transform.position + DEATH_OFFSET; // Offset death spawn position
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
                playerState.bloodCurrency += enemy.CURRENCY_DROP; // Give player currency amount from killing
                gameObject.SetActive(false);
            }
            else // Only force spawn if nest isn't dead from damaged
            {
                // The more damage dealt relative to curr health, the more likely to force an enemy spawn ( + base spawn chance)
                float spawnChance = (dmg / healthCurr) + .05f;

                if (Random.Range(0.0f, 1.0f) <= spawnChance)
                {
                    nestManager.SpawnFromNestHit(transform);
                }
            }
        }
    }
}
