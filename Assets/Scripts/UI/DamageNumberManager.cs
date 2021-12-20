using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageNumberManager : MonoBehaviour
{
    private const int POOL_NUM = 100;

    [SerializeField] GameObject damageNumber;
    protected List<GameObject> damageNumberPool;

    void Start()
    {
        // Initialize damage number pool and set parent of damage numbers to this game object
        damageNumberPool = new List<GameObject>();
        for (int i = 0; i < POOL_NUM; i++)
        {
            GameObject newDmgNumber = Instantiate(damageNumber);
            newDmgNumber.transform.SetParent(transform, false);
            newDmgNumber.SetActive(false);

            damageNumberPool.Add(newDmgNumber);
        }
    }

    public void GetDamageNumberAndDisplay(float damage, Vector3 hitPos, bool isHeadshot, bool isClonedShot, bool isOther = false)
    {
        for (int i = 0; i < damageNumberPool.Count; i++)
        {
            if (!damageNumberPool[i].activeInHierarchy)
            {
                DamageNumber dmgNumber = damageNumberPool[i].GetComponent<DamageNumber>();

                dmgNumber.Display(damage, hitPos, isHeadshot, isClonedShot, isOther);
                return;
            }
        }

        // If no object in the pool is available, create a new object and add to the pool
        GameObject newObj = Instantiate(damageNumber);
        newObj.transform.SetParent(transform, false);
        damageNumberPool.Add(newObj);

        DamageNumber newDmgNumber = newObj.GetComponent<DamageNumber>();
        newDmgNumber.Display(damage, hitPos, isHeadshot, isClonedShot, isOther);
    }
}
