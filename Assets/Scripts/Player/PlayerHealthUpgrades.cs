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

    public float peakSurvivalOffTime { get; set; }
    public float peakSurvivalNextActive { get; set; }
    private bool peakSurvivalThresholdReached = false; // If fell below health threshold to activate, will be set to true until health is regained above threshold

    private AudioSource audioSrc;

    [SerializeField] private AudioClip peakOfSurvivalActivate;
    [SerializeField] private AudioClip peakOfSurvivalEnd;

    private bool hasPlayedWarning = false;

    void Start()
    {
        // Set up audio, use Player's audio source
        audioSrc = GameObject.FindGameObjectWithTag("Player").GetComponent<AudioSource>();
        GameObject.FindGameObjectWithTag("SoundManager").GetComponent<SoundManager>().AddAudioSource(audioSrc);

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
                playerState.healthCurr = playerState.healthCurr + (playerState.stats["HealthMax"].statValue * STEADY_REGEN_PERCENTAGE) > playerState.stats["HealthMax"].statValue ?
                    playerState.stats["HealthMax"].statValue : playerState.healthCurr + (playerState.stats["HealthMax"].statValue * STEADY_REGEN_PERCENTAGE);

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
                && playerState.healthCurr < playerState.stats["HealthMax"].statValue * PEAK_SURVIVAL_THRESHOLD) // Check for and activate effect
            {
                audioSrc.PlayOneShot(peakOfSurvivalActivate);

                playerState.peakOfSurvivalActive = true;
                peakSurvivalThresholdReached = true;

                peakSurvivalOffTime = Time.time + PEAK_SURVIVAL_TIME;
                peakSurvivalNextActive = Time.time + PEAK_SURVIVAL_COOLDOWN;
            }
            else if (playerState.peakOfSurvivalActive) // Effect is active
            {
                // Play sound a little bit before effect ends
                if (!hasPlayedWarning && Time.time > peakSurvivalOffTime - 1f)
                {
                    hasPlayedWarning = true;

                    audioSrc.PlayOneShot(peakOfSurvivalEnd);
                }

                // Check for and deactive active effect after some time
                if (Time.time > peakSurvivalOffTime)
                {
                    playerState.peakOfSurvivalActive = false;

                    peakSurvivalThresholdReached = false; // Will end up re-activating if player still below threshold

                    hasPlayedWarning = false;
                }
            }
        }

        /*
        // After falling below health threshold, check to see if health is regained above the threshold
        if (peakSurvivalThresholdReached && playerState.healthCurr >= playerState.stats["HealthMax"].statValue * PEAK_SURVIVAL_THRESHOLD)
        {
            peakSurvivalThresholdReached = false;
        }*/
    }
}
