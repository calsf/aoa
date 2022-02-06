using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RealtimeUpgradesDisplay : MonoBehaviour
{
    [SerializeField] private PlayerStateObject playerState;

    [SerializeField] private GameObject tempo;
    [SerializeField] private GameObject venge;
    [SerializeField] private GameObject peak;

    private Text tempoText;
    private Text vengeText;
    private Text peakText;

    private PlayerHealthUpgrades playerHealthUpgrades;

    void Start()
    {
        playerHealthUpgrades = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerHealthUpgrades>();

        tempoText = tempo.GetComponentInChildren<Text>();
        vengeText = venge.GetComponentInChildren<Text>();
        peakText = peak.GetComponentInChildren<Text>();

        tempo.SetActive(false);
        venge.SetActive(false);
        peak.SetActive(false);
    }

    void Update()
    {
        if (playerState.powers["TempoShot"].isActive)
        {
            tempo.SetActive(true);
            tempoText.text = playerState.tempoShotExtraDmg.ToString();
        }

        if (playerState.powers["Vengeance"].isActive)
        {
            venge.SetActive(true);
            vengeText.text = playerState.bonusVengeDamage.ToString();
        }

        if (playerState.powers["PeakOfSurvival"].isActive)
        {
            peak.SetActive(true);
            
            if (playerState.peakOfSurvivalActive)
            {
                peakText.text = (playerHealthUpgrades.peakSurvivalOffTime - Time.time).ToString("F1");
                peakText.color = Color.yellow;
            }
            else
            {
                if (Time.time >= playerHealthUpgrades.peakSurvivalNextActive)
                {
                    peakText.text = "READY";
                    peakText.color = Color.green;
                }
                else
                {
                    peakText.text = (playerHealthUpgrades.peakSurvivalNextActive - Time.time).ToString("F1");
                    peakText.color = Color.red;
                }
            }
        }
    }
}
