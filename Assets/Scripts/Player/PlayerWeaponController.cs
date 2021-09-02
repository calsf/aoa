using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWeaponController : MonoBehaviour
{
    [SerializeField] private Weapon[] weapons;
    public Weapon weaponActive { get; set; }
    public Weapon weaponPrimary { get; set; }
    public Weapon weaponSecondary { get; set; }

    void Start()
    {
        weaponPrimary = weapons[0];
        weaponSecondary = weapons[1];

        weaponPrimary.gameObject.SetActive(true);
        weaponSecondary.gameObject.SetActive(false);

        weaponActive = weaponPrimary;
    }

    void Update()
    {
        if (Input.GetButtonDown("Primary") && weaponActive != weaponPrimary && !weaponPrimary.isSwapping && !weaponSecondary.isSwapping)
        {
            weaponSecondary.StartCoroutine(weaponSecondary.SwapOutFor(weaponPrimary));
            weaponActive = weaponPrimary;
        }

        if (Input.GetButtonDown("Secondary") && weaponActive != weaponSecondary && !weaponPrimary.isSwapping && !weaponSecondary.isSwapping)
        {
            weaponPrimary.StartCoroutine(weaponPrimary.SwapOutFor(weaponSecondary));
            weaponActive = weaponSecondary;
        }

        if (Input.GetAxis("Mouse ScrollWheel") != 0)
        {
            if (weaponActive == weaponSecondary && !weaponPrimary.isSwapping && !weaponSecondary.isSwapping)
            {
                weaponSecondary.StartCoroutine(weaponSecondary.SwapOutFor(weaponPrimary));
                weaponActive = weaponPrimary;
            }
            else if (weaponActive == weaponPrimary && !weaponPrimary.isSwapping && !weaponSecondary.isSwapping)
            {
                weaponPrimary.StartCoroutine(weaponPrimary.SwapOutFor(weaponSecondary));
                weaponActive = weaponSecondary;
            }
        }

        if (Input.GetButton("Fire") && weaponActive.canShoot)
        {
            weaponActive.Shoot();
        }

        if (Input.GetButtonDown("Reload") && weaponActive.canReload)
        {
            weaponActive.Reload();
        }

        weaponActive.Aim();
    }
}
