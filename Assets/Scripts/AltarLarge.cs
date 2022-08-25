using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AltarLarge : Altar
{
    private List<string> powers;
    protected override void Start()
    {
        base.Start();
        
        powers = new List<string>(playerState.powers.Keys);
    }

    public override int GetBaseCost()
    {
        return COST_BASE_LARGE;
    }

    protected override bool OpenAltar()
    {
        // Do not open if not enough currency
        if (playerState.bloodCurrency < costCurr)
        {
            return false;
        }

        playerState.bloodCurrency -= costCurr; // Take away currency

        // Select random power
        string selectedPowerKey = powers[Random.Range(0, powers.Count)];
        PlayerStateObject.Power newPower = playerState.powers[selectedPowerKey];

        if (newPower.isActive) // Power already active, give 2 stat upgrades instead
        {
            UpgradeStat();
            UpgradeStat();
        }
        else // Activate new power
        {
            newPower.isActive = true;
            playerState.UpdatePower(selectedPowerKey, newPower);
        }

        // Activate used effect and destroy self after use
        altarUsedEffectObj.SetActive(true);
        Destroy(minimapIcon.gameObject);
        Destroy(gameObject);

        // Play sound
        audioSrc.PlayOneShot(audioClip);

        return true;
    }
}