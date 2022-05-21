using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    [SerializeField] private Settings settings;

    private HashSet<AudioSource> audioSources;

    void Awake()
    {
        audioSources = new HashSet<AudioSource>();
    }

    void OnEnable()
    {
        settings.OnSettingsSaved.AddListener(UpdateVolume);
    }

    void OnDisable()
    {
        settings.OnSettingsSaved.RemoveListener(UpdateVolume);
    }

    // Update all audio source audio volume when settings get changed
    private void UpdateVolume()
    {
        foreach (AudioSource audioSrc in audioSources)
        {
            audioSrc.volume = PlayerPrefs.GetFloat("Volume", 1);
        }
    }

    public void AddAudioSource(AudioSource audioSrc)
    {
        if (audioSources == null)
        {
            audioSources = new HashSet<AudioSource>();
        }

        // Initialize audio volume based on settings
        audioSrc.volume = PlayerPrefs.GetFloat("Volume", 1);

        // Add to list of AudioSources for future updates
        audioSources.Add(audioSrc);
    }
}
