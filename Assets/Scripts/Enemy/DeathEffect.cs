using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathEffect : MonoBehaviour
{
    ParticleSystem particles;

    private void Start()
    {
        particles = GetComponent<ParticleSystem>();
    }

    void LateUpdate()
    {
        if (particles.particleCount <= 0)
        {
            Destroy(gameObject);
        }
    }
}
