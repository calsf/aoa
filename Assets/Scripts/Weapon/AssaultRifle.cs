using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AssaultRifle : Weapon
{
    protected override void Awake()
    {
        base.Awake();
        sizeDeltaModifier = 2500;
    }
}

