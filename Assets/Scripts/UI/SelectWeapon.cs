using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SelectWeapon : MonoBehaviour
{
    [SerializeField] private Image weaponDisplay;
    [SerializeField] private PlayerStateObject playerState;

    // Must match the proper weapon index
    [SerializeField] private Sprite[] weapons;

    // 0 = update primary weapon, 1 = update secondary weapon
    [Range(0, 1)]
    [SerializeField] private int weaponSlot;

    [SerializeField] private int defaultSelected;

    private int selected;

    void Start()
    {
        selected = defaultSelected;
        UpdateSelected(weaponSlot);
    }

    public void NextWeapon()
    {
        selected++;

        if (selected > weapons.Length - 1)
        {
            selected = 0;
        }

        UpdateSelected(weaponSlot);
    }

    public void PreviousWeapon()
    {
        selected--;

        if (selected < 0)
        {
            selected = weapons.Length - 1;
        }

        UpdateSelected(weaponSlot);
    }

    private void UpdateSelected(int weaponSlot)
    {
        weaponDisplay.sprite = weapons[selected];

        if (weaponSlot == 0)
        {
            playerState.selectedPrimary = selected;
        }
        else if (weaponSlot == 1)
        {
            playerState.selectedSecondary = selected;
        }
    }
}
