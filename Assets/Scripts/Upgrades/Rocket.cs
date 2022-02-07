using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rocket : MonoBehaviour
{
    private const int POOL_NUM = 3;
    private const int EXPLO_DMG_BASE = 50;

    [SerializeField] private PlayerStateObject playerState;

    [SerializeField] private GameObject rocketExplo;
    protected List<GameObject> rocketExploPool;

    protected Rigidbody rb;
    protected LayerMask collideLayerMask;

    [SerializeField] private float projectileSpeed;
    private Vector3 projectileDir;
    public float projectileDamage { get; set; }

    void Awake()
    {
        rb = GetComponent<Rigidbody>();

        collideLayerMask = new LayerMask();
        collideLayerMask.value = (1 << LayerMask.NameToLayer("Enemy")
            | 1 << LayerMask.NameToLayer("Ground")
            | 1 << LayerMask.NameToLayer("Wall")
            | 1 << LayerMask.NameToLayer("Altar")
            | 1 << LayerMask.NameToLayer("Nest")
            | 1 << LayerMask.NameToLayer("Boundary"));

        // Initialize pool of rocket explo effects
        rocketExploPool = new List<GameObject>();
        for (int i = 0; i < POOL_NUM; i++)
        {
            rocketExploPool.Add(Instantiate(rocketExplo, Vector3.zero, Quaternion.identity));

            rocketExploPool[i].SetActive(false);
        }

        projectileDir = Vector3.down;
    }

    void FixedUpdate()
    {
        rb.AddForce((projectileDir * projectileSpeed) - rb.velocity, ForceMode.VelocityChange);
    }

    void OnTriggerEnter(Collider other)
    {
        // If any of the layers match
        if (((1 << other.gameObject.layer) & collideLayerMask) != 0)
        {
            // Spawn rocket explo
            GameObject exploObj = GetFromPool(rocketExploPool, rocketExplo);

            // Set explo dmg based on flat damage and player bonus dmg
            Explosion explo = exploObj.GetComponent<Explosion>();
            explo.damage = EXPLO_DMG_BASE + playerState.stats["DamageBonus"].statValue;

            exploObj.transform.position = transform.position;
            exploObj.SetActive(true);

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
