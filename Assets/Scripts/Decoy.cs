using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Decoy : MonoBehaviour
{
    [SerializeField] private ParticleSystem particles;

    private List<GameObject> hitEnemiesObj;
    private List<Enemy> hitEnemies;

    void Awake()
    {
        hitEnemiesObj = new List<GameObject>();
        hitEnemies = new List<Enemy>();
    }

    void LateUpdate()
    {
        if (particles.particleCount <= 0)
        {
            Reset();
        }
    }

    public void Reset()
    {
        hitEnemiesObj.Clear();

        // Reset enemy target and isTaunted state
        foreach (Enemy e in hitEnemies)
        {
            if (e != null && e.gameObject != null)
            {
                e.RemoveTaunt();
            }
        }

        hitEnemies.Clear();

        gameObject.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Enemy") && !hitEnemiesObj.Contains(other.gameObject))
        {
            Enemy enemy = other.GetComponentInParent<Enemy>();
            
            if (enemy.isTaunted) // Ignore if its already taunted
            {
                return;
            }

            hitEnemiesObj.Add(other.gameObject);

            hitEnemies.Add(enemy);
            enemy.isTaunted = true;
            enemy.currTarget = transform;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        OnTriggerEnter(other);
    }
}
