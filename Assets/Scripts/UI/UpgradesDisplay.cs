using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UpgradesDisplay : MonoBehaviour
{
    [SerializeField] private PlayerStateObject playerState;

    [SerializeField] private Text textDamage;
    [SerializeField] private Text textMagazine;
    [SerializeField] private Text textReload;
    [SerializeField] private Text textFireRate;
    [SerializeField] private Text textHeadshot;
    [SerializeField] private Text textAccuracy;
    [SerializeField] private Text textAimSpeed;
    [SerializeField] private Text textRange;
    [SerializeField] private Text textMoveSpeed;
    [SerializeField] private Text textArmor;
    [SerializeField] private Text textMaxHealth;
    [SerializeField] private Text textJumps;

    private CanvasGroup upgradesScreen;
    private PlayerMoveController playerMove;

    [SerializeField] private List<GameObject> powerDisplays; // Power displays that can be used to show active powers
    private Queue<GameObject> powerDisplayItemQueue;
    private Dictionary<string, PlayerStateObject.Power> activePowers; // To keep track of active powers, should initially be empty

    void Start()
    {
        upgradesScreen = GetComponent<CanvasGroup>();
        playerMove = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerMoveController>();

        upgradesScreen.alpha = 0;
        upgradesScreen.blocksRaycasts = false;

        powerDisplayItemQueue = new Queue<GameObject>(powerDisplays);
        activePowers = new Dictionary<string, PlayerStateObject.Power>();
    }

    void Update()
    {
        // Return if game is paused
        if (Time.timeScale == 0)
        {
            return;
        }

        // Hold to display
        if (Input.GetKey(KeyCode.Tab))
        {
            ShowDisplay();
        }
        else
        {
            HideDisplay();
        }

        // Only update screen while it is active
        if (upgradesScreen.alpha > 0)
        {
            // Update text values
            UpdateStats();

            // Update powers display
            UpdatePowers();
        }
    }

    private void UpdateStats()
    {
        textDamage.text = "Damage\n+" + playerState.stats["DamageBonus"].statValue;
        textMagazine.text = "Magazine\n+" + playerState.stats["MagSizeMaxMultiplier"].statValue;
        textReload.text = "Reload\n+" + playerState.stats["ReloadMultiplier"].statValue;
        textFireRate.text = "Fire Rate\n+" + playerState.stats["FireRateMultiplier"].statValue;
        textHeadshot.text = "Headshot\n+" + playerState.stats["HeadShotMultiplierBonus"].statValue;
        textAccuracy.text = "Accuracy\n+" + playerState.stats["InaccuracyReduction"].statValue;
        textAimSpeed.text = "Aim Speed\n+" + playerState.stats["AimTimeReduction"].statValue;
        textRange.text = "Range\n+" + playerState.stats["EffectiveRangeBonus"].statValue;
        textMoveSpeed.text = "Move Speed\n+" + playerState.stats["MoveSpeedBonus"].statValue;
        textArmor.text = "Armor\n+" + playerState.stats["Armor"].statValue;
        textMaxHealth.text = "Max Health\n+" + (playerState.stats["HealthMax"].statValue - playerState.START_HEALTH);
        textJumps.text = "Jumps\n+" + playerState.stats["JumpBonus"].statValue;
    }

    private void UpdatePowers()
    {
        foreach (KeyValuePair<string, PlayerStateObject.Power> power in playerState.powers)
        {
            if (!activePowers.ContainsKey(power.Key) && power.Value.isActive)
            {
                activePowers.Add(power.Key, power.Value); // Add to active powers to avoid duplicate displays
                GameObject powerDisplay = powerDisplayItemQueue.Dequeue(); // Get first unused power display to show active power
                powerDisplay.GetComponent<PowerDisplayItem>().ShowPower(power.Value.powerIcon, power.Value.powerNameShort);
            }    
        }
    }

    private void ShowDisplay()
    {
        upgradesScreen.alpha = 1;
        upgradesScreen.blocksRaycasts = true;
    }

    public void HideDisplay()
    {
        upgradesScreen.alpha = 0;
        upgradesScreen.blocksRaycasts = false;
    }
}
