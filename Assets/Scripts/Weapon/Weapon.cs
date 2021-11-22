using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Weapon : MonoBehaviour
{
    // TEMP!!!!
    [SerializeField] protected GameObject placeholder;

    protected PlayerMoveController playerMoveControl;

    protected Camera cam;

    protected Animator anim;

    // Shooting
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

    protected LayerMask shootLayerMask;
    protected bool isShooting;
    protected bool isReloading;

    public bool isSwapping { get; set; }

    public bool canShoot { get { return !isShooting && !isReloading && magSizeCurr > 0 && !isSwapping; } }
    public bool canReload { get { return magSizeCurr != magSizeMax && !isReloading && !isSwapping; } }

    // Weapon props
    [SerializeField] protected WeaponObject weapon;
    protected float reload; // Anim dependent
    protected float fireRate; // Anim dependent
    protected float damage;
    protected float headshotMultiplier;
    public int magSizeMax { get; set; }
    public int magSizeCurr { get; set; }
    protected float aimTime;
    protected float inaccuracyMin;
    protected float inaccuracyMax;
    protected float inaccuracyCurr;
    protected float zoom;
    protected float effectiveRange;
    protected float falloffModifer;

    // Player state props
    [SerializeField] protected PlayerStateObject playerState;
    [SerializeField] protected DefiantReload defiantReloadEffect;
    protected const float COLD_SHOT_SLOW_MULTIPLIER = .3f;
    protected const float COLD_SHOT_SLOW_TIME = 2.5f;
    protected const float WEAKENING_SHOT_MULTIPLIER = .3f;
    protected const float WEAKENING_SHOT_TIME = 3f;
    protected const float CLONED_SHOT_OFFSET = 1.5f;
    protected const float CLONED_SHOT_DMG_MULTIPLIER = .08f;

    protected virtual void Awake()
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

        playerMoveControl = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerMoveController>();
        cam = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
        anim = GetComponent<Animator>();
        camMaxFov = cam.fieldOfView;
        aimPos = transform.Find("AimPositions/Aim").localPosition;
        hipPos = transform.Find("AimPositions/Hip").localPosition;

        shootLayerMask = new LayerMask();
        shootLayerMask.value = (1 << LayerMask.NameToLayer("Enemy") 
            | 1 << LayerMask.NameToLayer("Ground")
            | 1 << LayerMask.NameToLayer("Wall"));

        // Weapon stats
        reload = playerState.reloadMultiplier;
        fireRate = playerState.fireRateMultiplier;
        anim.SetFloat("ReloadSpeed", reload);
        anim.SetFloat("ShootSpeed", fireRate);

        damage = weapon.DAMAGE_BASE + playerState.damageBonus;
        headshotMultiplier = weapon.HEADSHOT_MULTIPLIER_BASE + playerState.headShotMultiplierBonus;
        magSizeMax = weapon.MAG_SIZE_BASE * playerState.magSizeMaxMultiplier;
        magSizeCurr = magSizeMax;
        aimTime = weapon.AIM_TIME_BASE - playerState.aimTimeReduction <= 0 ? .03f : weapon.AIM_TIME_BASE - playerState.aimTimeReduction; // Have a min aim time
        inaccuracyMin = weapon.INACCURACY_MIN;
        inaccuracyMax = weapon.INACCURACY_BASE - playerState.inaccuracyReduction < inaccuracyMin ? inaccuracyMin : weapon.INACCURACY_BASE - playerState.inaccuracyReduction;
        inaccuracyCurr = inaccuracyMax;
        zoom = weapon.ZOOM_BASE;
        effectiveRange = weapon.EFFECTIVE_RANGE_BASE + playerState.effectiveRangeBonus;
        falloffModifer = weapon.FALLOFF_MODIFIER_BASE;

        defiantReloadEffect.gameObject.SetActive(false);
    }

    void OnEnable()
    {
        foreach (Image crosshairLine in crosshairLines)
        {
            crosshairLine.enabled = true;
        }

        crosshairCenter.enabled = true;
        crosshairCircle.enabled = false;

        playerState.OnStateUpdate.AddListener(UpdateWeaponState);

        UpdateWeaponState();
    }

    void OnDisable()
    {
        playerState.OnStateUpdate.RemoveListener(UpdateWeaponState);
    }

    // Update weapon state
    protected void UpdateWeaponState()
    {
        // Stats
        reload = playerState.reloadMultiplier;
        fireRate = playerState.fireRateMultiplier;
        anim.SetFloat("ReloadSpeed", reload);
        anim.SetFloat("ShootSpeed", fireRate);

        damage = weapon.DAMAGE_BASE + playerState.damageBonus;
        headshotMultiplier = weapon.HEADSHOT_MULTIPLIER_BASE + playerState.headShotMultiplierBonus;
        magSizeMax = weapon.MAG_SIZE_BASE * playerState.magSizeMaxMultiplier;
        aimTime = weapon.AIM_TIME_BASE - playerState.aimTimeReduction <= 0 ? .03f : weapon.AIM_TIME_BASE - playerState.aimTimeReduction; // Have a min aim time
        inaccuracyMax = weapon.INACCURACY_BASE - playerState.inaccuracyReduction < inaccuracyMin ? inaccuracyMin : weapon.INACCURACY_BASE - playerState.inaccuracyReduction;
        effectiveRange = weapon.EFFECTIVE_RANGE_BASE + playerState.effectiveRangeBonus;
    }

    protected void OnFinishShoot()
    {
        isShooting = false;

        // Check for reload after shot
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

    protected void OnFinishSwapIn()
    {
        isSwapping = false;

        // Check for reload on swap in
        if (magSizeCurr <= 0)
        {
            Reload();
        }
    }

    public IEnumerator SwapOutFor(Weapon nextWeapon)
    {
        isSwapping = true;

        anim.Play("SwapOut");

        // Wait for swap out animation to finish
        while (anim.GetCurrentAnimatorStateInfo(0).IsName("SwapOut"))
        {
            yield return null;
        }

        // Swap in next weapon
        nextWeapon.gameObject.SetActive(true);
        nextWeapon.SwapIn();

        // Reset values for this weapon and deactivate
        isSwapping = false;
        isReloading = false;
        isShooting = false;
        gameObject.SetActive(false);
    }

    public void SwapIn()
    {
        isSwapping = true;
        anim.Play("SwapIn");
    }

    public void Reload()
    {
        isReloading = true;
        anim.Play("Reload");

        defiantReloadEffect.Reset();
        DefiantReload();
    }

    public virtual void Aim()
    {
        if (Input.GetButton("Aim") && !isReloading && !isSwapping) // Aim down sights position, field of view, inaccuracy
        {
            playerMoveControl.isAiming = true;
            transform.localPosition = Vector3.SmoothDamp(transform.localPosition, aimPos, ref posVelocity, aimTime);
            cam.fieldOfView = Mathf.SmoothDamp(cam.fieldOfView, camMaxFov - zoom, ref camVelocity, aimTime);
            inaccuracyCurr = Mathf.SmoothDamp(inaccuracyCurr, inaccuracyMin, ref inaccuracyVelocity, aimTime);
        }
        else // Hip fire position, field of view, inaccuracy
        {
            playerMoveControl.isAiming = false;
            transform.localPosition = Vector3.SmoothDamp(transform.localPosition, hipPos, ref posVelocity, aimTime);
            cam.fieldOfView = Mathf.SmoothDamp(cam.fieldOfView, camMaxFov, ref camVelocity, aimTime);
            inaccuracyCurr = Mathf.SmoothDamp(inaccuracyCurr, inaccuracyMax, ref inaccuracyVelocity, aimTime);
        }

        // Crosshair size
        float sizeDelta = inaccuracyCurr * sizeDeltaModifier;
        sizeDelta = Mathf.Clamp(sizeDelta, 60, sizeDelta);
        crosshair.sizeDelta = Vector2.SmoothDamp(crosshair.sizeDelta, new Vector2(sizeDelta, sizeDelta), ref crosshairVelocity, aimTime);
    }

    public virtual void Shoot()
    {
        SacrificialShotLoss();

        anim.Play("Shoot");
        isShooting = true;
        magSizeCurr -= 1;

        // Get shot direction and apply any inaccuracy to the shot
        Vector3 dir = cam.transform.forward;
        dir += Random.Range(-inaccuracyCurr, inaccuracyCurr) * cam.transform.up;
        dir += Random.Range(-inaccuracyCurr, inaccuracyCurr) * cam.transform.right;
        dir.Normalize();

        // Shoot raycast in direction and check hit
        ShootRaycast(dir, cam.transform.position);

        ClonedShot(dir);
    }

    protected void ShootRaycast(Vector3 dir, Vector3 raycastOrigin, float healthGainMultiplier = 2, float damageDealtMultiplier = 1)
    {
        if (!playerState.punchThrough) // No enemy punchthrough
        {
            RaycastHit hit;
            bool hasHit = Physics.Raycast(raycastOrigin, dir, out hit, Mathf.Infinity, shootLayerMask);

            if (hasHit && hit.collider != null)
            {
                // TEMP!!!
                GameObject obj = Instantiate(placeholder);
                obj.transform.position = hit.point;
                //Debug.Log(hit.collider.tag + "DISTANCE " + hit.distance);

                if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Enemy")) // Enemy hit
                {
                    // Check for distance and apply falloff to damage if necessary
                    float damageDealt = hit.distance > effectiveRange ? damage * falloffModifer : damage;

                    // Headshot
                    if (hit.collider.gameObject.tag == "EnemyHead")
                    {
                        damageDealt *= headshotMultiplier;

                        TempoShot(true);
                    }
                    else
                    {
                        TempoShot(false);
                    }

                    // Tempo shot - apply extra damage if applies
                    if (playerState.tempoShot)
                    {
                        damageDealt += playerState.tempoShotExtraDmg;
                    }

                    damageDealt *= damageDealtMultiplier; // Apply damage dealt multiplier
                    hit.collider.gameObject.GetComponentInParent<Enemy>().Damaged(damageDealt);

                    SacrificialShotGain(healthGainMultiplier);
                    ColdShot(hit.collider.gameObject);
                    WeakeningShot(hit.collider.gameObject);
                }
                else if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Wall")) // Wall hit
                {
                    // Check for distance and apply falloff to damage if necessary
                    // Use base damage and base weapon falloff modifier for hits on wall
                    float damageDealt = hit.distance > effectiveRange ? weapon.DAMAGE_BASE * weapon.FALLOFF_MODIFIER_BASE : weapon.DAMAGE_BASE;

                    // Tactical shot - destroy wall with one shot if within effective range
                    if (playerState.tacticalShot && hit.distance < effectiveRange)
                    {
                        damageDealt = 500;
                    }

                    damageDealt *= damageDealtMultiplier; // Apply damage dealt multiplier

                    // Deal damage to wall block
                    hit.collider.gameObject.GetComponent<WallBlock>().Damaged(damageDealt);
                }
                else // Else hit nothing
                {
                    TempoShot(false);
                }
            }
        }
        else // Apply enemy punchthrough, only hits wall if wall is first
        {
            RaycastHit[] allHit = Physics.RaycastAll(raycastOrigin, dir, Mathf.Infinity, shootLayerMask);

            // Sort hit in ascending order
            System.Array.Sort(allHit, (hit1, hit2) => hit1.distance.CompareTo(hit2.distance));

            // Check if first hit was a wall, if so, apply damage and do not apply any further punchthrough
            if (allHit.Length > 0 && allHit[0].collider.gameObject.layer == LayerMask.NameToLayer("Wall"))
            {
                // Check for distance and apply falloff to damage if necessary (Use base damage and base weapon falloff modifier for hits on wall)
                float damageDealt = allHit[0].distance > effectiveRange ? weapon.DAMAGE_BASE * weapon.FALLOFF_MODIFIER_BASE : weapon.DAMAGE_BASE;

                // Tactical shot - destroy wall with one shot if within effective range
                if (playerState.tacticalShot && allHit[0].distance < effectiveRange)
                {
                    damageDealt = 500;
                }

                damageDealt *= damageDealtMultiplier; // Apply damage dealt multiplier

                allHit[0].collider.gameObject.GetComponent<WallBlock>().Damaged(damageDealt);

                // Do not punch through wall
                return;
            }

            // Apply hit on all hit or until wall is hit
            List<GameObject> hitEnemies = new List<GameObject>();
            bool hasHeadshotOnce = false;
            foreach (RaycastHit hit in allHit)
            {
                // TEMP!!!
                GameObject obj = Instantiate(placeholder);
                obj.transform.position = hit.point;
                //Debug.Log(hit.collider.tag + "DISTANCE " + hit.distance);

                if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Enemy")) // Enemy hit
                {
                    // If enemy object was already hit, do not apply hit again
                    // Since all hit is ordered by distance, first hit is the correct hit point and should not apply hit again to same enemy again
                    if (hitEnemies.Contains(hit.collider.gameObject.transform.parent.gameObject))
                    {
                        continue;
                    }

                    // Check for distance and apply falloff to damage if necessary
                    float damageDealt = hit.distance > effectiveRange ? damage * falloffModifer : damage;

                    // Headshot
                    if (hit.collider.gameObject.tag == "EnemyHead")
                    {
                        damageDealt *= headshotMultiplier;

                        TempoShot(true);
                        hasHeadshotOnce = true; // Set headshot as already having hit since punchthrough
                    }
                    else if (!hasHeadshotOnce) // Do not penalize on punchthrough miss
                    {
                        TempoShot(false);
                    }

                    // Tempo shot - apply extra damage if applies
                    if (playerState.tempoShot)
                    {
                        damageDealt += playerState.tempoShotExtraDmg;
                    }

                    damageDealt *= damageDealtMultiplier; // Apply damage dealt multiplier
                    hit.collider.gameObject.GetComponentInParent<Enemy>().Damaged(damageDealt);

                    // Add this enemy object to list of hit objects so it does not get hit again
                    hitEnemies.Add(hit.collider.gameObject.transform.parent.gameObject);

                    SacrificialShotGain(healthGainMultiplier);
                    ColdShot(hit.collider.gameObject);
                    WeakeningShot(hit.collider.gameObject);
                }
                else if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Wall")) // Wall hit
                {
                    // Punchthrough should stop after hitting a wall, do not apply damage
                    return;
                }
                else if (!hasHeadshotOnce) // Hit nothing, but do not penalize on punchthrough miss
                {
                    TempoShot(false);
                }
            }
        }
    }

    protected void SacrificialShotLoss()
    {
        // Sacrificial shot - lose health on shot, gain double the amount lost on enemy hit, cannot fall below 1 health
        // CALL FIRST ON SHOOT SO HEALTH GAIN OCCURS AFTER LOSS
        if (playerState.sacrificialShot)
        {
            // Health to lose based on max health and max mag size of the gun
            float healthToLose = playerState.healthMax / (magSizeMax / 2.5f);

            // Lose health, always stay above 0 health
            playerState.healthCurr = playerState.healthCurr - healthToLose <= 0 ? 1 : playerState.healthCurr - healthToLose;
        }
    }

    protected void SacrificialShotGain(float healthGainMultiplier)
    {
        // Sacrificial shot - gain back health on hit
        if (playerState.sacrificialShot)
        {
            // Gain back double health lost from shooting, healthToLose * 2
            float healthToGain = (playerState.healthMax / (magSizeMax / 2.5f)) * healthGainMultiplier;

            playerState.healthCurr = playerState.healthCurr + healthToGain > playerState.healthMax ? playerState.healthMax : playerState.healthCurr + healthToGain;
        }
    }

    protected void ColdShot(GameObject hit)
    {
        // Slow enemy on hit, resets timer on consecutive hits
        if (playerState.coldShot)
        {
            hit.GetComponentInParent<Enemy>().ApplyColdShot(COLD_SHOT_SLOW_MULTIPLIER, COLD_SHOT_SLOW_TIME);
        }
    }

    protected void WeakeningShot(GameObject hit)
    {
        // Lower enemy damage on hit, resets timer on consecutive hits
        if (playerState.weakeningShot)
        {
            hit.GetComponentInParent<Enemy>().ApplyWeakeningShot(WEAKENING_SHOT_MULTIPLIER, WEAKENING_SHOT_TIME);
        }
    }

    protected void TempoShot(bool isHeadshot)
    {
        // Add extra dmg on consecutive headshots, reset to 0 on miss (should not reset on wall hit)
        if (playerState.tempoShot)
        {
            if (isHeadshot)
            {
                float extraDmg = damage * .2f;
                playerState.tempoShotExtraDmg += extraDmg;
            }
            else
            {
                playerState.tempoShotExtraDmg = 0;
            }
        }
    }

    protected void ClonedShot(Vector3 dir)
    {
        // Shoot additional shots, offset from the cam position and deals reduced damage, DOES NOT APPLY HEALTH GAIN FROM SACRIFICIAL
        if (playerState.clonedShot)
        {
            ShootRaycast(dir, cam.transform.position + Vector3.left * CLONED_SHOT_OFFSET, 0, CLONED_SHOT_DMG_MULTIPLIER);
            ShootRaycast(dir, cam.transform.position + Vector3.right * CLONED_SHOT_OFFSET, 0, CLONED_SHOT_DMG_MULTIPLIER);

            ShootRaycast(dir, cam.transform.position + Vector3.down * CLONED_SHOT_OFFSET, 0, CLONED_SHOT_DMG_MULTIPLIER);
            ShootRaycast(dir, cam.transform.position + Vector3.up * CLONED_SHOT_OFFSET, 0, CLONED_SHOT_DMG_MULTIPLIER);

            ShootRaycast(dir, cam.transform.position + (Vector3.down * CLONED_SHOT_OFFSET + Vector3.left * CLONED_SHOT_OFFSET).normalized, 0, CLONED_SHOT_DMG_MULTIPLIER);
            ShootRaycast(dir, cam.transform.position + (Vector3.up * CLONED_SHOT_OFFSET + Vector3.left * CLONED_SHOT_OFFSET).normalized, 0, CLONED_SHOT_DMG_MULTIPLIER);

            ShootRaycast(dir, cam.transform.position + (Vector3.down * CLONED_SHOT_OFFSET + Vector3.right * CLONED_SHOT_OFFSET).normalized, 0, CLONED_SHOT_DMG_MULTIPLIER);
            ShootRaycast(dir, cam.transform.position + (Vector3.up * CLONED_SHOT_OFFSET + Vector3.right * CLONED_SHOT_OFFSET).normalized, 0, CLONED_SHOT_DMG_MULTIPLIER);
        }
    }

    protected void DefiantReload()
    {
        // Knock back enemies upon reloading
        if (playerState.defiantReload)
        {
            defiantReloadEffect.Reset();
            defiantReloadEffect.gameObject.SetActive(true);
        }
    }
}
