using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathEffect : MonoBehaviour
{
    private ParticleSystem particles;

    private AudioSource audioSrc;

    private void Awake()
    {
        particles = GetComponent<ParticleSystem>();

        audioSrc = GetComponent<AudioSource>();

        if (audioSrc != null)
        {
            GameObject.FindGameObjectWithTag("SoundManager").GetComponent<SoundManager>().AddAudioSource(audioSrc);
        }
    }

    private void OnEnable()
    {
        particles.Play();
    }

    void LateUpdate()
    {
        if (particles.isPlaying && particles.particleCount <= 0)
        {
            particles.Stop();
            gameObject.SetActive(false);
        }
    }
}
