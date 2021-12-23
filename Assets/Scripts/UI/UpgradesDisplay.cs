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
        textDamage.text = "Damage\n+" + playerState.damageBonus;
        textMagazine.text = "Magazine\n+" + playerState.magSizeMaxMultiplier;
        textReload.text = "Reload\n+" + playerState.reloadMultiplier;
        textFireRate.text = "Fire Rate\n+" + playerState.fireRateMultiplier;
        textHeadshot.text = "Headshot\n+" + playerState.headShotMultiplierBonus;
        textAccuracy.text = "Accuracy\n+" + playerState.inaccuracyReduction;
        textAimSpeed.text = "Aim Speed\n+" + playerState.aimTimeReduction;
        textRange.text = "Range\n+" + playerState.effectiveRangeBonus;
        textMoveSpeed.text = "Move Speed\n+" + playerState.moveSpeedBonus;
        textArmor.text = "Armor\n+" + playerState.armor;
        textMaxHealth.text = "Max Health\n+" + (playerState.START_HEALTH - playerState.healthMax);
        textJumps.text = "Jumps\n+" + playerState.jumpBonus;
    }

    private void UpdatePowers()
    {
        foreach (KeyValuePair<string, PlayerStateObject.Power> power in playerState.powers)
        {
            if (!activePowers.ContainsKey(power.Key) && power.Value.isActive)
            {
                activePowers.Add(power.Key, power.Value); // Add to active powers to avoid duplicate displays
                GameObject powerDisplay = powerDisplayItemQueue.Dequeue(); // Get first unused power display to show active power
                powerDisplay.GetComponent<PowerDisplayItem>().ShowPower(power.Value.powerIcon, power.Value.powerName);
            }    
        }
    }

    private void ShowDisplay()
    {
        playerMove.canLook = false;

        Cursor.lockState = CursorLockMode.Confined;
        upgradesScreen.alpha = 1;
        upgradesScreen.blocksRaycasts = true;
    }

    public void HideDisplay()
    {
        playerMove.canLook = true;

        Cursor.lockState = CursorLockMode.Locked;
        upgradesScreen.alpha = 0;
        upgradesScreen.blocksRaycasts = false;
    }
}
