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

    private PlayerWeaponController playerWeaponController;
    private Color decoyShotColor = new Color(1, 0, 1, 1);

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

    // Check for decoy shot and change mag curr text color if shot will be decoy, else default to normal text color
    private void CheckDecoyShot()
    {
        if (playerState.powers["DecoyShot"].isActive && playerWeaponController.weaponActive.magSizeCurr == 1)
        {
            magCurr.color = decoyShotColor;
        }
        else
        {
            magCurr.color = Color.white;
        }
    }
}
