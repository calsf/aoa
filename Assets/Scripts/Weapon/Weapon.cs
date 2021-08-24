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

    protected LayerMask shootLayerMask;

    protected RectTransform crosshair;
    protected Image[] crosshairLines;
    protected Image crosshairCenter;
    protected Image crosshairCircle;
    protected RectTransform crosshairCircleRect;
    protected float sizeDeltaModifier;

    protected Vector3 aimPos;
    protected Vector3 hipPos;

    protected Vector3 posVelocity;
    protected Vector2 crosshairVelocity;
    protected float inaccuracyVelocity;
    protected float camVelocity;
    protected float camMaxFov;

    protected bool isShooting;
    protected bool isReloading;

    [SerializeField] protected WeaponObject weapon;
    protected float reload; // Anim dependent
    protected float fireRate; // Anim dependent
    protected float damage;
    protected int magSizeMax;
    protected int magSizeCurr;
    protected float aimTime;
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

        shootLayerMask = new LayerMask();
        shootLayerMask.value = (1 << LayerMask.NameToLayer("Enemy") 
            | 1 << LayerMask.NameToLayer("Ground")
            | 1 << LayerMask.NameToLayer("Wall"));

        reload = weapon.RELOAD_BASE;
        fireRate = weapon.FIRE_RATE_BASE;
        damage = weapon.DAMAGE_BASE;
        magSizeMax = weapon.MAG_SIZE_BASE;
        magSizeCurr = magSizeMax;
        aimTime = weapon.AIM_TIME_BASE;
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
        if (Physics.Raycast(cam.transform.position, dir, out hit, Mathf.Infinity, shootLayerMask))
        {
            if (hit.collider != null)
            {
                // TODO: Apply on hit and account for effective range damage
                GameObject obj = Instantiate(placeholder);
                obj.transform.position = hit.point;
                Debug.Log(hit.collider.tag + "DISTANCE " + hit.distance);

                if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Wall"))
                {
                    WallBlock wallBlock = hit.collider.gameObject.GetComponent<WallBlock>();

                    wallBlock.Damaged(weapon.DAMAGE_BASE);
                }
            }
        }
    }

    protected virtual void Aim()
    {
        if (Input.GetButton("Aim") && !isReloading) // Aim down sights position, field of view, inaccuracy
        {
            player.isAiming = true;
            transform.localPosition = Vector3.SmoothDamp(transform.localPosition, aimPos, ref posVelocity, aimTime);
            cam.fieldOfView = Mathf.SmoothDamp(cam.fieldOfView, camMaxFov - zoom, ref camVelocity, aimTime);
            inaccuracyCurr = Mathf.SmoothDamp(inaccuracyCurr, inaccuracyMin, ref inaccuracyVelocity, aimTime);
        }
        else // Hip fire position, field of view, inaccuracy
        {
            player.isAiming = false;
            transform.localPosition = Vector3.SmoothDamp(transform.localPosition, hipPos, ref posVelocity, aimTime);
            cam.fieldOfView = Mathf.SmoothDamp(cam.fieldOfView, camMaxFov, ref camVelocity, aimTime);
            inaccuracyCurr = Mathf.SmoothDamp(inaccuracyCurr, inaccuracyMax, ref inaccuracyVelocity, aimTime);
        }

        // Crosshair size
        float sizeDelta = inaccuracyCurr * sizeDeltaModifier;
        sizeDelta = Mathf.Clamp(sizeDelta, 60, sizeDelta);
        crosshair.sizeDelta = Vector2.SmoothDamp(crosshair.sizeDelta, new Vector2(sizeDelta, sizeDelta), ref crosshairVelocity, aimTime);
    }
}
