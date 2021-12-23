using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Crosshairs : MonoBehaviour
{
    [SerializeField] private Settings settings;

    [SerializeField] private Image[] crosshairImgs;

    void Start()
    {
        UpdateCrosshairColor();
    }

    void OnEnable()
    {
        settings.OnSettingsSaved.AddListener(UpdateCrosshairColor);
    }

    void OnDisable()
    {
        settings.OnSettingsSaved.RemoveListener(UpdateCrosshairColor);
    }

    // Update crosshair color
    private void UpdateCrosshairColor()
    {
        float r = PlayerPrefs.GetFloat("CrosshairR", 1);
        float g = PlayerPrefs.GetFloat("CrosshairG", 1);
        float b = PlayerPrefs.GetFloat("CrosshairB", 1);

        foreach (Image img in crosshairImgs)
        {
            img.color = new Color(r, g, b, 1);
        }
    }
}
