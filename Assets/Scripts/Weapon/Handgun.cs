using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Handgun : Weapon
{
    protected override void Awake()
    {
        base.Awake();
        sizeDeltaModifier = 4500;
    }
}
