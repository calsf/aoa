using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWeaponController : MonoBehaviour
{
    [SerializeField] private PlayerStateObject playerState;

    [SerializeField] private Weapon[] weapons;
    public Weapon weaponActive { get; set; }
    public Weapon weaponPrimary { get; set; }
    public Weapon weaponSecondary { get; set; }

    private float holsteredReloadNextTime;
    private const float HOLSTERED_RELOAD_DELAY = 2f; // Default delay
    private const float HOLSTERED_RELOAD_PERCENT = .25f; // Percentage of mag max to reload each iteration

    private Camera cam;
    private LayerMask hoverLayerMask;

    void Start()
    {
        weaponPrimary = weapons[playerState.selectedPrimary];
        weaponSecondary = weapons[playerState.selectedSecondary];

        // Make sure to activate each weapon at start to avoid Aim issues when swapping
        weaponPrimary.gameObject.SetActive(true);
        weaponSecondary.gameObject.SetActive(true);
        weaponPrimary.gameObject.SetActive(false);
        weaponSecondary.gameObject.SetActive(false);

        // Set active weapon
        weaponActive = playerState.selectedActive == 0 ? weaponPrimary : weaponSecondary;
        weaponActive.gameObject.SetActive(true);

        holsteredReloadNextTime = Time.time;

        cam = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();

        // Must also include anything that can obstruct enemy view
        hoverLayerMask = new LayerMask();
        hoverLayerMask.value = (1 << LayerMask.NameToLayer("Enemy")
            | 1 << LayerMask.NameToLayer("Nest")
            | 1 << LayerMask.NameToLayer("Altar")
            | 1 << LayerMask.NameToLayer("Ground")
            | 1 << LayerMask.NameToLayer("Wall"));
    }

    void Update()
    {
        // Do not run if timescale is 0
        if (Time.timeScale == 0)
        {
            return;
        }

        // Swap to primary
        if (Input.GetButtonDown("Primary") && weaponActive != weaponPrimary && !weaponPrimary.isSwapping && !weaponSecondary.isSwapping)
        {
            weaponSecondary.StartCoroutine(weaponSecondary.SwapOutFor(weaponPrimary));
            weaponActive = weaponPrimary;
            playerState.selectedActive = 0;
        }

        // Swap to secondary
        if (Input.GetButtonDown("Secondary") && weaponActive != weaponSecondary && !weaponPrimary.isSwapping && !weaponSecondary.isSwapping)
        {
            weaponPrimary.StartCoroutine(weaponPrimary.SwapOutFor(weaponSecondary));
            weaponActive = weaponSecondary;
            playerState.selectedActive = 1;
        }

        // Swap to inactive weapon
        if (Input.GetAxis("Mouse ScrollWheel") != 0)
        {
            if (weaponActive == weaponSecondary && !weaponPrimary.isSwapping && !weaponSecondary.isSwapping)
            {
                weaponSecondary.StartCoroutine(weaponSecondary.SwapOutFor(weaponPrimary));
                weaponActive = weaponPrimary;
                playerState.selectedActive = 0;
            }
            else if (weaponActive == weaponPrimary && !weaponPrimary.isSwapping && !weaponSecondary.isSwapping)
            {
                weaponPrimary.StartCoroutine(weaponPrimary.SwapOutFor(weaponSecondary));
                weaponActive = weaponSecondary;
                playerState.selectedActive = 1;
            }
        }

        // Shoot
        if (Input.GetButton("Fire") && weaponActive.canShoot)
        {
            weaponActive.Shoot();
        }


        // Reload
        if (Input.GetButtonDown("Reload") && weaponActive.canReload)
        {
            weaponActive.Reload();
        }

        // Aim
        weaponActive.Aim();

        // Holstered Reload - reload the inactive weapon
        if (playerState.powers["HolsteredReload"].isActive)
        {
            Weapon weaponInactive = weaponActive == weaponPrimary ? weaponSecondary : weaponPrimary;

            if (Time.time > holsteredReloadNextTime)
            {
                // Increase holstered reload time with the player's current reload multiplier
                holsteredReloadNextTime = Time.time + (HOLSTERED_RELOAD_DELAY / playerState.stats["ReloadMultiplier"].statValue);

                // Reload percentage of weapon mag, min of 1 ammo reloaded
                weaponInactive.magSizeCurr += (int)(weaponInactive.magSizeMax * HOLSTERED_RELOAD_PERCENT) <= 0 ? 1 : (int)(weaponInactive.magSizeMax * HOLSTERED_RELOAD_PERCENT);

                // Check for mag overflow
                if (weaponInactive.magSizeCurr > weaponInactive.magSizeMax)
                {
                    weaponInactive.magSizeCurr = weaponInactive.magSizeMax;
                }
            }
        }

        // Trigger health bars on hover and if in view of player's mouse target
        RaycastHit hit;
        bool hasHit = Physics.Raycast(cam.transform.position, cam.transform.forward, out hit, Mathf.Infinity, hoverLayerMask);
        if (hasHit && hit.collider != null && (hit.collider.gameObject.layer == LayerMask.NameToLayer("Enemy") || hit.collider.gameObject.layer == LayerMask.NameToLayer("Nest")))
        {
            hit.collider.gameObject.GetComponentInParent<Enemy>().TriggerHealthBar();
        }
    }
}
