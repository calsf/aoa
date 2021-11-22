using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DefiantReload : MonoBehaviour
{
    private List<GameObject> hitEnemiesObj;
    private List<Enemy> hitEnemies;

    void Awake()
    {
        hitEnemiesObj = new List<GameObject>();
        hitEnemies = new List<Enemy>();
    }

    public void Reset()
    {
        hitEnemiesObj.Clear();

        // Reset enemy movement
        foreach (Enemy e in hitEnemies)
        {
            e.canMove = true;
        }

        hitEnemies.Clear();

        gameObject.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Enemy") && !hitEnemiesObj.Contains(other.gameObject))
        {
            hitEnemiesObj.Add(other.gameObject);

            // Stop their movement
            Enemy enemy = other.GetComponentInParent<Enemy>();
            enemy.canMove = false;
            hitEnemies.Add(enemy);

            // Apply force
            Rigidbody rb = other.GetComponentInParent<Rigidbody>();
            rb.AddForce((rb.transform.position - transform.position).normalized * 50, ForceMode.VelocityChange);
        }
    }
}
