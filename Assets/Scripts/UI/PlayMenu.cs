using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayMenu : MonoBehaviour
{
    private Vector3 INFO_OFFSET = Vector3.up * 48;

    [SerializeField] private ShardCurrencyDisplay shardDisplay;
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

    [SerializeField] private CanvasGroup upgradeInfoCanvas;
    [SerializeField] private Image upgradeInfoImage;
    [SerializeField] private Text upgradeInfoName;
    [SerializeField] private Text upgradeInfoDesc;

    [SerializeField] private CanvasGroup currencyInfoCanvas;

    private int initialShardAmount;

    private Animator fade;

    [SerializeField] private AudioSource failAudioSrc;

    void Start()
    {
        GameObject.FindGameObjectWithTag("SoundManager").GetComponent<SoundManager>().AddAudioSource(failAudioSrc);

        UpdateStats();
        initialShardAmount = shardDisplay.shardCurrency;

        fade = GameObject.FindGameObjectWithTag("Fade").GetComponent<Animator>();
    }

    private void UpdateStats()
    {
        textDamage.text = "Damage\n+" + playerState.stats["DamageBonus"].statValue;
        textMagazine.text = "Magazine\n+" + ((Mathf.RoundToInt(playerState.stats["MagSizeMaxMultiplier"].statValue * 100)) - 100) + "%";
        textReload.text = "Reload\n+" + ((Mathf.RoundToInt(playerState.stats["ReloadMultiplier"].statValue * 100)) - 100) + "%";
        textFireRate.text = "Fire Rate\n+" + ((Mathf.RoundToInt(playerState.stats["FireRateMultiplier"].statValue * 100)) - 100) + "%";
        textHeadshot.text = "Headshot\n+" + (Mathf.RoundToInt(playerState.stats["HeadShotMultiplierBonus"].statValue * 100)) + "%";
        textAccuracy.text = "Accuracy\n+" + playerState.stats["InaccuracyReduction"].statValue.ToString("0.00");
        textAimSpeed.text = "Aim Speed\n+" + playerState.stats["AimTimeReduction"].statValue.ToString("0.00");
        textRange.text = "Range\n+" + playerState.stats["EffectiveRangeBonus"].statValue;
        textMoveSpeed.text = "Move Speed\n+" + playerState.stats["MoveSpeedBonus"].statValue;
        textArmor.text = "Armor\n+" + (Mathf.RoundToInt(playerState.stats["Armor"].statValue * 100)) + "%";
        textMaxHealth.text = "Max Health\n+" + (playerState.stats["HealthMax"].statValue - playerState.START_HEALTH);
        textJumps.text = "Jumps\n+" + playerState.stats["JumpBonus"].statValue;
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

    public void ShowCurrencyInfo()
    {
        currencyInfoCanvas.alpha = 1;
    }

    public void HideCurrencyInfo()
    {
        currencyInfoCanvas.alpha = 0;
    }

    public void OnStart()
    {
        // Update shard currency
        PlayerPrefs.SetInt("ShardCurrency", shardDisplay.shardCurrency);

        StartCoroutine(StartGame());
    }

    public void OnStatIncrease(string statKey)
    {
        if (shardDisplay.shardCurrency == 0
            || playerState.stats[statKey].statValue >= playerState.stats[statKey].maxValue)
        {
            failAudioSrc.Play();
            return;
        }

        shardDisplay.shardCurrency -= 1;
        shardDisplay.UpdateText();

        PlayerStateObject.Stat newStat = playerState.stats[statKey];
        newStat.statValue = newStat.setStat(); // Upgrades the stat
        playerState.UpdateStat(statKey, newStat);

        UpdateStats();
    }

    public void OnStatDecrease(string statKey)
    {
        if (shardDisplay.shardCurrency == initialShardAmount 
            || playerState.stats[statKey].statValue <= playerState.stats[statKey].minValue)
        {
            failAudioSrc.Play();
            return;
        }

        shardDisplay.shardCurrency += 1;
        shardDisplay.UpdateText();

        PlayerStateObject.Stat newStat = playerState.stats[statKey];
        newStat.statValue = newStat.decreaseStat(); // Upgrades the stat
        playerState.UpdateStat(statKey, newStat);

        UpdateStats();
    }

    public void OnStatReset()
    {
        List<string> statKeys = new List<string>(playerState.stats.Keys);
        foreach (string statKey in statKeys)
        {
            PlayerStateObject.Stat newStat = playerState.stats[statKey];
            newStat.statValue = newStat.minValue;
            playerState.UpdateStat(statKey, newStat);
        }

        shardDisplay.shardCurrency = initialShardAmount;
        shardDisplay.UpdateText();

        UpdateStats();
    }

    private IEnumerator StartGame()
    {
        fade.Play("FadeOut");

        // Wait a bit
        yield return new WaitForSeconds(.55f);

        SceneManager.LoadScene("PreLevel001");
    }
}
