using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathEffect : MonoBehaviour
{
    private ParticleSystem particles;

    private void Awake()
    {
        particles = GetComponent<ParticleSystem>();
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
