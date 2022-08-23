using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AltarSmall : Altar
{
    protected override void Start()
    {
        base.Start();
    }

    public override int GetBaseCost()
    {
        return COST_BASE_SMALL;
    }

    protected override bool OpenAltar()
    {
        // Do not open if not enough currency
        if (playerState.bloodCurrency < costCurr)
        {
            return false;
        }

        playerState.bloodCurrency -= costCurr; // Take away currency

        // Upgrades 1 random stat
        UpgradeStat();

        // Activate used effect and destroy self after use
        altarUsedEffectObj.SetActive(true);
        Destroy(gameObject);


        // Play sound
        audioSrc.PlayOneShot(audioClip);

        return true;
    }
}
