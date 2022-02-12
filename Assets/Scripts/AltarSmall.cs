using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AltarSmall : Altar
{
    protected override void Start()
    {
        base.Start();
        costCurr = COST_BASE_SMALL;
        costText.text = costCurr.ToString();
    }

    protected override void OpenAltar()
    {
        // Do not open if not enough currency
        if (playerState.bloodCurrency < costCurr)
        {
            return;
        }

        playerState.bloodCurrency -= costCurr; // Take away currency

        // Upgrades 1 random stat
        UpgradeStat();

        // Activate used effect and destroy self after use
        altarUsedEffectObj.SetActive(true);
        Destroy(gameObject);
    }
}
