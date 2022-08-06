using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Explosion : MonoBehaviour
{
    private List<GameObject> hitEnemies; // Keep track of enemies hit by this explosion
    private ParticleSystem mainParticles;
    private DamageNumberManager damageNumberManager;
    private Hitmarker hitmarker;

    public float damage { get; set; }

    private AudioSource audioSrc;

    private bool hasInit = false;

    void Awake()
    {
        audioSrc = GameObject.FindGameObjectWithTag("ExplosionAudioSource").GetComponent<AudioSource>();
        GameObject.FindGameObjectWithTag("SoundManager").GetComponent<SoundManager>().AddAudioSource(audioSrc);

        hitEnemies = new List<GameObject>();
        mainParticles = GetComponent<ParticleSystem>();
        damageNumberManager = GameObject.FindGameObjectWithTag("DamageNumberManager").GetComponent<DamageNumberManager>();
        hitmarker = GameObject.FindGameObjectWithTag("Hitmarker").GetComponent<Hitmarker>();
    }

    void OnEnable()
    {
        hitEnemies.Clear();

        if (hasInit)
        {
            audioSrc.PlayOneShot(audioSrc.clip);
        }
    }

    void OnDisable()
    {
        // Needed for first time it is disabled
        // Set to true to indicate this object has been initialized so that it can play sound next time it is enabled
        hasInit = true;
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
        if ((other.gameObject.layer == LayerMask.NameToLayer("Enemy") || other.gameObject.layer == LayerMask.NameToLayer("Nest")) && !hitEnemies.Contains(other.gameObject))
        {
            hitEnemies.Add(other.gameObject);

            Enemy enemy = other.GetComponentInParent<Enemy>();
            enemy.Damaged(damage);

            damageNumberManager.GetDamageNumberAndDisplay(damage, enemy.transform.position, false, false, true); // Damage numbers
            hitmarker.OnBodyShot(); // Hitmarker
        }
    }
}
