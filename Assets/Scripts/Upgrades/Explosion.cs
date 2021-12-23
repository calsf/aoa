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

    void Awake()
    {
        hitEnemies = new List<GameObject>();
        mainParticles = GetComponent<ParticleSystem>();
        damageNumberManager = GameObject.FindGameObjectWithTag("DamageNumberManager").GetComponent<DamageNumberManager>();
        hitmarker = GameObject.FindGameObjectWithTag("Hitmarker").GetComponent<Hitmarker>();
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
        if (other.gameObject.layer == LayerMask.NameToLayer("Enemy") && !hitEnemies.Contains(other.gameObject))
        {
            hitEnemies.Add(other.gameObject);

            Enemy enemy = other.GetComponentInParent<Enemy>();
            enemy.Damaged(damage);

            damageNumberManager.GetDamageNumberAndDisplay(damage, enemy.transform.position, false, false, true); // Damage numbers
            hitmarker.OnBodyShot(); // Hitmarker
        }
    }
}
