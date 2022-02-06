using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WeaponDisplay : MonoBehaviour
{
    [SerializeField] private PlayerStateObject playerState;

    [SerializeField] private Text magCurr;
    [SerializeField] private Text magMax;

    [SerializeField] private Image imageSelected;
    [SerializeField] private Image imageUnselected;

    [SerializeField] private RectTransform ammoUnselectedFill;

    private PlayerWeaponController playerWeaponController;
    private Color decoyShotColor = new Color(1, 0, 1, 1);
    private Color swapShotColor = new Color(1, .75f, 0, 1);

    void Start()
    {
        playerWeaponController = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerWeaponController>();
    }

    void Update()
    {
        magCurr.text = playerWeaponController.weaponActive.magSizeCurr.ToString();
        magMax.text = "/ " + playerWeaponController.weaponActive.magSizeMax.ToString();

        CheckDecoyShot();
        UpdateSelectedWeapon();
        UpdateAmmoUnselectedFill();
    }

    // Update selected and unselected images
    private void UpdateSelectedWeapon()
    {
        if (playerWeaponController.weaponActive == playerWeaponController.weaponPrimary)
        {
            imageSelected.sprite = playerWeaponController.weaponPrimary.weapon.ICON;
            imageUnselected.sprite = playerWeaponController.weaponSecondary.weapon.ICON;
        }
        else
        {
            imageSelected.sprite = playerWeaponController.weaponSecondary.weapon.ICON;
            imageUnselected.sprite = playerWeaponController.weaponPrimary.weapon.ICON;
        }
    }

    // Check for decoy shot and change mag curr text color if shot will swap shot/decoy, else default to normal text color
    private void CheckDecoyShot()
    {
        // Swap shot color or default to normal
        if (playerState.powers["SwapShot"].isActive && playerState.bonusSwapDamage > 0)
        {
            magCurr.color = swapShotColor;
        }
        else
        {
            magCurr.color = Color.white;
        }


        // Override any color with decoy shot color if will be decoy
        if (playerState.powers["DecoyShot"].isActive && playerWeaponController.weaponActive.magSizeCurr == 1)
        {
            magCurr.color = decoyShotColor;
        }
    }

    // Update ammo fill bar of unselected weapon
    private void UpdateAmmoUnselectedFill()
    {
        Weapon weapon = playerWeaponController.weaponActive == playerWeaponController.weaponPrimary ? playerWeaponController.weaponSecondary : playerWeaponController.weaponPrimary;

        ammoUnselectedFill.localScale = new Vector3(1, ((float) weapon.magSizeCurr / (float) weapon.magSizeMax), 1);
    }
}
