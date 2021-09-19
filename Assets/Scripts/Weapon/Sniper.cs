using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sniper : Weapon
{
    [SerializeField] SpriteRenderer scopeOverlay;
    protected float scopeOverlayMaxAlpha = .7f;

    protected float overlayColorVelocity;

    protected override void Awake()
    {
        base.Awake();
        sizeDeltaModifier = 1250;
    }

    // Override aim to also change scope overlay
    public override void Aim()
    {
        if (Input.GetButton("Aim") && !isReloading && !isSwapping) // Aim down sights position, field of view, inaccuracy, scope overlay alpha
        {
            playerMoveControl.isAiming = true;
            transform.localPosition = Vector3.SmoothDamp(transform.localPosition, aimPos, ref posVelocity, aimTime);
            cam.fieldOfView = Mathf.SmoothDamp(cam.fieldOfView, camMaxFov - zoom, ref camVelocity, aimTime);
            inaccuracyCurr = Mathf.SmoothDamp(inaccuracyCurr, inaccuracyMin, ref inaccuracyVelocity, aimTime);
            scopeOverlay.color = new Color(0, 0, 0, Mathf.SmoothDamp(scopeOverlay.color.a, 0, ref overlayColorVelocity, aimTime));
        }
        else // Hip fire position, field of view, inaccuracy, scope overlay alpha
        {
            playerMoveControl.isAiming = false;
            transform.localPosition = Vector3.SmoothDamp(transform.localPosition, hipPos, ref posVelocity, aimTime);
            cam.fieldOfView = Mathf.SmoothDamp(cam.fieldOfView, camMaxFov, ref camVelocity, aimTime);
            inaccuracyCurr = Mathf.SmoothDamp(inaccuracyCurr, inaccuracyMax, ref inaccuracyVelocity, aimTime);
            scopeOverlay.color = new Color(0, 0, 0, Mathf.SmoothDamp(scopeOverlay.color.a, scopeOverlayMaxAlpha, ref overlayColorVelocity, aimTime));
        }

        // Crosshair size
        float sizeDelta = inaccuracyCurr * sizeDeltaModifier;
        sizeDelta = Mathf.Clamp(sizeDelta, 60, sizeDelta);
        crosshair.sizeDelta = Vector2.SmoothDamp(crosshair.sizeDelta, new Vector2(sizeDelta, sizeDelta), ref crosshairVelocity, aimTime);
    }
}