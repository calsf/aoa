using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    protected const int POOL_NUM = 2;

    [SerializeField] protected PlayerStateObject playerState;

    protected Rigidbody rb;
    protected LayerMask collideLayerMask;

    [SerializeField] protected float projectileSpeed;
    [SerializeField] protected GameObject destroyedEffect;
    protected List<GameObject> destroyedEffectPool;

    public Vector3 projectileDir { get; set; }
    public float projectileDamage { get; set; }

    void Awake()
    {
        rb = GetComponent<Rigidbody>();

        collideLayerMask = new LayerMask();
        collideLayerMask.value = (1 << LayerMask.NameToLayer("Player")
            | 1 << LayerMask.NameToLayer("Ground")
            | 1 << LayerMask.NameToLayer("Wall")
            | 1 << LayerMask.NameToLayer("Altar")
            | 1 << LayerMask.NameToLayer("Nest")
            | 1 << LayerMask.NameToLayer("Boundary"));

        destroyedEffectPool = new List<GameObject>();
        for (int i = 0; i < POOL_NUM; i++)
        {
            destroyedEffectPool.Add(Instantiate(destroyedEffect, Vector3.zero, destroyedEffect.transform.rotation));
            destroyedEffectPool[i].SetActive(false);
        }
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
                playerState.DamagePlayer(projectileDamage);
            }

            GameObject destroyedEffectObj = GetFromPool(destroyedEffectPool, destroyedEffect);
            destroyedEffectObj.transform.position = transform.position;
            destroyedEffectObj.SetActive(true);

            gameObject.SetActive(false);
        }
    }

    protected GameObject GetFromPool(List<GameObject> pool, GameObject obj)
    {
        for (int i = 0; i < pool.Count; i++)
        {
            if (!pool[i].activeInHierarchy)
            {
                return pool[i];
            }
        }

        // If no object in the pool is available, create a new object and add to the pool
        GameObject newObj = Instantiate(obj, Vector3.zero, Quaternion.identity);
        pool.Add(newObj);
        return newObj;
    }
}
