using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthBar : MonoBehaviour
{
    private const float ACTIVE_DURATION = 3f;

    private Camera cam;
    private float nextHideTime;

    [SerializeField] protected RectTransform healthFill;

    [SerializeField] protected GameObject coldStatus;
    [SerializeField] protected GameObject weakenedStatus;

    private float startY;
    private Canvas parentCanvas;

    protected Settings settings;
    protected bool showHealthBars;

    public Enemy ownerEnemy { get; set; }

    protected virtual void Awake()
    {
        cam = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
        settings = GameObject.FindGameObjectWithTag("Settings").GetComponent<Settings>();
        parentCanvas = GetComponentInParent<Canvas>();
        parentCanvas.worldCamera = cam;

        startY = parentCanvas.transform.localPosition.y;
        healthFill.localScale = Vector3.one;

        UpdateShowHealthBar();

        gameObject.SetActive(false);

        // Update to show health bar or not based on settings
        settings.OnSettingsSaved.AddListener(UpdateShowHealthBar);
    }

    protected virtual void OnDestroy()
    {
        if (settings != null)
        {
            settings.OnSettingsSaved.RemoveListener(UpdateShowHealthBar);
        }
    }

    protected virtual void LateUpdate()
    {
        // Return if not set
        if (ownerEnemy == null)
        {
            return;
        }

        // Hide after duration
        if (Time.time > nextHideTime)
        {
            gameObject.SetActive(false);
            return;
        }


        float zoomRatio = cam.fieldOfView / 90; // Adjust scale based on zoom to keep consistency
        float dist = Vector3.Distance(parentCanvas.transform.position, cam.transform.position); // Scale based on distance from camera
        parentCanvas.transform.localScale = zoomRatio * (new Vector3(.03f, .03f, .03f) * dist) / 18; // Set size

        // Offset Y position based on scaling to maintain an overhead position
        float offsetY = ((parentCanvas.transform.localScale.y / .03f) - 1f) / 3;
        parentCanvas.transform.localPosition = new Vector3(
            parentCanvas.transform.localPosition.x, 
            (startY + (offsetY > 0 ? offsetY : 0)), 
            parentCanvas.transform.localPosition.z);
        //Debug.Log(offsetY);

        // Look at camera
        parentCanvas.transform.LookAt(parentCanvas.transform.position + cam.transform.rotation * Vector3.back, cam.transform.rotation * Vector3.up);

        // --- Statuses ---

        // Cold
        if (ownerEnemy.isColdShotted)
        {
            coldStatus.SetActive(true);
        }
        else
        {
            coldStatus.SetActive(false);
        }

        // Weaken
        if (ownerEnemy.isWeakenShotted)
        {
            weakenedStatus.SetActive(true);
        }
        else
        {
            weakenedStatus.SetActive(false);
        }
    }

    // Update setting to show health bars
    protected virtual void UpdateShowHealthBar()
    {
        showHealthBars = PlayerPrefs.GetInt("ShowHealthBars", 1) == 1 ? true : false;

        if (!showHealthBars)
        {
            nextHideTime = Time.time;
        }
        else if (Time.time < nextHideTime)
        {
            gameObject.SetActive(true);
        }
    }

    public virtual void HealthBarOnHit(float healthRemaining)
    {
        // Do not show if setting is off
        if (!showHealthBars || float.IsNaN(healthRemaining))
        {
            return;
        }

        // Set health scale
        healthFill.localScale = new Vector3(healthRemaining, 1, 1);

        // Activate and/or refresh active duration
        gameObject.SetActive(true);
        nextHideTime = Time.time + ACTIVE_DURATION;
    }
}
