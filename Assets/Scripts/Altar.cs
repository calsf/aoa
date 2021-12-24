using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Altar : MonoBehaviour
{
    [SerializeField] protected PlayerStateObject playerState;
    protected const float COST_BASE_SMALL = 0;
    protected const float CONST_BASE_LARGE = 0;

    protected float costCurr;

    protected abstract void OpenAltar();
}
