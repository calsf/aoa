using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "New Player State", menuName = "Player State")]
public class PlayerStateObject : ScriptableObject
{
    private const int MAX_BLOOD_CURRENCY = 999999999;
    private const int MAX_HEALTH = 100000000;
    private const float INVULN_TIME = 1f;
    private float nextDamagedTime;
    public float START_HEALTH = 100;
    public float SWAP_MULTIPLIER = .1f;

    public float tempoShotExtraDmg { get; set; }
    public bool peakOfSurvivalActive { get; set; }
    public float bonusSwapDamage { get; set; }
    public float bonusVengeDamage { get; set; }

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


    public Sprite reloadMultiplierIcon;
    public Sprite fireRateMultiplierIcon;
    public Sprite damageBonusIcon;
    public Sprite headShotMultiplierBonusIcon;
    public Sprite magSizeMaxMultiplierIcon;
    public Sprite aimTimeReductionIcon;
    public Sprite inaccuracyReductionIcon;
    public Sprite effectiveRangeBonusIcon;
    public Sprite moveSpeedBonusIcon;
    public Sprite jumpBonusIcon;
    public Sprite healthMaxIcon;
    public Sprite armorIcon;


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
    public Sprite swapShotIcon;
    public Sprite vengeanceIcon;
    public Sprite rocketJumpIcon;

    // Objects that need to be updated should listen for this event to be invoked
    public UnityEvent OnStateUpdate;

    // Objects that need to be updated when player gets damaged should listen for this event to be invoked
    public UnityEvent OnPlayerDamaged;

    // These events will pass the upgraded Stat or Power as an argument when invoked
    public UnityEvent<Stat> OnUpgradeStat;
    public UnityEvent<Power> OnUpgradePower;

    // Struct for a Power
    public struct Power
    {
        public bool isActive;
        public Sprite powerIcon;
        public string powerNameShort;
        public string powerName;
        public string powerDesc;

        public Power(bool isActive, Sprite powerIcon, string powerNameShort, string powerName, string powerDesc)
        {
            this.isActive = isActive;
            this.powerIcon = powerIcon;
            this.powerNameShort = powerNameShort;
            this.powerName = powerName;
            this.powerDesc = powerDesc;
        }
    }

    // Struct for a Stat
    public struct Stat
    {
        public float statValue;
        public float minValue;
        public float maxValue;
        public Sprite statIcon;
        public string statName;
        public string statDesc;
        public SetStatDelegate setStat;
        public DecreaseStatDelegate decreaseStat;

        public Stat(float statValue, float maxValue, Sprite statIcon, string statName, string statDesc, SetStatDelegate setStat, DecreaseStatDelegate decreaseStat)
        {
            this.statValue = statValue;
            this.minValue = statValue;
            this.maxValue = maxValue;
            this.statIcon = statIcon;
            this.statName = statName;
            this.statDesc = statDesc;
            this.setStat = setStat;
            this.decreaseStat = decreaseStat;
        }
    }

    public void DamagePlayer(float damage)
    {
        // Return if player is still invulnerable from previous damaged or is invincible
        if (Time.time < nextDamagedTime || peakOfSurvivalActive)
        {
            return;
        }

        // Vengeance - store unreduced damage taken as bonus damage
        if (powers["Vengeance"].isActive)
        {
            bonusVengeDamage += damage;
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
        powers.Add("AimGlide", new Power(
            false,
            aimGlideIcon,
            "Aim Glide",
            "Aim Glide",
            "Aiming down sights while in the air will slow down your fall."));
        powers.Add("HolsteredReload", new Power(
            false,
            holsteredReloadIcon,
            "Holstered",
            "Holstered Reload",
            "Gradually reloads your holstered weapon."));
        powers.Add("Punchthrough", new Power(
            false,
            punchThroughIcon,
            "Pierce",
            "Piercing Shot",
            "Shots can pierce through multiple enemies. Does not pierce through walls."));
        powers.Add("SacrificialShot", new Power(
            false,
            sacrificialShotIcon,
            "Sacrificial",
            "Sacrificial Shot",
            "Lose health per shot. Recover double the health lost if the shot hits an enemy."));
        powers.Add("TacticalShot", new Power(
            false,
            tacticalShotIcon,
            "Tactical",
            "Tactical Shot",
            "Shots within effective range will break walls in one hit. Broken walls will leave behind a temporary gas cloud."));
        powers.Add("ColdShot", new Power(
            false, 
            coldShotIcon, 
            "Cold",
            "Cold Shot",
            "Shots will lower the move speed of an enemy for a short duration."));
        powers.Add("WeakeningShot", new Power(
            false, 
            weakeningShotIcon, 
            "Weakening",
            "Weakening Shot",
            "Shots will lower the damage of an enemy for a short duration."));
        powers.Add("TempoShot", new Power(
            false, 
            tempoShotIcon, 
            "Tempo",
            "Tempo Shot",
            "Gain bonus damage with each consecutive headshot. A body shot will reset bonus damage to 0."));
        powers.Add("PeakOfSurvival", new Power(
            false, 
            peakOfSurvivalIcon, 
            "Survival",
            "Peak of Survival",
            "When dropped to low health, become invincible for a short duration. Goes on cooldown afterwards."));
        powers.Add("SteadyRegen", new Power(
            false, 
            steadyRegenIcon, 
            "Regen",
            "Steady Regen",
            "Gradually recover health over time."));
        powers.Add("ExplosiveShot", new Power(
            false, 
            explosiveShotIcon, 
            "Explosive",
            "Explosive Death",
            "Enemies explode on death."));
        powers.Add("ClonedShot", new Power(
            false, 
            clonedShotIcon, 
            "Cloned",
            "Cloned Shot",
            "Additional shots are fired near the original shot."));
        powers.Add("DefiantReload", new Power(
            false, 
            defiantReloadIcon, 
            "Defiant",
            "Defiant Reload",
            "Reloading knocks back all nearby enemies."));
        powers.Add("DecoyShot", new Power(
            true, 
            decoyShotIcon, 
            "Decoy",
            "Decoy Shot",
            "The last shot of a magazine will spawn a decoy that can attract up to 5 nearby enemies."));
        powers.Add("AirSlide", new Power(
            false, 
            airSlideIcon, 
            "Air Slide",
            "Air Slide",
            "Sliding can be performed while in the air."));
        powers.Add("SwapShot", new Power(
            false, 
            swapShotIcon, 
            "Swap Shot",
            "Swap Shot",
            "Deal increased damage on the first shot after swapping weapons."));
        powers.Add("Vengeance", new Power(
            false,
            vengeanceIcon,
            "Vengeance",
            "Vengeance",
            "Stores damage taken as bonus damage on the next shot. Stored damage is applied before armor reduction."));
        powers.Add("RocketJump", new Power(
            false,
            rocketJumpIcon,
            "Rocket Jump",
            "Rocket Jump",
            "Drop a rocket when jumping. Deals more damage based on Damage Bonus stat."));

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
        stats.Add("ReloadMultiplier", new Stat(
            1,
            10,
            reloadMultiplierIcon,
            "Reload Speed",
            "Increases reload speed.",
            SetReload,
            DecreaseReload));
        stats.Add("FireRateMultiplier", new Stat(
            1,
            10,
            fireRateMultiplierIcon,
            "Fire Rate",
            "Increases fire rate.",
            SetFireRate,
            DecreaseFireRate));
        stats.Add("DamageBonus", new Stat(
            0,
            1000,
            damageBonusIcon,
            "Damage Bonus",
            "Increases weapon damage.",
            SetDamageBonus,
            DecreaseDamageBonus));
        stats.Add("HeadShotMultiplierBonus", new Stat(
            0,
            10,
            headShotMultiplierBonusIcon,
            "Headshot Damage",
            "Increases headshot damage.",
            SetHeadShot,
            DecreaseHeadShot));
        stats.Add("MagSizeMaxMultiplier", new Stat(
            1,
            15,
            magSizeMaxMultiplierIcon,
            "Magazine Size",
            "Increases maximum magazine size.",
            SetMagazine,
            DecreaseMagazine));
        stats.Add("AimTimeReduction", new Stat(
            0,
            1.5f,
            aimTimeReductionIcon,
            "Aim Speed", 
            "Increases aim down sight speed.",
            SetAimSpeed,
            DecreaseAimSpeed));
        stats.Add("InaccuracyReduction", new Stat(
            0,
            1,
            inaccuracyReductionIcon,
            "Accuracy",
            "Increases hip fire accuracy.",
            SetAccuracy,
            DecreaseAccuracy));
        stats.Add("EffectiveRangeBonus", new Stat(
            0,
            1000,
            effectiveRangeBonusIcon,
            "Range", 
            "Increases the effective range of weapons.",
            SetRange,
            DecreaseRange));
        stats.Add("MoveSpeedBonus", new Stat(
            0,
            45,
            moveSpeedBonusIcon,
            "Move Speed",
            "Increases move speed.", 
            SetMoveSpeed,
            DecreaseMoveSpeed));
        stats.Add("JumpBonus", new Stat(
            15,
            15,
            jumpBonusIcon,
            "Jump", 
            "Increases the number of jumps.",
            SetJumps,
            DecreaseJumps));
        stats.Add("Armor", new Stat(
            0,
            .9f,
            armorIcon,
            "Armor", 
            "Increases damage reduction.",
            SetArmor,
            DecreaseArmor));
        stats.Add("HealthMax", new Stat(
            START_HEALTH,
            MAX_HEALTH,
            healthMaxIcon,
            "Max Health", 
            "Increases maximum health.",
            SetMaxHealth,
            DecreaseMaxHealth));

        //selectedPrimary = 2;
        //selectedSecondary = 1;
        selectedActive = 0;

        healthCurr = START_HEALTH;

        tempoShotExtraDmg = 0;

        peakOfSurvivalActive = false;

        bonusSwapDamage = 0;

        bonusVengeDamage = 0;
        
        bloodCurrency = 0;

        nextDamagedTime = Time.time;

        OnStateUpdate.Invoke();
    }

    public void UpdateStat(string key, Stat value)
    {
        if (value.statValue == stats[key].statValue)    // If stat is capped, show different description
        {
            OnUpgradeStat.Invoke(new Stat(
            value.statValue,
            value.maxValue,
            value.statIcon,
            value.statName,
            "You feel no change...",
            value.setStat,
            value.decreaseStat));
        }
        else  // Else, upgrade and display normally
        {
            stats[key] = value;

            OnStateUpdate.Invoke();
            OnUpgradeStat.Invoke(value);
        }
    }

    public void UpdatePower(string key, Power value)
    {
        powers[key] = value;

        OnStateUpdate.Invoke();
        OnUpgradePower.Invoke(value);
    }

    // Delegate to upgrade a specific stat value
    public delegate float SetStatDelegate();

    private float SetReload()
    {
        // Cap max reload
        if (stats["ReloadMultiplier"].statValue >= stats["ReloadMultiplier"].maxValue)
        {
            return stats["ReloadMultiplier"].maxValue;
        }

        return (float)((decimal)stats["ReloadMultiplier"].statValue + .4m);
    }

    private float SetFireRate()
    {
        // Cap max fire rate
        if (stats["FireRateMultiplier"].statValue >= stats["FireRateMultiplier"].maxValue)
        {
            return stats["FireRateMultiplier"].maxValue;
        }

        return (float)((decimal)stats["FireRateMultiplier"].statValue + .25m);
    }

    private float SetDamageBonus()
    {
        // Cap max damage bonus
        if (stats["DamageBonus"].statValue >= stats["DamageBonus"].maxValue)
        {
            return stats["DamageBonus"].maxValue;
        }

        return stats["DamageBonus"].statValue + 10;
    }

    private float SetHeadShot()
    {
        // Cap max headshot multiplier bonus
        if (stats["HeadShotMultiplierBonus"].statValue >= stats["HeadShotMultiplierBonus"].maxValue)
        {
            return stats["HeadShotMultiplierBonus"].maxValue;
        }

        return (float)((decimal)stats["HeadShotMultiplierBonus"].statValue + .25m);
    }

    private float SetMagazine()
    {
        // Cap max magazine
        if (stats["MagSizeMaxMultiplier"].statValue >= stats["MagSizeMaxMultiplier"].maxValue)
        {
            return stats["MagSizeMaxMultiplier"].maxValue;
        }

        return (float)((decimal)stats["MagSizeMaxMultiplier"].statValue + .3m);
    }

    private float SetAimSpeed()
    {
        // Cap max aim speed
        if (stats["AimTimeReduction"].statValue >= stats["AimTimeReduction"].maxValue)
        {
            return stats["AimTimeReduction"].maxValue;
        }

        return (float)((decimal)stats["AimTimeReduction"].statValue + .03m);
    }

    private float SetAccuracy()
    {
        // Cap max accuracy
        if (stats["InaccuracyReduction"].statValue >= stats["InaccuracyReduction"].maxValue)
        {
            return stats["InaccuracyReduction"].maxValue;
        }

        return (float)((decimal)stats["InaccuracyReduction"].statValue + .01m);
    }

    private float SetRange()
    {
        // Cap max range
        if (stats["EffectiveRangeBonus"].statValue >= stats["EffectiveRangeBonus"].maxValue)
        {
            return stats["EffectiveRangeBonus"].maxValue;
        }

        return stats["EffectiveRangeBonus"].statValue + 50;
    }

    private float SetMoveSpeed()
    {
        // Cap max move speed
        if (stats["MoveSpeedBonus"].statValue >= stats["MoveSpeedBonus"].maxValue)
        {
            return stats["MoveSpeedBonus"].maxValue;
        }

        return stats["MoveSpeedBonus"].statValue + 3;
    }

    private float SetJumps()
    {
        // Cap max jumps
        if (stats["JumpBonus"].statValue >= stats["JumpBonus"].maxValue)
        {
            return stats["JumpBonus"].maxValue;
        }

        return stats["JumpBonus"].statValue + 1;
    }

    private float SetArmor()
    {
        // Cap max armor
        if (stats["Armor"].statValue >= stats["Armor"].maxValue)
        {
            return stats["Armor"].maxValue;
        }

        float newVal = (float)((decimal)stats["Armor"].statValue + .1m);

        // Lower increase amount after some value
        if (stats["Armor"].statValue >= .5f)
        {
            newVal = (float)((decimal)stats["Armor"].statValue + .05m);
        }

        return newVal;
    }

    private float SetMaxHealth()
    {
        // Cap max health
        if (stats["HealthMax"].statValue >= stats["HealthMax"].maxValue)
        {
            return stats["HealthMax"].maxValue;
        }

        float increaseAmount = 25;

        // Increase current health by max health increase amount
        healthCurr += increaseAmount;
        if (healthCurr > stats["HealthMax"].statValue + increaseAmount)
        {
            healthCurr = stats["HealthMax"].statValue + increaseAmount;
        }
        
        return stats["HealthMax"].statValue + increaseAmount;
    }

    // --- Stat decreases for play menu ---

    // Delegate to decrease a specific stat value
    public delegate float DecreaseStatDelegate();

    private float DecreaseReload()
    {
        if (stats["ReloadMultiplier"].statValue <= stats["ReloadMultiplier"].minValue)
        {
            return stats["ReloadMultiplier"].minValue;
        }

        return (float)((decimal) stats["ReloadMultiplier"].statValue - .4m);
    }

    private float DecreaseFireRate()
    {
        if (stats["FireRateMultiplier"].statValue <= stats["FireRateMultiplier"].minValue)
        {
            return stats["FireRateMultiplier"].minValue;
        }

        return (float)((decimal)stats["FireRateMultiplier"].statValue - .25m);
    }

    private float DecreaseDamageBonus()
    {
        if (stats["DamageBonus"].statValue <= stats["DamageBonus"].minValue)
        {
            return stats["DamageBonus"].minValue;
        }

        return stats["DamageBonus"].statValue - 10;
    }

    private float DecreaseHeadShot()
    {
        if (stats["HeadShotMultiplierBonus"].statValue <= stats["HeadShotMultiplierBonus"].minValue)
        {
            return stats["HeadShotMultiplierBonus"].minValue;
        }

        return (float)((decimal)stats["HeadShotMultiplierBonus"].statValue - .25m);
    }

    private float DecreaseMagazine()
    {
        if (stats["MagSizeMaxMultiplier"].statValue <= stats["MagSizeMaxMultiplier"].minValue)
        {
            return stats["MagSizeMaxMultiplier"].minValue;
        }

        return (float)((decimal)stats["MagSizeMaxMultiplier"].statValue - .3m);
    }

    private float DecreaseAimSpeed()
    {
        if (stats["AimTimeReduction"].statValue <= stats["AimTimeReduction"].minValue)
        {
            return stats["AimTimeReduction"].minValue;
        }

        return (float)((decimal)stats["AimTimeReduction"].statValue - .03m);
    }

    private float DecreaseAccuracy()
    {
        if (stats["InaccuracyReduction"].statValue <= stats["InaccuracyReduction"].minValue)
        {
            return stats["InaccuracyReduction"].minValue;
        }

        return (float)((decimal)stats["InaccuracyReduction"].statValue - .01m);
    }

    private float DecreaseRange()
    {
        if (stats["EffectiveRangeBonus"].statValue <= stats["EffectiveRangeBonus"].minValue)
        {
            return stats["EffectiveRangeBonus"].minValue;
        }

        return stats["EffectiveRangeBonus"].statValue - 50;
    }

    private float DecreaseMoveSpeed()
    {
        if (stats["MoveSpeedBonus"].statValue <= stats["MoveSpeedBonus"].minValue)
        {
            return stats["MoveSpeedBonus"].minValue;
        }

        return stats["MoveSpeedBonus"].statValue - 3;
    }

    private float DecreaseJumps()
    {
        if (stats["JumpBonus"].statValue <= stats["JumpBonus"].minValue)
        {
            return stats["JumpBonus"].minValue;
        }

        return stats["JumpBonus"].statValue - 1;
    }

    private float DecreaseArmor()
    {
        if (stats["Armor"].statValue <= stats["Armor"].minValue)
        {
            return stats["Armor"].minValue;
        }

        float newVal;

        if (stats["Armor"].statValue > .5f)
        {
            newVal = (float)((decimal)stats["Armor"].statValue - .05m);
        } else
        {
            newVal = (float)((decimal)stats["Armor"].statValue - .1m);
        }

        return newVal;
    }

    private float DecreaseMaxHealth()
    {
        if (stats["HealthMax"].statValue <= stats["HealthMax"].minValue)
        {
            return stats["HealthMax"].minValue;
        }

        float decreaseAmount = 25;

        return stats["HealthMax"].statValue - decreaseAmount;
    }

    public void AddBloodCurrency(int amount)
    {
        // Cap max blood currency
        bloodCurrency = bloodCurrency + amount >= MAX_BLOOD_CURRENCY ? MAX_BLOOD_CURRENCY : bloodCurrency + amount;
    }
}
