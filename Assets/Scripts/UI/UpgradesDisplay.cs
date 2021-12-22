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
    [SerializeField] private Text textMaxHealth;
    [SerializeField] private Text textJumps;

    private CanvasGroup upgradesScreen;
    private PlayerMoveController playerMove;

    void Start()
    {
        upgradesScreen = GetComponent<CanvasGroup>();
        playerMove = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerMoveController>();

        upgradesScreen.alpha = 0;
        upgradesScreen.blocksRaycasts = false;
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

        // Update text values
        UpdateStats();
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
        textMaxHealth.text = "Max Health\n+" + (playerState.START_HEALTH - playerState.healthMax);
        textJumps.text = "Jumps\n+" + playerState.jumpBonus;
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
