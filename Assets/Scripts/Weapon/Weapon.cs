using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class Weapon : MonoBehaviour
{
    // TEMP!!!!
    [SerializeField] protected GameObject placeholder;

    protected PlayerController player;

    protected Camera cam;

    protected Animator anim;

    protected RectTransform crosshair;
    protected Image[] crosshairLines;
    protected Image crosshairCenter;
    protected Image crosshairCircle;
    protected RectTransform crosshairCircleRect;
    protected float sizeDeltaModifier;

    protected Vector3 aimPos;
    protected Vector3 hipPos;

    protected float camMaxFov;

    protected bool isShooting;
    protected bool isReloading;

    [SerializeField] protected WeaponObject weapon;
    protected float reload; // Anim dependent
    protected float fireRate; // Anim dependent
    protected float damage;
    protected int magSizeMax;
    protected int magSizeCurr;
    protected float aimSpeed;
    protected float inaccuracyMin;
    protected float inaccuracyMax;
    protected float inaccuracyCurr;
    protected float zoom;
    protected float effectiveRange;
    protected float falloffModifer;

    void Awake()
    {
        crosshair = GameObject.FindGameObjectWithTag("Crosshair").GetComponent<RectTransform>();

        GameObject[] lineObj = GameObject.FindGameObjectsWithTag("CrosshairLine");
        crosshairLines = new Image[lineObj.Length];
        for (int i = 0; i < crosshairLines.Length; i++)
        {
            crosshairLines[i] = lineObj[i].GetComponent<Image>();
        }

        crosshairCenter = GameObject.FindGameObjectWithTag("CrosshairCenter").GetComponent<Image>();
        crosshairCircle = GameObject.FindGameObjectWithTag("CrosshairCircle").GetComponent<Image>();
        crosshairCircleRect = crosshairCircle.GetComponent<RectTransform>();

        player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
        cam = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
        anim = GetComponent<Animator>();
        camMaxFov = cam.fieldOfView;
        aimPos = transform.Find("AimPositions/Aim").localPosition;
        hipPos = transform.Find("AimPositions/Hip").localPosition;

        reload = weapon.RELOAD_BASE;
        fireRate = weapon.FIRE_RATE_BASE;
        damage = weapon.DAMAGE_BASE;
        magSizeMax = weapon.MAG_SIZE_BASE;
        magSizeCurr = magSizeMax;
        aimSpeed = weapon.AIM_SPEED_BASE;
        inaccuracyMin = weapon.INACCURACY_MIN;
        inaccuracyMax = weapon.INACCURACY_BASE;
        inaccuracyCurr = inaccuracyMax;
        zoom = weapon.ZOOM_BASE;
        effectiveRange = weapon.EFFECTIVE_RANGE_BASE;
        falloffModifer = weapon.FALLOFF_MODIFIER_BASE;
    }

    void OnEnable()
    {
        foreach (Image crosshairLine in crosshairLines)
        {
            crosshairLine.enabled = true;
        }

        crosshairCenter.enabled = true;
        crosshairCircle.enabled = false;
    }

    void Update()
    {
        if (Input.GetButton("Fire") && !isShooting && !isReloading && magSizeCurr > 0)
        {
            Shoot();
        }

        if (Input.GetButtonDown("Reload") && magSizeCurr != magSizeMax && !isReloading)
        {
            Reload();
        }

        Aim();
    }

    protected void OnFinishShoot()
    {
        isShooting = false;

        if (magSizeCurr <= 0)
        {
            Reload();
        }
    }

    protected void OnFinishReload()
    {
        magSizeCurr = magSizeMax;

        isReloading = false;
        isShooting = false; // Also reset isShooting in case shooting was cancelled by reload
    }

    protected void Reload()
    {
        isReloading = true;
        anim.Play("Reload");
    }

    protected virtual void Shoot()
    {
        anim.Play("Shoot");
        isShooting = true;
        magSizeCurr -= 1;

        // Apply any inaccuracy to shot
        Vector3 dir = cam.transform.forward;
        dir += Random.Range(-inaccuracyCurr, inaccuracyCurr) * cam.transform.up;
        dir += Random.Range(-inaccuracyCurr, inaccuracyCurr) * cam.transform.right;
        dir.Normalize();

        RaycastHit hit;
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

    protected virtual void Aim()
    {
        if (Input.GetButton("Aim") && !isReloading) // Aim down sights position, field of view, inaccuracy
        {
            player.isAiming = true;
            transform.localPosition = Vector3.Lerp(transform.localPosition, aimPos, Time.deltaTime * aimSpeed);
            cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, camMaxFov - zoom, Time.deltaTime * aimSpeed);
            inaccuracyCurr = Mathf.Lerp(inaccuracyCurr, inaccuracyMin, Time.deltaTime * aimSpeed);
        }
        else // Hip fire position, field of view, inaccuracy
        {
            player.isAiming = false;
            transform.localPosition = Vector3.Lerp(transform.localPosition, hipPos, Time.deltaTime * aimSpeed);
            cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, camMaxFov, Time.deltaTime * aimSpeed);
            inaccuracyCurr = Mathf.Lerp(inaccuracyCurr, inaccuracyMax, Time.deltaTime * aimSpeed);
        }

        // Crosshair size
        float sizeDelta = inaccuracyCurr * sizeDeltaModifier;
        sizeDelta = Mathf.Clamp(sizeDelta, 60, sizeDelta);
        crosshair.sizeDelta = Vector2.Lerp(crosshair.sizeDelta, new Vector2(sizeDelta, sizeDelta), Time.deltaTime * aimSpeed);
    }
}
