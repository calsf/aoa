using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileWithGravity : Projectile
{
    private void OnEnable()
    {
        rb.AddForce(projectileDir * projectileSpeed, ForceMode.VelocityChange);
    }

    private void OnDisable()
    {
        rb.velocity = Vector3.zero;
    }

    void FixedUpdate()
    {
        return;
    }
}
