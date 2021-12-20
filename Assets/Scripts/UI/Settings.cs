using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class Settings : MonoBehaviour
{
    private CanvasGroup settingsScreen;

    [SerializeField] private Image[] crosshairPreviewImgs;

    [SerializeField] private Slider rSlider;
    [SerializeField] private Slider gSlider;
    [SerializeField] private Slider bSlider;
    [SerializeField] private Text rValue;
    [SerializeField] private Text gValue;
    [SerializeField] private Text bValue;
    private float r;
    private float g;
    private float b;

    [SerializeField] private Slider sensitivitySlider;
    [SerializeField] private Text sensitivityValue;
    private float sensitivity;

    [SerializeField] private Slider volumeSlider;
    [SerializeField] private Text volumeValue;
    private float volume;

    public UnityEvent OnSettingsSaved;

    void Start()
    {
        ResetValues();

        settingsScreen = GetComponent<CanvasGroup>();
    }

    void Update()
    {
        // Only update while screen is active
        if (settingsScreen.alpha > 0)
        {
            UpdateValuesText();
            UpdateCrosshairPreview();
        }
    }

    // Update the text to match slider values
    private void UpdateValuesText()
    {
        rValue.text = ((int) (rSlider.value * 255)).ToString();
        gValue.text = ((int)(gSlider.value * 255)).ToString();
        bValue.text = ((int)(bSlider.value * 255)).ToString();

        volumeValue.text = ((int)(volumeSlider.value * 100)).ToString();

        sensitivityValue.text = (sensitivitySlider.value * 10).ToString("F1");
    }

    // Update crosshair preview color
    private void UpdateCrosshairPreview()
    {
        float r = rSlider.value;
        float g = gSlider.value ;
        float b = bSlider.value;

        foreach (Image img in crosshairPreviewImgs)
        {
            img.color = new Color(r, g, b, 1);
        }
    }

    private void InitializeCrosshairColor()
    {
        // Get saved values
        r = PlayerPrefs.GetFloat("CrosshairR", 1);
        g = PlayerPrefs.GetFloat("CrosshairG", 1);
        b = PlayerPrefs.GetFloat("CrosshairB", 1);

        // Set sliders based on saved values
        rSlider.value = r;
        gSlider.value = g;
        bSlider.value = b;
    }

    private void InitializeVolume()
    {
        // Get saved value
        volume = PlayerPrefs.GetFloat("Volume", 1);

        // Set slider based on saved value
        volumeSlider.value = volume;
    }

    private void InitializeSensitivity()
    {
        // Get saved value
        sensitivity = PlayerPrefs.GetFloat("Sensitivity", .3f);

        // Set slider based on saved value
        sensitivitySlider.value = sensitivity;
    }

    // Initializes all setting values to display based on saved values, should be called before Settings screen is opened
    private void ResetValues()
    {
        InitializeCrosshairColor();
        InitializeVolume();
        InitializeSensitivity();
    }

    public void OnSave()
    {
        // Save settings based on slider values
        PlayerPrefs.SetFloat("CrosshairR", rSlider.value);
        PlayerPrefs.SetFloat("CrosshairG", gSlider.value);
        PlayerPrefs.SetFloat("CrosshairB", bSlider.value);

        PlayerPrefs.SetFloat("Volume", volumeSlider.value);

        PlayerPrefs.SetFloat("Sensitivity", sensitivitySlider.value);

        // Invoke OnSettingsSaved event
        OnSettingsSaved.Invoke();

        // Close settings after save
        OnCancel();
    }

    // Close settings menu without saving any values
    public void OnCancel()
    {
        settingsScreen.alpha = 0;
        settingsScreen.blocksRaycasts = false;

        // Reset values for next open
        ResetValues();
    }
}
