using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    private const float HEALTH_BASE = 100;
    private const float INVULN_TIME = 1f;

    private float healthMax;
    private float healthCurr;
    private float nextDamagedTime;

    void Start()
    {
        healthMax = HEALTH_BASE;
        healthCurr = healthMax;
    }

    public void DamagePlayer(float damage)
    {
        // Return if player is still invulnerable from previous damaged
        if (Time.time < nextDamagedTime)
        {
            return;
        }

        nextDamagedTime = Time.time + INVULN_TIME;
        healthCurr -= damage;

        Debug.Log("OUCH! Health: " + healthCurr.ToString());
    }
}
