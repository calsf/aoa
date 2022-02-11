using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Upgrades display to be shown when paused, allows for hover on stats/powers to show information
public class PausedUpgradesDisplay : MonoBehaviour
{
    private Vector3 INFO_OFFSET = Vector3.up * 35;

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

    [SerializeField] private CanvasGroup originalUpgradesScreen;
    [SerializeField] private List<GameObject> powerDisplays; // Power displays that can be used to show active powers
    private Queue<GameObject> powerDisplayItemQueue;
    private Dictionary<string, PlayerStateObject.Power> activePowers; // To keep track of active powers, should initially be empty

    [SerializeField] private CanvasGroup upgradeInfoCanvas;
    [SerializeField] private Image upgradeInfoImage;
    [SerializeField] private Text upgradeInfoName;
    [SerializeField] private Text upgradeInfoDesc;

    void Start()
    {
        upgradesScreen = GetComponent<CanvasGroup>();

        powerDisplayItemQueue = new Queue<GameObject>(powerDisplays);
        activePowers = new Dictionary<string, PlayerStateObject.Power>();
    }

    void Update()
    {
        // Only update screen while it is active OR the original is active to maintain consistency
        if (upgradesScreen.alpha > 0 || originalUpgradesScreen.alpha > 0)
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

                PowerDisplayItem powerDisplayItem = powerDisplay.GetComponent<PowerDisplayItem>();
                powerDisplayItem.power = power.Value; // Set power
                powerDisplayItem.ShowPower(power.Value.powerIcon, power.Value.powerNameShort);
            }
        }
    }

    public void ShowDisplay()
    {
        upgradesScreen.alpha = 1;
        upgradesScreen.blocksRaycasts = true;
    }

    public void HideDisplay()
    {
        upgradesScreen.alpha = 0;
        upgradesScreen.blocksRaycasts = false;
    }

    public void ShowStatInfo(StatDisplayItem item)
    {
        if (item.stat.statIcon == null)
        {
            return;
        }

        upgradeInfoCanvas.alpha = 1;
        upgradeInfoCanvas.transform.position = item.transform.position + INFO_OFFSET;
        upgradeInfoImage.sprite = item.stat.statIcon;
        upgradeInfoName.text = item.stat.statName;
        upgradeInfoDesc.text = item.stat.statDesc;
    }

    public void HideStatInfo(StatDisplayItem item)
    {
        upgradeInfoCanvas.alpha = 0;
    }

    public void ShowPowerInfo(PowerDisplayItem item)
    {
        if (item.power.powerIcon == null)
        {
            return;
        }

        upgradeInfoCanvas.alpha = 1;
        upgradeInfoCanvas.transform.position = item.transform.position + INFO_OFFSET;
        upgradeInfoImage.sprite = item.power.powerIcon;
        upgradeInfoName.text = item.power.powerName;
        upgradeInfoDesc.text = item.power.powerDesc;
    }

    public void HidePowerInfo(PowerDisplayItem item)
    {
        upgradeInfoCanvas.alpha = 0;
    }
}
