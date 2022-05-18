using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Shotgun : Weapon
{
    private float totalDamage;
    private bool hasHeadshot;
    private bool hasDisplayPos;
    private Vector3 displayPos;

    private float[] totalDamageClones;
    private bool[] hasHeadshotClones;
    private bool[] hasDisplayPosClones;
    private Vector3[] displayPosClones;

    private int currCloneIndex;


    protected override void Awake()
    {
        base.Awake();
        sizeDeltaModifier = 1600;

        totalDamageClones = new float[8];
        hasHeadshotClones = new bool[8];
        hasDisplayPosClones = new bool[8];
        displayPosClones = new Vector3[8];

        // Split damage bonus among pellets
        damage = weapon.DAMAGE_BASE + (playerState.stats["DamageBonus"].statValue / 17);
    }

    void OnEnable()
    {
        foreach (Image crosshairLine in crosshairLines)
        {
            crosshairLine.enabled = false;
        }

        foreach (Image center in crosshairCenters)
        {
            center.enabled = true;
        }

        foreach (Image circle in crosshairCircles)
        {
            circle.enabled = true;
        }

        playerState.OnStateUpdate.AddListener(UpdateWeaponState);

        UpdateWeaponState();
    }

    void OnDisable()
    {
        playerState.OnStateUpdate.RemoveListener(UpdateWeaponState);
    }

    protected override void UpdateWeaponState()
    {
        base.UpdateWeaponState();

        // Split damage bonus among pellets
        damage = weapon.DAMAGE_BASE + (playerState.stats["DamageBonus"].statValue / 17);
    }

    // Override aim to change scope circle size
    public override void Aim()
    {
        if (Input.GetButton("Aim") && !isReloading && !isSwapping) // Aim down sights position, field of view, inaccuracy
        {
            playerMoveControl.isAiming = true;
            transform.localPosition = Vector3.SmoothDamp(transform.localPosition, aimPos, ref posVelocity, aimTime);
            cam.fieldOfView = Mathf.SmoothDamp(cam.fieldOfView, camMaxFov - zoom, ref camVelocity, aimTime);
            inaccuracyCurr = Mathf.SmoothDamp(inaccuracyCurr, inaccuracyMin, ref inaccuracyVelocity, aimTime);

            // Crosshair size
            float size = inaccuracyCurr * (sizeDeltaModifier + 500);
            size = Mathf.Clamp(size, 60, inaccuracyCurr * sizeDeltaModifier);
            crosshairCircleRect.sizeDelta = Vector2.SmoothDamp(crosshairCircleRect.sizeDelta, new Vector2(size, size), ref crosshairVelocity, aimTime);
        }
        else // Hip fire position, field of view, inaccuracy
        {
            playerMoveControl.isAiming = false;
            transform.localPosition = Vector3.SmoothDamp(transform.localPosition, hipPos, ref posVelocity, aimTime);
            cam.fieldOfView = Mathf.SmoothDamp(cam.fieldOfView, camMaxFov, ref camVelocity, aimTime);
            inaccuracyCurr = Mathf.SmoothDamp(inaccuracyCurr, inaccuracyMax, ref inaccuracyVelocity, aimTime);

            // Crosshair size
            float size = inaccuracyCurr * sizeDeltaModifier;
            size = Mathf.Clamp(size, 60, size);
            crosshairCircleRect.sizeDelta = Vector2.SmoothDamp(crosshairCircleRect.sizeDelta, new Vector2(size, size), ref crosshairVelocity, aimTime);
        }
    }
    public override void Shoot()
    {
        SacrificialShotLoss();

        anim.Play("Shoot");
        isShooting = true;
        magSizeCurr -= 1;

        // Calculate spread directions
        Vector3[] dirs = new Vector3[17];
        dirs[0] = cam.transform.forward.normalized;
 
        dirs[1] = (cam.transform.forward + ((inaccuracyCurr / 2) * cam.transform.up) + ((inaccuracyCurr / 2) * cam.transform.right)).normalized;
        dirs[2] = (cam.transform.forward + (-(inaccuracyCurr / 2) * cam.transform.up) + ((inaccuracyCurr / 2) * cam.transform.right)).normalized;
        dirs[3] = (cam.transform.forward + ((inaccuracyCurr / 2) * cam.transform.up) + (-(inaccuracyCurr / 2) * cam.transform.right)).normalized;
        dirs[4] = (cam.transform.forward + (-(inaccuracyCurr / 2) * cam.transform.up) + (-(inaccuracyCurr / 2) * cam.transform.right)).normalized;
        dirs[5] = (cam.transform.forward + (-(inaccuracyCurr / 2) * cam.transform.up)).normalized;
        dirs[6] = (cam.transform.forward + ((inaccuracyCurr / 2) * cam.transform.up)).normalized;
        dirs[7] = (cam.transform.forward + (-(inaccuracyCurr / 2) * cam.transform.right)).normalized;
        dirs[8] = (cam.transform.forward + ((inaccuracyCurr / 2) * cam.transform.right)).normalized;

        dirs[9] = (cam.transform.forward + (inaccuracyCurr * cam.transform.up) + (inaccuracyCurr * cam.transform.right)).normalized;
        dirs[10] = (cam.transform.forward + (-inaccuracyCurr * cam.transform.up) + (inaccuracyCurr * cam.transform.right)).normalized;
        dirs[11] = (cam.transform.forward + (inaccuracyCurr * cam.transform.up) + (-inaccuracyCurr * cam.transform.right)).normalized;
        dirs[12] = (cam.transform.forward + ((-inaccuracyCurr * cam.transform.up) + (-inaccuracyCurr * cam.transform.right))).normalized;
        dirs[13] = (cam.transform.forward + (-inaccuracyCurr * cam.transform.up)).normalized;
        dirs[14] = (cam.transform.forward + (inaccuracyCurr * cam.transform.up)).normalized;
        dirs[15] = (cam.transform.forward + (-inaccuracyCurr * cam.transform.right)).normalized;
        dirs[16] = (cam.transform.forward + (inaccuracyCurr * cam.transform.right)).normalized;


        // Reset values before shoot raycasts
        totalDamage = 0;
        hasHeadshot = false;
        hasDisplayPos = false;

        // Reset values for cloned shotgun shots
        for (int i = 0; i < totalDamageClones.Length; i++)
        {
            totalDamageClones[i] = 0;
            hasHeadshotClones[i] = false;
            hasDisplayPosClones[i] = false;
        }

        // Shoot raycasts in direction and check hit
        ClonedShotShotgun(dirs[0]);
        ShootRaycast(dirs[0], cam.transform.position, (2.0f / 17.0f)); // Only middle shot should apply any special effects e.g decoy shot
        for (int i = 1; i < dirs.Length; i++)
        {
            ClonedShotShotgun(dirs[i]);
            ShootRaycast(dirs[i], cam.transform.position, (2.0f / 17.0f ), 1, false); // Each 'pellet' should have its own separate sacrificial shot health gain
        }

        OnShotgunHit();
    }

    // Shotgun specific version of Weapon.cs ClonedShot
    protected void ClonedShotShotgun(Vector3 dir)
    {
        // Shoot additional shots, offset from the cam position and deals reduced damage, DOES NOT APPLY HEALTH GAIN FROM SACRIFICIAL and CANNOT DECOY
        if (playerState.powers["ClonedShot"].isActive)
        {
            ShootRaycastCloned(dir, cam.transform.position + Vector3.left * CLONED_SHOT_OFFSET, 0);

            ShootRaycastCloned(dir, cam.transform.position + Vector3.right * CLONED_SHOT_OFFSET, 1);

            ShootRaycastCloned(dir, cam.transform.position + Vector3.down * CLONED_SHOT_OFFSET, 2);

            ShootRaycastCloned(dir, cam.transform.position + Vector3.up * CLONED_SHOT_OFFSET, 3);

            ShootRaycastCloned(dir, cam.transform.position + (Vector3.down * CLONED_SHOT_OFFSET + Vector3.left * CLONED_SHOT_OFFSET).normalized, 4);

            ShootRaycastCloned(dir, cam.transform.position + (Vector3.up * CLONED_SHOT_OFFSET + Vector3.left * CLONED_SHOT_OFFSET).normalized, 5);

            ShootRaycastCloned(dir, cam.transform.position + (Vector3.down * CLONED_SHOT_OFFSET + Vector3.right * CLONED_SHOT_OFFSET).normalized, 6);

            ShootRaycastCloned(dir, cam.transform.position + (Vector3.up * CLONED_SHOT_OFFSET + Vector3.right * CLONED_SHOT_OFFSET).normalized, 7);
        }
    }

    // ShootRaycast as cloned shot in given direction and with given offset, also sets clone index to use for accumulating damage
    protected void ShootRaycastCloned(Vector3 dir, Vector3 raycastOrigin, int cloneIndex)
    {
        // Shoot raycast in direction and check hit
        ShootRaycast(dir, raycastOrigin, 0, CLONED_SHOT_DMG_MULTIPLIER, false, true);

        // Set curr clone index so OnShootRaycastHit uses separate set of values for each group of cloned shot raycasts
        currCloneIndex = cloneIndex;
    }

    // Override to accumulate damage from all ShootRaycast and display one number at first hit position
    protected override void OnShootRaycastHit(float damage, Vector3 hitPos, bool isHeadshot, bool isClonedShot)
    {
        if (isClonedShot) // Handle cloned shots using the array of clone values
        {
            totalDamageClones[currCloneIndex] += damage;

            // If has headshot at least once, set to true
            if (!hasHeadshotClones[currCloneIndex] && isHeadshot)
            {
                hasHeadshotClones[currCloneIndex] = true;
            }

            // Only set display position once, which will be set to first hit position
            if (!hasDisplayPosClones[currCloneIndex])
            {
                hasDisplayPosClones[currCloneIndex] = true;
                displayPosClones[currCloneIndex] = hitPos;
            }
        }
        else // Original shots
        {
            totalDamage += damage;

            // If has headshot at least once, set to true
            if (!hasHeadshot && isHeadshot)
            {
                hasHeadshot = true;
            }

            // Only set display position once, which will be set to first hit position
            if (!hasDisplayPos)
            {
                hasDisplayPos = true;
                displayPos = hitPos;
            }
        }
    }

    protected void OnShotgunHit()
    {
        bool hasHitAny = false;

        // Display for cloned shotgun shots first so original shot numbers appear first
        for (int i = 0; i < totalDamageClones.Length; i++)
        {
            if (totalDamageClones[i] != 0 && hasDisplayPosClones[i])
            {
                // Display accumulated damage from all 'pellets'
                damageNumberManager.GetDamageNumberAndDisplay(totalDamageClones[i], displayPosClones[i], hasHeadshotClones[i], true);

                hasHitAny = true;
            }
        }

        // Only display if did damage and obtained hit position (0 damage or no hit position was obtained, means every 'pellet' missed)
        if (totalDamage != 0 && hasDisplayPos)
        {
            // Display accumulated damage from all 'pellets'
            damageNumberManager.GetDamageNumberAndDisplay(totalDamage, displayPos, hasHeadshot, false);

            hasHitAny = true;
        }

        // Only proceed if landed a hit at all
        if (hasHitAny)
        {
            // Hitmarkers (If ANY shot was a headshot, hitmarker should show as headshot)
            bool hasHeadShotAny = hasHeadshot;

            // Check if cloned shots were headshot at all
            if (!hasHeadShotAny)
            {
                foreach (bool cloneHeadshot in hasHeadshotClones)
                {
                    if (cloneHeadshot)
                    {
                        hasHeadShotAny = true;
                        break;
                    }
                }
            }

            if (hasHeadShotAny)
            {
                hitmarker.OnHeadShot();
            }
            else
            {
                hitmarker.OnBodyShot();
            }
        }
    }
}
