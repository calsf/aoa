using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAirExploder : EnemyAir
{
    protected bool isExploding;

    protected override void Start()
    {
        base.Start();

        foreach (GameObject deathEffect in deathEffectPool)
        {
            deathEffect.GetComponent<ExploderDeath>().enemy = this;
        }
    }

    void Update()
    {
        // Prepare explosion once close to player
        if (!isExploding && Vector3.Distance(transform.position, player.transform.position) < 10f)
        {
            anim.Play("Explode");
        }
        
        CheckColdShot();
        CheckWeakeningShot();
        MoveMinimapIcon();
    }

    private void Explode()
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
        gameObject.SetActive(false);
    }
}
