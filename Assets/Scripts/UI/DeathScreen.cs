using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathScreen : MonoBehaviour
{
    [SerializeField] private PlayerStateObject playerState;
    [SerializeField] private PauseMenu pauseMenu;

    private GameObject player;
    private Animator anim;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        anim = GetComponent<Animator>();
    }

    void OnEnable()
    {
        playerState.OnPlayerDamaged.AddListener(DeathCheck);
    }

    void OnDisable()
    {
        playerState.OnPlayerDamaged.RemoveListener(DeathCheck);
    }

    private void DeathCheck()
    {
        if (playerState.healthCurr <= 0)
        {
            pauseMenu.canPause = false;

            player.GetComponent<CharacterController>().enabled = false;
            player.GetComponent<PlayerMoveController>().enabled = false;
            player.GetComponent<PlayerWeaponController>().enabled = false;

            Cursor.lockState = CursorLockMode.None;

            anim.Play("DeathScreenFadeIn");
        }
    }
}
