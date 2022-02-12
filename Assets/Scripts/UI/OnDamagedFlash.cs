using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnDamagedFlash : MonoBehaviour
{
    [SerializeField] private PlayerStateObject playerState;

    private Animator anim;

    void Start()
    {
        anim = GetComponent<Animator>();
    }

    void OnEnable()
    {
        playerState.OnPlayerDamaged.AddListener(DamageFlash);
    }

    void OnDisable()
    {
        playerState.OnPlayerDamaged.RemoveListener(DamageFlash);
    }

    private void DamageFlash()
    {
        anim.Play("OnDamaged");
    }
}
