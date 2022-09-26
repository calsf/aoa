using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateOnly : MonoBehaviour
{
    void FixedUpdate()
    {
        transform.Rotate(0, 5, 0);
    }
}
