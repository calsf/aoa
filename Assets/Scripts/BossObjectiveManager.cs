using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BossObjectiveManager : MonoBehaviour
{
    [SerializeField] private GameObject boss;

    [SerializeField] private PlayerStateObject playerState;

    [SerializeField] private Text objectiveHeaderText;
    [SerializeField] private Text objectiveText;

    [SerializeField] private Exit exit;

    void Start()
    {
        objectiveHeaderText.text = "DAY " + playerState.daysSurvived + " - OBJECTIVE";
    }

    void Update()
    {
        if (boss.activeInHierarchy)
        {
            objectiveText.text = "Eliminate the Swarm";
        }
        else if (exit.hasActivated) // No boss and has activated portal
        {
            float timeRemaining = Mathf.Clamp(exit.exitTime - Time.time, 0, exit.exitTime - Time.time);
            objectiveText.text = "Teleporting in " + timeRemaining.ToString("F0");
        }
        else if (!exit.objectiveComplete) // No boss and has not activated portal
        {
            objectiveText.text = "Find and Activate Portal";
            exit.objectiveComplete = true;
            exit.OpenPortal();
        }
    }
}
