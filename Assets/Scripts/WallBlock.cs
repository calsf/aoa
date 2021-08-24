using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallBlock : MonoBehaviour
{
    private Renderer renderer;

    private float healthMax = 500f;
    private float healthCurr = 500f;

    // Start is called before the first frame update
    void Start()
    {
        renderer = GetComponent<Renderer>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Damaged(float dmg)
    {
        healthCurr -= dmg;

        if (healthCurr <= 0)
        {
            Destroy(gameObject);
        }
        else
        {
            float blend1 = (healthMax - healthCurr) / 200;
            float blend2 = healthCurr < healthMax / 1.5f ? blend1 / 1.5f : 0;
            float blend3 = healthCurr < healthMax / 3 ? blend1 / 2 : 0;
            blend2 = Mathf.Clamp(blend2, 0, 1);
            blend3 = Mathf.Clamp(blend3, 0, 1);
            renderer.material.SetFloat("_Blend1", blend1);
            renderer.material.SetFloat("_Blend2", blend2);
            renderer.material.SetFloat("_Blend3", blend3);
        }
    }
}
