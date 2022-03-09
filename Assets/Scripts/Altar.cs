using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class Altar : MonoBehaviour
{
    [SerializeField] protected PlayerStateObject playerState;
    protected const int COST_BASE_SMALL = 0;
    protected const int COST_BASE_LARGE = 0;

    [SerializeField] protected Text costText;
    [SerializeField] protected GameObject interactPopUpCanvas;
    [SerializeField] protected GameObject interactPopUp;
    protected Camera cam;

    protected int costCurr;

    protected List<string> stats;

    protected bool canOpen;
    protected bool hasOpened;

    [SerializeField] protected GameObject altarUsedEffect;
    protected GameObject altarUsedEffectObj;

    protected virtual void Start()
    {
        stats = new List<string>(playerState.stats.Keys);
        cam = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
        interactPopUp.SetActive(false);

        altarUsedEffectObj = Instantiate(altarUsedEffect, transform.position, Quaternion.identity);
    }

    void Update()
    {
        // Check for interact input
        if (!hasOpened && canOpen && Input.GetButtonDown("Interact"))
        {
            hasOpened = true;
            interactPopUp.SetActive(false);
            OpenAltar();
        }
    }

    private void LateUpdate()
    {
        float zoomRatio = cam.fieldOfView / 90; // Adjust scale based on zoom to keep consistency
        float dist = Vector3.Distance(interactPopUpCanvas.transform.position, cam.transform.position); // Scale based on distance from camera
        interactPopUpCanvas.transform.localScale = zoomRatio * (new Vector3(.05f, .05f, .05f) * dist) / 12; // Set size

        // Look at camera
        interactPopUpCanvas.transform.LookAt(interactPopUpCanvas.transform.position + cam.transform.rotation * Vector3.back, cam.transform.rotation * Vector3.up);
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

    // Check for when player enters interactable range
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            canOpen = true;

            if (!hasOpened)
            {
                interactPopUp.SetActive(true);
            }
        }
    }

    // Check for when player leaves interactable range
    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Player")
        {
            canOpen = false;

            if (!hasOpened)
            {
                interactPopUp.SetActive(false);
            }
        }
    }
}
