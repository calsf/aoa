using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySwarm : Enemy
{
    [SerializeField] private Transform deathPos;

    protected override void Start()
    {
        base.Start();
        InitializeStats(
            enemy.MOVE_SPEED_BASE,
            enemy.HEALTH_BASE,
            enemy.DAMAGE_BASE);
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
        return;
    }

    protected override void PathFind()
    {
        return;
    }
}
