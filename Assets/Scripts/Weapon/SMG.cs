using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SMG : Weapon
{
    protected override void Awake()
    {
        base.Awake();
        sizeDeltaModifier = 3500;
    }
}
