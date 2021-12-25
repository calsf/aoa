using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Altar : MonoBehaviour
{
    [SerializeField] protected PlayerStateObject playerState;
    protected const int COST_BASE_SMALL = 0;
    protected const int COST_BASE_LARGE = 0;

    protected int costCurr;

    protected List<string> stats;

    protected virtual void Start()
    {
        stats = new List<string>(playerState.stats.Keys);
    }

    protected abstract void OpenAltar();

    // Upgrades one random player stat
    protected void UpgradeStat()
    {
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
