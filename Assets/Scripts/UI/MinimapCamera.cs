using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinimapCamera : MonoBehaviour
{
    [SerializeField] Transform player;

    void LateUpdate()
    {
        transform.position = new Vector3(player.position.x, transform.position.y, player.position.z);

        transform.rotation = Quaternion.Euler(90f, player.eulerAngles.y, 0f);
    }
}
