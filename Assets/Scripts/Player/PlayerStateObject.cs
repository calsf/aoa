using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "New Player State", menuName = "Player State")]
public class PlayerStateObject : ScriptableObject
{
    private const float INVULN_TIME = 1f;
    private float nextDamagedTime;
    public float START_HEALTH = 100;

    public float tempoShotExtraDmg { get; set; }
    public bool peakOfSurvivalActive { get; set; }

    public int selectedPrimary;
    public int selectedSecondary;
    public int selectedActive; // Selected active weapon, 0 for primary, 1 for secondary

    public float reloadMultiplier;     // Anim dependent, all weapons have 1 speed multiplier by default
    public float fireRateMultiplier;     // Anim dependent, all weapons have 1 speed multiplier by default
    public float damageBonus;
    public float headShotMultiplierBonus;
    public int magSizeMaxMultiplier;    // Should always be 1
    public float aimTimeReduction;
    public float inaccuracyReduction;
    public float effectiveRangeBonus;

    public float moveSpeedBonus;
    public int jumpBonus;

    public float healthMax;
    public float healthCurr;

    // Implemented
    public bool aimGlide;
    public bool holsteredReload;
    public bool punchThrough;
    public bool sacrificialShot;
    public bool tacticalShot;
    public bool coldShot;
    public bool weakeningShot;
    public bool tempoShot;
    public bool peakOfSurvival;
    public bool steadyRegen;
    public bool explosiveShot;
    public bool clonedShot;
    public bool defiantReload;
    public bool decoyShot;
    public bool airSlide;

    // TODO:
    public bool luckyShot;

    // Objects that need to be updated should listen for this event to be invoked
    public UnityEvent OnStateUpdate;

    // Objects that need to be updated when player gets damaged should listen for this event to be invoked
    public UnityEvent OnPlayerDamaged;

    public void DamagePlayer(float damage)
    {
        // Return if player is still invulnerable from previous damaged or is invincible
        if (Time.time < nextDamagedTime || peakOfSurvivalActive)
        {
            return;
        }

        nextDamagedTime = Time.time + INVULN_TIME;
        healthCurr = healthCurr - damage < 0 ? 0 : healthCurr - damage;

        Debug.Log("OUCH! Health: " + healthCurr.ToString());

        OnPlayerDamaged.Invoke();
    }

    public void InitializeState()
    {
        selectedPrimary = 1;
        selectedSecondary = 3;
        selectedActive = 0;

        reloadMultiplier = 1;
        fireRateMultiplier = 1;
        damageBonus = 0;
        headShotMultiplierBonus = 0;
        magSizeMaxMultiplier = 1;
        aimTimeReduction = 0;
        inaccuracyReduction = 0;
        effectiveRangeBonus = 0;
        
        moveSpeedBonus = 0;
        jumpBonus = 0;

        healthMax = START_HEALTH;
        healthCurr = healthMax;

        tempoShotExtraDmg = 0;

        aimGlide = false;
        holsteredReload = false;
        punchThrough = false;
        explosiveShot = false;
        clonedShot = false;
        sacrificialShot = false;
        coldShot = false;
        weakeningShot = false;
        tacticalShot = false;
        defiantReload = false;

        tempoShot = false;
        decoyShot = false;
        luckyShot = false;
        peakOfSurvival = false;
        steadyRegen = false;

        nextDamagedTime = Time.time;

        OnStateUpdate.Invoke();
    }
}
