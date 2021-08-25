using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    private const int HEALTH_BASE = 100;
    private const float INVULN_TIME = 1f;

    private int healthMax;
    private int healthCurr;
    private float nextDamagedTime;

    void Start()
    {
        healthMax = HEALTH_BASE;
        healthCurr = healthMax;
    }

    public void DamagePlayer(int damage)
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
