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

    protected float[] pitches = { 1, .9f, 1.1f };
    protected int playedCount = 0;
    [SerializeField] protected AudioSource audioSrcHit;

    void Start()
    {
        // Set up audio
        SoundManager soundManager = GameObject.FindGameObjectWithTag("SoundManager").GetComponent<SoundManager>();
        soundManager.AddAudioSource(audioSrcHit);

        anim = GetComponent<Animator>();

        // Get children hitmarker images
        hitmarkerImgs = new List<Image>();
        foreach (Transform child in transform)
        {
            Image img = child.GetComponent<Image>();
            hitmarkerImgs.Add(img);
        }
    }

    // Play hit audio with current pitch
    public void PlayAudioHit()
    {
        if (playedCount > pitches.Length - 1)
        {
            playedCount = 0;
        }

        audioSrcHit.pitch = pitches[playedCount];
        audioSrcHit.Play();
    }

    public void OnHeadShot()
    {
        foreach (Image img in hitmarkerImgs)
        {
            img.color = headShotColor;
        }

        anim.Play("HitmarkerHitHead");
    }

    public void OnBodyShot(bool playSound = true)
    {
        foreach (Image img in hitmarkerImgs)
        {
            img.color = bodyShotColor;
        }

        anim.Play("HitmarkerHitBody");

        // Play audio (for non weapon hits, playSound should be false when hit using weapon)
        if (playSound)
        {
            PlayAudioHit();
        }
    }
}
