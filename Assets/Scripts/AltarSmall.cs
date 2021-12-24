using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AltarSmall : Altar
{
    private List<string> stats;
    void Start()
    {
        costCurr = COST_BASE_SMALL;
        stats = new List<string>(playerState.stats.Keys);
    }

    protected override void OpenAltar()
    {
        // Do not open if not enough currency
        if (playerState.bloodCurrency < costCurr)
        {
            return;
        }
        
        string selectedStatKey = stats[Random.Range(0, stats.Count)];


        PlayerStateObject.Stat newStat = playerState.stats[selectedStatKey];
        newStat.statValue = newStat.setStat(); // Upgrades the stat
        playerState.UpdateStat(selectedStatKey, newStat);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            OpenAltar();
            Debug.Log("OPEN!!");
        }

        return;
    }

    private void OnTriggerExit(Collider other)
    {
        return;
    }
}
