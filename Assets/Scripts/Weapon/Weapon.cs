using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Weapon : MonoBehaviour
{
    protected const int PROJECTILE_POOL_NUM = 10;

    protected PlayerMoveController playerMoveControl;

    protected Camera cam;

    protected Animator anim;

    // Shooting
    protected RectTransform crosshair;
    protected Image[] crosshairLines;   // Crosshair images include Crosshair preview in Settings
    protected Image[] crosshairCenters;
    protected Image[] crosshairCircles;
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

    protected DamageNumberManager damageNumberManager;
    protected Hitmarker hitmarker;

    public bool isSwapping { get; set; }

    public bool canShoot { get { return !isShooting && !isReloading && magSizeCurr > 0 && !isSwapping; } }
    public bool canReload { get { return magSizeCurr != magSizeMax && !isReloading && !isSwapping; } }

    // Weapon props
    public WeaponObject weapon;
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
    [SerializeField] protected GameObject decoyShotEffect;
    protected const float COLD_SHOT_SLOW_MULTIPLIER = .3f;
    protected const float COLD_SHOT_SLOW_TIME = 2.5f;
    protected const float WEAKENING_SHOT_MULTIPLIER = .3f;
    protected const float WEAKENING_SHOT_TIME = 3f;
    protected const float CLONED_SHOT_OFFSET = 1.5f;
    protected const float CLONED_SHOT_DMG_MULTIPLIER = .08f;

    protected List<GameObject> decoyShotPool;

    protected virtual void Awake()
    {
        crosshair = GameObject.FindGameObjectWithTag("Crosshair").GetComponent<RectTransform>();

        // Get all crosshair lines (including ones for Crosshair Color preview)
        GameObject[] lineObj = GameObject.FindGameObjectsWithTag("CrosshairLine");
        crosshairLines = new Image[lineObj.Length];
        for (int i = 0; i < crosshairLines.Length; i++)
        {
            crosshairLines[i] = lineObj[i].GetComponent<Image>();
        }

        // Get all crosshair centers (including ones for Crosshair Color preview)
        GameObject[] centerObj = GameObject.FindGameObjectsWithTag("CrosshairCenter");
        crosshairCenters = new Image[centerObj.Length];
        for (int i = 0; i < crosshairCenters.Length; i++)
        {
            crosshairCenters[i] = centerObj[i].GetComponent<Image>();
        }

        // Get all crosshair circles (including ones for Crosshair Color preview)
        GameObject[] circleObj = GameObject.FindGameObjectsWithTag("CrosshairCircle");
        crosshairCircles = new Image[circleObj.Length];
        for (int i = 0; i < crosshairCircles.Length; i++)
        {
            crosshairCircles[i] = circleObj[i].GetComponent<Image>();
        }

        // Only change the main in-game crosshair
        crosshairCircleRect = crosshairCircles[0].GetComponent<RectTransform>();
        if (crosshairCircleRect.parent.tag != "Crosshair") // Main one should have parent tag of "Crosshair", the preview for Crosshair Color should have no tag
        {
            crosshairCircles[1].GetComponent<RectTransform>();
        }

        playerMoveControl = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerMoveController>();
        cam = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
        anim = GetComponent<Animator>();
        camMaxFov = cam.fieldOfView;
        aimPos = transform.Find("AimPositions/Aim").localPosition;
        hipPos = transform.Find("AimPositions/Hip").localPosition;

        shootLayerMask = new LayerMask();
        shootLayerMask.value = (1 << LayerMask.NameToLayer("Enemy") 
            | 1 << LayerMask.NameToLayer("Ground")
            | 1 << LayerMask.NameToLayer("Wall")
            | 1 << LayerMask.NameToLayer("Altar")
            | 1 << LayerMask.NameToLayer("Nest"));

        damageNumberManager = GameObject.FindGameObjectWithTag("DamageNumberManager").GetComponent<DamageNumberManager>();
        hitmarker = GameObject.FindGameObjectWithTag("Hitmarker").GetComponent<Hitmarker>();

        // Weapon stats
        reload = playerState.stats["ReloadMultiplier"].statValue;
        fireRate = playerState.stats["FireRateMultiplier"].statValue;
        anim.SetFloat("ReloadSpeed", reload);
        anim.SetFloat("ShootSpeed", fireRate);

        damage = weapon.DAMAGE_BASE + playerState.stats["DamageBonus"].statValue;
        headshotMultiplier = weapon.HEADSHOT_MULTIPLIER_BASE + playerState.stats["HeadShotMultiplierBonus"].statValue;
        magSizeMax = (int) (weapon.MAG_SIZE_BASE * playerState.stats["MagSizeMaxMultiplier"].statValue);
        magSizeCurr = magSizeMax;
        aimTime = weapon.AIM_TIME_BASE - playerState.stats["AimTimeReduction"].statValue <= 0 ? .03f : weapon.AIM_TIME_BASE - playerState.stats["AimTimeReduction"].statValue; // Have a min aim time
        inaccuracyMin = weapon.INACCURACY_MIN;
        inaccuracyMax = weapon.INACCURACY_BASE - playerState.stats["InaccuracyReduction"].statValue < inaccuracyMin ? inaccuracyMin : weapon.INACCURACY_BASE - playerState.stats["InaccuracyReduction"].statValue;
        inaccuracyCurr = inaccuracyMax;
        zoom = weapon.ZOOM_BASE;
        effectiveRange = weapon.EFFECTIVE_RANGE_BASE + playerState.stats["EffectiveRangeBonus"].statValue;
        falloffModifer = weapon.FALLOFF_MODIFIER_BASE;

        defiantReloadEffect.gameObject.SetActive(false);

        // Init decoy shot obj pool
        decoyShotPool = new List<GameObject>();
        for (int i = 0; i < PROJECTILE_POOL_NUM; i++)
        {
            decoyShotPool.Add(Instantiate(decoyShotEffect, Vector3.zero, Quaternion.identity));
            decoyShotPool[i].SetActive(false);
        }
    }

    void OnEnable()
    {
        foreach (Image crosshairLine in crosshairLines)
        {
            crosshairLine.enabled = true;
        }

        foreach (Image center in crosshairCenters)
        {
            center.enabled = true;
        }

        foreach (Image circle in crosshairCircles)
        {
            circle.enabled = false;
        }

        playerState.OnStateUpdate.AddListener(UpdateWeaponState);

        UpdateWeaponState();
    }

    void OnDisable()
    {
        playerState.OnStateUpdate.RemoveListener(UpdateWeaponState);
    }

    // Update weapon state
    protected virtual void UpdateWeaponState()
    {
        // Stats
        reload = playerState.stats["ReloadMultiplier"].statValue;
        fireRate = playerState.stats["FireRateMultiplier"].statValue;
        anim.SetFloat("ReloadSpeed", reload);
        anim.SetFloat("ShootSpeed", fireRate);

        damage = weapon.DAMAGE_BASE + playerState.stats["DamageBonus"].statValue;
        headshotMultiplier = weapon.HEADSHOT_MULTIPLIER_BASE + playerState.stats["HeadShotMultiplierBonus"].statValue;
        magSizeMax = (int) (weapon.MAG_SIZE_BASE * playerState.stats["MagSizeMaxMultiplier"].statValue);
        aimTime = weapon.AIM_TIME_BASE - playerState.stats["AimTimeReduction"].statValue <= 0 ? .03f : weapon.AIM_TIME_BASE - playerState.stats["AimTimeReduction"].statValue; // Have a min aim time
        inaccuracyMax = weapon.INACCURACY_BASE - playerState.stats["InaccuracyReduction"].statValue < inaccuracyMin ? inaccuracyMin : weapon.INACCURACY_BASE - playerState.stats["InaccuracyReduction"].statValue;
        effectiveRange = weapon.EFFECTIVE_RANGE_BASE + playerState.stats["EffectiveRangeBonus"].statValue;
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

        // Cloned shot first so damage numbers show behind
        ClonedShot(dir);

        // Shoot raycast in direction and check hit
        ShootRaycast(dir, cam.transform.position);
    }

    protected void ShootRaycast(Vector3 dir, Vector3 raycastOrigin, float healthGainMultiplier = 2, float damageDealtMultiplier = 1, bool canDecoy = true, bool isClonedShot = false)
    {
        if (!playerState.powers["Punchthrough"].isActive) // No enemy punchthrough
        {
            RaycastHit hit;
            bool hasHit = Physics.Raycast(raycastOrigin, dir, out hit, Mathf.Infinity, shootLayerMask);

            if (hasHit && hit.collider != null)
            {
                //Debug.Log(hit.collider.tag + "DISTANCE " + hit.distance);

                // Decoy shot first hit
                if (canDecoy)
                {
                    DecoyShot(hit.point);
                }

                if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Enemy") || hit.collider.gameObject.layer == LayerMask.NameToLayer("Nest")) // Enemy hit
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
                    if (playerState.powers["TempoShot"].isActive)
                    {
                        damageDealt += playerState.tempoShotExtraDmg;
                    }

                    // Vengeance - apply any stored bonus damage from venge
                    if (playerState.powers["Vengeance"].isActive)
                    {
                        // Add bonus venge damage
                        damageDealt += playerState.bonusVengeDamage;

                        // Reset the bonus venge damage
                        playerState.bonusVengeDamage = 0;
                    }

                    // Swap shot - apply any bonus damage from swapping
                    if (playerState.powers["SwapShot"].isActive)
                    {
                        // Add bonus damage based on percent of damage dealt
                        damageDealt += damageDealt * playerState.bonusSwapDamage;

                        // Reset the bonus swap damage
                        playerState.bonusSwapDamage = 0;
                    }

                    damageDealt *= damageDealtMultiplier; // Apply damage dealt multiplier
                    hit.collider.gameObject.GetComponentInParent<Enemy>().Damaged(damageDealt);

                    SacrificialShotGain(healthGainMultiplier);
                    ColdShot(hit.collider.gameObject);
                    WeakeningShot(hit.collider.gameObject);

                    OnShootRaycastHit(damageDealt, hit.point, hit.collider.gameObject.tag == "EnemyHead", isClonedShot);
                }
                else if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Wall")) // Wall hit
                {
                    // Check for distance and apply falloff to damage if necessary
                    // Use base damage and base weapon falloff modifier for hits on wall
                    float damageDealt = hit.distance > effectiveRange ? weapon.DAMAGE_BASE * weapon.FALLOFF_MODIFIER_BASE : weapon.DAMAGE_BASE;

                    // Tactical shot - destroy wall with one shot if within effective range
                    if (playerState.powers["TacticalShot"].isActive && hit.distance < effectiveRange)
                    {
                        damageDealt = 500;
                    }

                    damageDealt *= damageDealtMultiplier; // Apply damage dealt multiplier

                    // Deal damage to wall block
                    hit.collider.gameObject.GetComponent<WallBlock>().Damaged(damageDealt);

                    OnShootRaycastHit(damageDealt, hit.point, false, isClonedShot);
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

            // Decoy shot first hit
            if (canDecoy && allHit.Length > 0)
            {
                DecoyShot(allHit[0].point);
            }

            // Check if first hit was a wall, if so, apply damage and do not apply any further punchthrough
            if (allHit.Length > 0 && allHit[0].collider.gameObject.layer == LayerMask.NameToLayer("Wall"))
            {
                // Check for distance and apply falloff to damage if necessary (Use base damage and base weapon falloff modifier for hits on wall)
                float damageDealt = allHit[0].distance > effectiveRange ? weapon.DAMAGE_BASE * weapon.FALLOFF_MODIFIER_BASE : weapon.DAMAGE_BASE;

                // Tactical shot - destroy wall with one shot if within effective range
                if (playerState.powers["TacticalShot"].isActive && allHit[0].distance < effectiveRange)
                {
                    damageDealt = 500;
                }

                damageDealt *= damageDealtMultiplier; // Apply damage dealt multiplier

                allHit[0].collider.gameObject.GetComponent<WallBlock>().Damaged(damageDealt);

                OnShootRaycastHit(damageDealt, allHit[0].point, false, isClonedShot);

                // Do not punch through wall
                return;
            }

            // Apply hit on all hit or until wall is hit
            List<GameObject> hitEnemies = new List<GameObject>();
            bool hasHeadshotOnce = false;
            foreach (RaycastHit hit in allHit)
            {
                //Debug.Log(hit.collider.tag + "DISTANCE " + hit.distance);

                if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Enemy") || hit.collider.gameObject.layer == LayerMask.NameToLayer("Nest")) // Enemy hit
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
                    if (playerState.powers["TempoShot"].isActive)
                    {
                        damageDealt += playerState.tempoShotExtraDmg;
                    }

                    // Vengeance - apply any stored bonus damage from venge
                    if (playerState.powers["Vengeance"].isActive)
                    {
                        // Add bonus venge damage
                        damageDealt += playerState.bonusVengeDamage;

                        // Reset the bonus venge damage
                        playerState.bonusVengeDamage = 0;
                    }

                    // Swap shot - apply any bonus damage from swapping
                    if (playerState.powers["SwapShot"].isActive)
                    {
                        // Add bonus damage based on percent of damage dealt
                        damageDealt += damageDealt * playerState.bonusSwapDamage;

                        // Reset the bonus swap damage
                        playerState.bonusSwapDamage = 0;
                    }

                    damageDealt *= damageDealtMultiplier; // Apply damage dealt multiplier
                    hit.collider.gameObject.GetComponentInParent<Enemy>().Damaged(damageDealt);

                    // Add this enemy object to list of hit objects so it does not get hit again
                    hitEnemies.Add(hit.collider.gameObject.transform.parent.gameObject);

                    SacrificialShotGain(healthGainMultiplier);
                    ColdShot(hit.collider.gameObject);
                    WeakeningShot(hit.collider.gameObject);

                    OnShootRaycastHit(damageDealt, allHit[0].point, allHit[0].collider.gameObject.tag == "EnemyHead", isClonedShot);
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

        // Swap shot - MAKE SURE TO RESET EVEN ON MISS
        if (playerState.powers["SwapShot"].isActive)
        {
            // Reset the bonus swap damage
            playerState.bonusSwapDamage = 0;
        }
    }

    // Called when ShootRaycast hits something
    protected virtual void OnShootRaycastHit(float damage, Vector3 hitPos, bool isHeadshot, bool isClonedShot)
    {
        // Damage numbers
        damageNumberManager.GetDamageNumberAndDisplay(damage, hitPos, isHeadshot, isClonedShot);

        // Hitmarkers
        if (isHeadshot)
        {
            hitmarker.OnHeadShot();
        }
        else
        {
            hitmarker.OnBodyShot();
        }
    }

    protected void SacrificialShotLoss()
    {
        // Sacrificial shot - lose health on shot, gain double the amount lost on enemy hit, cannot fall below 1 health
        // CALL FIRST ON SHOOT SO HEALTH GAIN OCCURS AFTER LOSS
        if (playerState.powers["SacrificialShot"].isActive)
        {
            // Health to lose based on health and max mag size of the gun
            float healthToLose = playerState.healthCurr / (magSizeMax / 2f);

            // Lose health, always stay above 0 health
            playerState.healthCurr = playerState.healthCurr - healthToLose <= 0 ? 1 : playerState.healthCurr - healthToLose;
        }
    }

    protected void SacrificialShotGain(float healthGainMultiplier)
    {
        // Sacrificial shot - gain back health on hit
        if (playerState.powers["SacrificialShot"].isActive)
        {
            // Gain back double health lost from shooting, healthToLose * 2
            float healthToGain = (playerState.stats["HealthMax"].statValue / (magSizeMax / 2.5f)) * healthGainMultiplier;

            playerState.healthCurr = playerState.healthCurr + healthToGain > playerState.stats["HealthMax"].statValue ? playerState.stats["HealthMax"].statValue : playerState.healthCurr + healthToGain;
        }
    }

    protected void ColdShot(GameObject hit)
    {
        // Slow enemy on hit, resets timer on consecutive hits
        if (playerState.powers["ColdShot"].isActive && hit.activeInHierarchy)
        {
            hit.GetComponentInParent<Enemy>().ApplyColdShot(COLD_SHOT_SLOW_MULTIPLIER, COLD_SHOT_SLOW_TIME);
        }
    }

    protected void WeakeningShot(GameObject hit)
    {
        // Lower enemy damage on hit, resets timer on consecutive hits
        if (playerState.powers["WeakeningShot"].isActive && hit.activeInHierarchy)
        {
            hit.GetComponentInParent<Enemy>().ApplyWeakeningShot(WEAKENING_SHOT_MULTIPLIER, WEAKENING_SHOT_TIME);
        }
    }

    protected void TempoShot(bool isHeadshot)
    {
        // Add extra dmg on consecutive headshots, reset to 0 on miss (should not reset on wall hit)
        if (playerState.powers["TempoShot"].isActive)
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
        // Shoot additional shots, offset from the cam position and deals reduced damage, DOES NOT APPLY HEALTH GAIN FROM SACRIFICIAL and CANNOT DECOY
        if (playerState.powers["ClonedShot"].isActive)
        {
            ShootRaycast(dir, cam.transform.position + Vector3.left * CLONED_SHOT_OFFSET, 0, CLONED_SHOT_DMG_MULTIPLIER, false, true);
            ShootRaycast(dir, cam.transform.position + Vector3.right * CLONED_SHOT_OFFSET, 0, CLONED_SHOT_DMG_MULTIPLIER, false, true);

            ShootRaycast(dir, cam.transform.position + Vector3.down * CLONED_SHOT_OFFSET, 0, CLONED_SHOT_DMG_MULTIPLIER, false, true);
            ShootRaycast(dir, cam.transform.position + Vector3.up * CLONED_SHOT_OFFSET, 0, CLONED_SHOT_DMG_MULTIPLIER, false, true);

            ShootRaycast(dir, cam.transform.position + (Vector3.down * CLONED_SHOT_OFFSET + Vector3.left * CLONED_SHOT_OFFSET).normalized, 0, CLONED_SHOT_DMG_MULTIPLIER, false, true);
            ShootRaycast(dir, cam.transform.position + (Vector3.up * CLONED_SHOT_OFFSET + Vector3.left * CLONED_SHOT_OFFSET).normalized, 0, CLONED_SHOT_DMG_MULTIPLIER, false, true);

            ShootRaycast(dir, cam.transform.position + (Vector3.down * CLONED_SHOT_OFFSET + Vector3.right * CLONED_SHOT_OFFSET).normalized, 0, CLONED_SHOT_DMG_MULTIPLIER, false, true);
            ShootRaycast(dir, cam.transform.position + (Vector3.up * CLONED_SHOT_OFFSET + Vector3.right * CLONED_SHOT_OFFSET).normalized, 0, CLONED_SHOT_DMG_MULTIPLIER, false, true);
        }
    }

    protected void DefiantReload()
    {
        // Knock back enemies upon reloading
        if (playerState.powers["DefiantReload"].isActive)
        {
            defiantReloadEffect.Reset();
            defiantReloadEffect.gameObject.SetActive(true);
        }
    }

    protected void DecoyShot(Vector3 pos)
    {
        // Spawn decoy at position if last shot of mag
        if (playerState.powers["DecoyShot"].isActive && magSizeCurr == 0)
        {
            GameObject obj = GetFromPool(decoyShotPool, decoyShotEffect);
            obj.transform.position = pos;
            obj.SetActive(true);
        }
    }

    protected GameObject GetFromPool(List<GameObject> pool, GameObject obj)
    {
        for (int i = 0; i < pool.Count; i++)
        {
            if (!pool[i].activeInHierarchy)
            {
                return pool[i];
            }
        }

        // If no object in the pool is available, create a new object and add to the pool
        GameObject newObj = Instantiate(obj, Vector3.zero, Quaternion.identity);
        pool.Add(obj);
        return obj;
    }
}
