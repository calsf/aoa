using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Weapon", menuName = "Weapon")]
public class WeaponObject : ScriptableObject
{
    public float DAMAGE_BASE;
    public float HEADSHOT_MULTIPLIER_BASE;
    public int MAG_SIZE_BASE;
    public float AIM_TIME_BASE;
    public float INACCURACY_MIN;
    public float INACCURACY_BASE;
    public float ZOOM_BASE;
    public float EFFECTIVE_RANGE_BASE;
    [Range(0, 1)]
    public float FALLOFF_MODIFIER_BASE;
}
