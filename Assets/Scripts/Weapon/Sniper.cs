using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sniper : Weapon
{
    [SerializeField] SpriteRenderer scopeOverlay;
    protected float scopeOverlayMaxAlpha = .7f;

    void Start()
    {
        sizeDeltaModifier = 1250;
    }

    // Override aim to also change scope overlay
    protected override void Aim()
    {
        if (Input.GetButton("Aim") && !isReloading) // Aim down sights position, field of view, inaccuracy, scope overlay alpha
        {
            player.isAiming = true;
            transform.localPosition = Vector3.Lerp(transform.localPosition, aimPos, Time.deltaTime * aimSpeed);
            cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, camMaxFov - zoom, Time.deltaTime * aimSpeed);
            inaccuracyCurr = Mathf.Lerp(inaccuracyCurr, inaccuracyMin, Time.deltaTime * aimSpeed);
            scopeOverlay.color = new Color(0, 0, 0, Mathf.Lerp(scopeOverlay.color.a, 0, Time.deltaTime * aimSpeed * 4));
        }
        else // Hip fire position, field of view, inaccuracy, scope overlay alpha
        {
            player.isAiming = false;
            transform.localPosition = Vector3.Lerp(transform.localPosition, hipPos, Time.deltaTime * aimSpeed);
            cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, camMaxFov, Time.deltaTime * aimSpeed);
            inaccuracyCurr = Mathf.Lerp(inaccuracyCurr, inaccuracyMax, Time.deltaTime * aimSpeed);
            scopeOverlay.color = new Color(0, 0, 0, Mathf.Lerp(scopeOverlay.color.a, scopeOverlayMaxAlpha, Time.deltaTime * aimSpeed));
        }

        // Crosshair size
        float sizeDelta = inaccuracyCurr * sizeDeltaModifier;
        sizeDelta = Mathf.Clamp(sizeDelta, 60, sizeDelta);
        crosshair.sizeDelta = Vector2.Lerp(crosshair.sizeDelta, new Vector2(sizeDelta, sizeDelta), Time.deltaTime * aimSpeed);
    }
}