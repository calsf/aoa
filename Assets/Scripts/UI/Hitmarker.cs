using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Hitmarker : MonoBehaviour
{
    private Color headShotColor = Color.red;
    private Color bodyShotColor = Color.yellow;

    private Animator anim;
    private List<Image> hitmarkerImgs;

    void Start()
    {
        anim = GetComponent<Animator>();

        // Get children hitmarker images
        hitmarkerImgs = new List<Image>();
        foreach (Transform child in transform)
        {
            Image img = child.GetComponent<Image>();
            hitmarkerImgs.Add(img);
        }
    }

    public void OnHeadShot()
    {
        foreach (Image img in hitmarkerImgs)
        {
            img.color = headShotColor;
        }

        anim.Play("HitmarkerHitHead");
    }

    public void OnBodyShot()
    {
        foreach (Image img in hitmarkerImgs)
        {
            img.color = bodyShotColor;
        }

        anim.Play("HitmarkerHitBody");
    }
}
