using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GasCloud : MonoBehaviour
{
    private const float DAMAGE_RATE = .2f;
    private const float DAMAGE = 5;

    [SerializeField] private ParticleSystem particles;

    private DamageNumberManager damageNumberManager;
    private Hitmarker hitmarker;

    private List<GameObject> hitEnemiesObj;

    private float nextReset;

    private AudioSource audioSrc;

    void Awake()
    {
        hitEnemiesObj = new List<GameObject>();
        damageNumberManager = GameObject.FindGameObjectWithTag("DamageNumberManager").GetComponent<DamageNumberManager>();
        hitmarker = GameObject.FindGameObjectWithTag("Hitmarker").GetComponent<Hitmarker>();

        audioSrc = GetComponent<AudioSource>();
        GameObject.FindGameObjectWithTag("SoundManager").GetComponent<SoundManager>().AddAudioSource(audioSrc);
    }

    void Update()
    {
        if (Time.time > nextReset)
        {
            nextReset = Time.time + DAMAGE_RATE;

            // Clear enemies hit
            hitEnemiesObj.Clear();
        }
    }

    void LateUpdate()
    {
        // Destroy after particle duration is over, won't need anymore
        if (particles.isStopped)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Enemy") && !hitEnemiesObj.Contains(other.gameObject))
        {
            Enemy enemy = other.GetComponentInParent<Enemy>();
            enemy.Damaged(DAMAGE);

            damageNumberManager.GetDamageNumberAndDisplay(DAMAGE, enemy.transform.position, false, false, true); // Damage numbers
            hitmarker.OnBodyShot(); // Hitmarker

            hitEnemiesObj.Add(other.gameObject);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        OnTriggerEnter(other);
    }
}
