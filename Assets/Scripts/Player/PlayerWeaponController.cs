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
    }

    void Update()
    {
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
    }
}
