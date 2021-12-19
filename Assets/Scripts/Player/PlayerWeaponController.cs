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
        if (playerState.holsteredReload)
        {
            Weapon weaponInactive = weaponActive == weaponPrimary ? weaponSecondary : weaponPrimary;

            if (Time.time > holsteredReloadNextTime)
            {
                // Increase holstered reload time with the player's current reload multiplier
                holsteredReloadNextTime = Time.time + (HOLSTERED_RELOAD_DELAY / playerState.reloadMultiplier);

                // Reload percentage of weapon mag, min of 1 ammo reloaded
                weaponInactive.magSizeCurr += (int)(weaponInactive.magSizeMax * HOLSTERED_RELOAD_PERCENT) <= 0 ? 1 : (int)(weaponInactive.magSizeMax * HOLSTERED_RELOAD_PERCENT);

                // Check for mag overflow
                if (weaponInactive.magSizeCurr > weaponInactive.magSizeMax)
                {
                    weaponInactive.magSizeCurr = weaponInactive.magSizeMax;
                }
            }
        }
    }
}
