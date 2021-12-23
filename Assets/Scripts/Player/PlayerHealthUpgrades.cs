using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHealthUpgrades : MonoBehaviour
{
    [SerializeField] private PlayerStateObject playerState;

    private const float STEADY_REGEN_DELAY = 5f;
    private const float STEADY_REGEN_PERCENTAGE = .01f;

    private const float PEAK_SURVIVAL_TIME = 6f;
    private const float PEAK_SURVIVAL_COOLDOWN = PEAK_SURVIVAL_TIME + 6f;
    private const float PEAK_SURVIVAL_THRESHOLD = .3f;

    private float steadyRegenNextTime;

    private float peakSurvivalOffTime;
    private float peakSurvivalNextActive;
    private bool peakSurvivalThresholdReached = false; // If fell below health threshold to activate, will be set to true until health is regained above threshold

    void Start()
    {
        steadyRegenNextTime = Time.time;

        peakSurvivalOffTime = Time.time;
        peakSurvivalNextActive = Time.time;
    }

    void Update()
    {
        SteadyRegen();
        PeakOfSurvival();
    }

    private void SteadyRegen()
    {
        if (playerState.powers["SteadyRegen"].isActive)
        {
            if (Time.time > steadyRegenNextTime)
            {
                // Regen small percentage of max health
                playerState.healthCurr = playerState.healthCurr + (playerState.healthMax * STEADY_REGEN_PERCENTAGE) > playerState.healthMax ?
                    playerState.healthMax : playerState.healthCurr + (playerState.healthMax * STEADY_REGEN_PERCENTAGE);

                steadyRegenNextTime = Time.time + STEADY_REGEN_DELAY;
            }
        }
    }

    private void PeakOfSurvival()
    {
        // Player becomes invincible for some time when certain health threshold is reached
        if (playerState.powers["PeakOfSurvival"].isActive)
        {
            if (Time.time > peakSurvivalNextActive
                && !playerState.peakOfSurvivalActive 
                && !peakSurvivalThresholdReached 
                && playerState.healthCurr < playerState.healthMax * PEAK_SURVIVAL_THRESHOLD) // Check for and activate effect
            {
                playerState.peakOfSurvivalActive = true;
                peakSurvivalThresholdReached = true;

                peakSurvivalOffTime = Time.time + PEAK_SURVIVAL_TIME;
                peakSurvivalNextActive = Time.time + PEAK_SURVIVAL_COOLDOWN;
            }
            else // Effect is active
            {
                // Check for and deactive active effect after some time
                if (Time.time > peakSurvivalOffTime)
                {
                    playerState.peakOfSurvivalActive = false;
                }
            }
        }

        // After falling below health threshold, check to see if health is regained above the threshold
        if (peakSurvivalThresholdReached && playerState.healthCurr >= playerState.healthMax * PEAK_SURVIVAL_THRESHOLD)
        {
            peakSurvivalThresholdReached = false;
        }
    }
}
