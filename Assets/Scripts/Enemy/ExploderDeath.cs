using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExploderDeath : MonoBehaviour
{
    [SerializeField] private PlayerStateObject playerState;

    private ParticleSystem mainParticles;

    private bool hasHitPlayer;

    public Enemy enemy { get; set; }

    private AudioSource audioSrc;

    void Awake()
    {
        mainParticles = GetComponent<ParticleSystem>();

        audioSrc = GetComponent<AudioSource>();
    }

    private void Start()
    {
        GameObject.FindGameObjectWithTag("SoundManager").GetComponent<SoundManager>().AddAudioSource(audioSrc);
    }

    void OnEnable()
    {
        // Reset has hit player
        hasHitPlayer = false;
    }

    void LateUpdate()
    {
        if (mainParticles.particleCount <= 0)
        {
            gameObject.SetActive(false);
        }
    }

    public void SetSize(float size)
    {
        transform.localScale = new Vector3(size, size, size);

        foreach (Transform child in transform)
        {
            child.localScale = new Vector3(size, size, size);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Hit player only once per activation
        if (other.gameObject.layer == LayerMask.NameToLayer("Player") && !hasHitPlayer)
        {
            hasHitPlayer = true;

            playerState.DamagePlayer(enemy.damageCurr);
        }
    }
}
