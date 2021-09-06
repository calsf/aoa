using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    private Rigidbody rb;
    private LayerMask collideLayerMask;
    private PlayerHealth playerHealth;

    [SerializeField] protected float projectileSpeed;
    public Vector3 projectileDir { get; set; }
    public float projectileDamage { get; set; }

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        playerHealth = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerHealth>();

        collideLayerMask = new LayerMask();
        collideLayerMask.value = (1 << LayerMask.NameToLayer("Player")
            | 1 << LayerMask.NameToLayer("Ground")
            | 1 << LayerMask.NameToLayer("Wall"));
    }

    void FixedUpdate()
    {
        rb.AddForce((projectileDir * projectileSpeed) - rb.velocity, ForceMode.VelocityChange);
    }

    void OnTriggerEnter(Collider other)
    {
        // If any of the layers match
        if ( ((1 << other.gameObject.layer) & collideLayerMask) != 0)
        {
            // Damage player
            if (other.gameObject.layer == LayerMask.NameToLayer("Player"))
            {
                playerHealth.DamagePlayer(projectileDamage);
            }

            gameObject.SetActive(false);
        }
    }
}
