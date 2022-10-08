using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwarmHealthBar : HealthBar
{
    protected override void Awake()
    {
        settings = GameObject.FindGameObjectWithTag("Settings").GetComponent<Settings>();

        healthFill.localScale = Vector3.one;

        UpdateShowHealthBar();

        // Update to show health bar or not based on settings
        settings.OnSettingsSaved.AddListener(UpdateShowHealthBar);
    }

    protected override void OnDestroy()
    {
        if (settings != null)
        {
            settings.OnSettingsSaved.RemoveListener(UpdateShowHealthBar);
        }
    }

    protected override void LateUpdate()
    {
        // Return if not set
        if (ownerEnemy == null)
        {
            return;
        }

        // --- Statuses ---

        // Cold
        if (ownerEnemy.isColdShotted)
        {
            coldStatus.SetActive(true);
        }
        else
        {
            coldStatus.SetActive(false);
        }

        // Weaken
        if (ownerEnemy.isWeakenShotted)
        {
            weakenedStatus.SetActive(true);
        }
        else
        {
            weakenedStatus.SetActive(false);
        }
    }

    // Update setting to show health bars
    protected override void UpdateShowHealthBar()
    {
        showHealthBars = PlayerPrefs.GetInt("ShowHealthBars", 1) == 1 ? true : false;

        gameObject.SetActive(showHealthBars);
    }

    public override void HealthBarOnHit(float healthRemaining)
    {
        // Do not show if setting is off
        if (!showHealthBars || float.IsNaN(healthRemaining))
        {
            return;
        }

        // Set health scale
        healthFill.localScale = new Vector3(healthRemaining, 1, 1);

        gameObject.SetActive(true);
    }
}
