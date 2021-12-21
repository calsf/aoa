using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpgradesDisplay : MonoBehaviour
{
    [SerializeField] private PlayerStateObject playerState;

    private CanvasGroup upgradesScreen;
    private PlayerMoveController playerMove;

    void Start()
    {
        upgradesScreen = GetComponent<CanvasGroup>();
        playerMove = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerMoveController>();

        upgradesScreen.alpha = 0;
        upgradesScreen.blocksRaycasts = false;
    }

    void Update()
    {
        // Return if game is paused
        if (Time.timeScale == 0)
        {
            return;
        }

        // Hold to display
        if (Input.GetKey(KeyCode.Tab))
        {
            ShowDisplay();
        }
        else
        {
            HideDisplay();
        }
    }

    private void ShowDisplay()
    {
        playerMove.canLook = false;

        Cursor.lockState = CursorLockMode.Confined;
        upgradesScreen.alpha = 1;
        upgradesScreen.blocksRaycasts = true;
    }

    public void HideDisplay()
    {
        playerMove.canLook = true;

        Cursor.lockState = CursorLockMode.Locked;
        upgradesScreen.alpha = 0;
        upgradesScreen.blocksRaycasts = false;
    }
}
