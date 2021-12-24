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
    public Dictionary<string, Power> powers { get; set; }
    public Dictionary<string, Stat> stats { get; set; }

    public float healthCurr;
    public int bloodCurrency;

    public int selectedPrimary;
    public int selectedSecondary;
    public int selectedActive; // Selected active weapon, 0 for primary, 1 for secondary

    /*
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

    public float armor;
    */

    /*
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
    */

    public Sprite aimGlideIcon;
    public Sprite holsteredReloadIcon;
    public Sprite punchThroughIcon;
    public Sprite sacrificialShotIcon;
    public Sprite tacticalShotIcon;
    public Sprite coldShotIcon;
    public Sprite weakeningShotIcon;
    public Sprite tempoShotIcon;
    public Sprite peakOfSurvivalIcon;
    public Sprite steadyRegenIcon;
    public Sprite explosiveShotIcon;
    public Sprite clonedShotIcon;
    public Sprite defiantReloadIcon;
    public Sprite decoyShotIcon;
    public Sprite airSlideIcon;

    public Sprite luckyShotIcon;

    // Objects that need to be updated should listen for this event to be invoked
    public UnityEvent OnStateUpdate;

    // Objects that need to be updated when player gets damaged should listen for this event to be invoked
    public UnityEvent OnPlayerDamaged;

    // Struct for a Power
    public struct Power
    {
        public bool isActive;
        public Sprite powerIcon;
        public string powerName;
        //public string powerDesc;

        public Power(bool isActive, Sprite powerIcon, string powerName)
        {
            this.isActive = isActive;
            this.powerIcon = powerIcon;
            this.powerName = powerName;
        }
    }

    // Struct for a Stat
    public struct Stat
    {
        public float statValue;
        //public Sprite statIcon;
        public string statName;
        //public string statDesc;
        public SetStatDelegate setStat;

        public Stat(float statValue, string statName, SetStatDelegate setStat)
        {
            this.statValue = statValue;
            this.statName = statName;
            this.setStat = setStat;
        }
    }

    public void DamagePlayer(float damage)
    {
        // Return if player is still invulnerable from previous damaged or is invincible
        if (Time.time < nextDamagedTime || peakOfSurvivalActive)
        {
            return;
        }

        // Apply armor damage reduction before taking damage
        damage = damage - (damage * stats["Armor"].statValue);

        nextDamagedTime = Time.time + INVULN_TIME;
        healthCurr = healthCurr - damage < 0 ? 0 : healthCurr - damage;

        Debug.Log("OUCH! Health: " + healthCurr.ToString());

        OnPlayerDamaged.Invoke();
    }

    public void InitializeState()
    {
        powers = new Dictionary<string, Power>();
        powers.Add("AimGlide", new Power(false, aimGlideIcon, "Aim Glide"));
        powers.Add("HolsteredReload", new Power(false, holsteredReloadIcon, "Holstered"));
        powers.Add("Punchthrough", new Power(false, punchThroughIcon, "Punchthrough"));
        powers.Add("SacrificialShot", new Power(true, sacrificialShotIcon, "Sacrificial"));
        powers.Add("TacticalShot", new Power(true, tacticalShotIcon, "Tactical"));
        powers.Add("ColdShot", new Power(true, coldShotIcon, "Cold"));
        powers.Add("WeakeningShot", new Power(true, weakeningShotIcon, "Weakening"));
        powers.Add("TempoShot", new Power(true, tempoShotIcon, "Tempo"));
        powers.Add("PeakOfSurvival", new Power(true, peakOfSurvivalIcon, "Peak Survival"));
        powers.Add("SteadyRegen", new Power(true, steadyRegenIcon, "Regen"));
        powers.Add("ExplosiveShot", new Power(true, explosiveShotIcon, "Explosive"));
        powers.Add("ClonedShot", new Power(true, clonedShotIcon, "Cloned"));
        powers.Add("DefiantReload", new Power(true, defiantReloadIcon, "Defiant"));
        powers.Add("DecoyShot", new Power(true, decoyShotIcon, "Decoy"));
        powers.Add("AirSlide", new Power(true, airSlideIcon, "Air Slide"));
        powers.Add("LuckyShot", new Power(false, luckyShotIcon, "Lucky"));

        /*
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

        armor = 0;

        healthMax = START_HEALTH;
        */

        stats = new Dictionary<string, Stat>();
        stats.Add("ReloadMultiplier", new Stat(1, "Reload", SetReload));
        stats.Add("FireRateMultiplier", new Stat(1, "Fire Rate", SetFireRate));
        stats.Add("DamageBonus", new Stat(0, "Damage", SetDamageBonus));
        stats.Add("HeadShotMultiplierBonus", new Stat(0, "Headshot", SetHeadShot));
        stats.Add("MagSizeMaxMultiplier", new Stat(1, "Magazine", SetMagazine));
        stats.Add("AimTimeReduction", new Stat(0, "Aim Speed", SetAimSpeed));
        stats.Add("InaccuracyReduction", new Stat(0, "Accuracy", SetAccuracy));
        stats.Add("EffectiveRangeBonus", new Stat(0, "Range", SetRange));
        stats.Add("MoveSpeedBonus", new Stat(0, "Move Speed", SetMoveSpeed));
        stats.Add("JumpBonus", new Stat(0, "Jumps", SetJumps));
        stats.Add("Armor", new Stat(0, "Armor", SetArmor));
        stats.Add("HealthMax", new Stat(START_HEALTH, "Max Health", SetMaxHealth));

        selectedPrimary = 1;
        selectedSecondary = 3;
        selectedActive = 0;

        healthCurr = START_HEALTH;

        tempoShotExtraDmg = 0;

        /*
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
        */

        bloodCurrency = 0;

        nextDamagedTime = Time.time;

        OnStateUpdate.Invoke();
    }

    public void UpdateStat(string key, Stat value)
    {
        stats[key] = value;

        OnStateUpdate.Invoke();
    }

    public void UpdatePower(string key, Power value)
    {
        powers[key] = value;

        OnStateUpdate.Invoke();
    }

    // Delegate to upgrade a specific stat value
    public delegate float SetStatDelegate();

    private float SetReload()
    {
        return stats["ReloadMultiplier"].statValue + 3;
    }

    private float SetFireRate()
    {
        return stats["FireRateMultiplier"].statValue + 3;
    }

    private float SetDamageBonus()
    {
        return stats["DamageBonus"].statValue + 3;
    }

    private float SetHeadShot()
    {
        return stats["HeadShotMultiplierBonus"].statValue + 3;
    }

    private float SetMagazine()
    {
        return stats["MagSizeMaxMultiplier"].statValue + 3;
    }

    private float SetAimSpeed()
    {
        return stats["AimTimeReduction"].statValue + 3;
    }

    private float SetAccuracy()
    {
        return stats["InaccuracyReduction"].statValue + 3;
    }

    private float SetRange()
    {
        return stats["EffectiveRangeBonus"].statValue + 3;
    }

    private float SetMoveSpeed()
    {
        return stats["MoveSpeedBonus"].statValue + 3;
    }

    private float SetJumps()
    {
        return stats["JumpBonus"].statValue + 3;
    }

    private float SetArmor()
    {
        return stats["Armor"].statValue + 3;
    }

    private float SetMaxHealth()
    {
        return stats["HealthMax"].statValue + 3;
    }
}
