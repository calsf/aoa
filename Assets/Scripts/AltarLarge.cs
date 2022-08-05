using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AltarLarge : Altar
{
    private List<string> powers;
    protected override void Start()
    {
        base.Start();
        costCurr = COST_BASE_LARGE;
        costText.text = costCurr.ToString();
        powers = new List<string>(playerState.powers.Keys);
    }

    protected override void OpenAltar()
    {
        // Do not open if not enough currency
        if (playerState.bloodCurrency < costCurr)
        {
            return;
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
        Destroy(gameObject);

        // Play sound
        audioSrc.PlayOneShot(audioClip);
    }
}