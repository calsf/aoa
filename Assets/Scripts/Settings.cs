using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class Settings : MonoBehaviour
{
    private CanvasGroup settingsScreen;

    [SerializeField] private Slider rSlider;
    [SerializeField] private Slider gSlider;
    [SerializeField] private Slider bSlider;
    private float r;
    private float g;
    private float b;

    [SerializeField] private Slider sensitivitySlider;
    private float sensitivity;

    [SerializeField] private Slider volumeSlider;
    private float volume;

    public UnityEvent OnSettingsSaved;

    void Start()
    {
        ResetValues();

        settingsScreen = GetComponent<CanvasGroup>();
    }

    void Update()
    {
        
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

    // Initializes all setting values to display based on saved values, should be called before Settings screen is opened
    private void ResetValues()
    {
        InitializeCrosshairColor();
        InitializeVolume();
        InitializeSensitivity();
    }
}
