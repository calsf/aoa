using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TitleCamera : MonoBehaviour
{
    void FixedUpdate()
    {
        transform.Rotate(Vector3.up * .05f);
    }
}
