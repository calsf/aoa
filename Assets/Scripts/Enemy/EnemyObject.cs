using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Enemy", menuName = "Enemy")]
public class EnemyObject : ScriptableObject
{
    public float MOVE_SPEED_BASE;
    public float HEALTH_MAX;
    public float DAMAGE_BASE;
}
