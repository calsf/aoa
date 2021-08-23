using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Shotgun : Weapon
{
    void Start()
    {
        sizeDeltaModifier = 1600;
    }

    void OnEnable()
    {
        foreach (Image crosshairLine in crosshairLines)
        {
            crosshairLine.enabled = false;
        }

        crosshairCenter.enabled = true;
        crosshairCircle.enabled = true;
    }

    protected override void Shoot()
    {
        anim.Play("Shoot");
        isShooting = true;
        magSizeCurr -= 1;

        // Calculate spread
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

        RaycastHit hit;
        foreach (Vector3 dir in dirs)
        {
            if (Physics.Raycast(cam.transform.position, dir, out hit))
            {
                if (hit.transform.name != "Plane")
                {
                    GameObject obj = Instantiate(placeholder);
                    obj.transform.position = hit.point;
                    Debug.Log(hit.collider.tag);
                }
            }
        }
    }

    // Override aim to change scope circle size
    protected override void Aim()
    {
        if (Input.GetButton("Aim") && !isReloading) // Aim down sights position, field of view, inaccuracy
        {
            player.isAiming = true;
            transform.localPosition = Vector3.Lerp(transform.localPosition, aimPos, Time.deltaTime * aimSpeed);
            cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, camMaxFov - zoom, Time.deltaTime * aimSpeed);
            inaccuracyCurr = Mathf.Lerp(inaccuracyCurr, inaccuracyMin, Time.deltaTime * aimSpeed);

            // Crosshair size
            float size = inaccuracyCurr * (sizeDeltaModifier + 500);
            size = Mathf.Clamp(size, 60, inaccuracyCurr * sizeDeltaModifier);
            crosshairCircleRect.sizeDelta = Vector2.Lerp(crosshairCircleRect.sizeDelta, new Vector2(size, size), Time.deltaTime * aimSpeed);
        }
        else // Hip fire position, field of view, inaccuracy
        {
            player.isAiming = false;
            transform.localPosition = Vector3.Lerp(transform.localPosition, hipPos, Time.deltaTime * aimSpeed);
            cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, camMaxFov, Time.deltaTime * aimSpeed);
            inaccuracyCurr = Mathf.Lerp(inaccuracyCurr, inaccuracyMax, Time.deltaTime * aimSpeed);

            // Crosshair size
            float size = inaccuracyCurr * sizeDeltaModifier;
            size = Mathf.Clamp(size, 60, size);
            crosshairCircleRect.sizeDelta = Vector2.Lerp(crosshairCircleRect.sizeDelta, new Vector2(size, size), Time.deltaTime * aimSpeed);
        }
    }
}
