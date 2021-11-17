using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "New Player State", menuName = "Player State")]
public class PlayerStateObject : ScriptableObject
{
    private const float INVULN_TIME = 1f;
    private float nextDamagedTime;

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

    public bool aimGlide;
    public bool holsteredReload;
    public bool punchThrough;
    public bool explosiveShot;
    public bool clonedShot;
    public bool sacrificialShot;
    public bool coldShot;
    public bool stunShot;
    public bool crippleShot;
    public bool heavyShot;

    public bool tempoShot;
    public bool loadedShot;
    public bool decoyShot;
    public bool luckyShot;
    public bool peakOfSurvival;

    // Objects that need to be updated should listen for this event to be invoked
    public UnityEvent OnStateUpdate;

    // Objects that need to be updated when player gets damaged should listen for this event to be invoked
    public UnityEvent OnPlayerDamaged;

    public void DamagePlayer(float damage)
    {
        // Return if player is still invulnerable from previous damaged
        if (Time.time < nextDamagedTime)
        {
            return;
        }

        nextDamagedTime = Time.time + INVULN_TIME;
        healthCurr -= damage;

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

        healthMax = 100;
        healthCurr = healthMax;

        aimGlide = false;
        holsteredReload = false;
        punchThrough = false;
        explosiveShot = false;
        clonedShot = false;
        sacrificialShot = false;
        coldShot = false;
        stunShot = false;
        crippleShot = false;
        heavyShot = false;

        tempoShot = false;
        loadedShot = false;
        decoyShot = false;
        luckyShot = false;
        peakOfSurvival = false;

        nextDamagedTime = Time.time;

        OnStateUpdate.Invoke();
    }
}
