using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleMenu : MonoBehaviour
{
    [SerializeField] private CanvasGroup settingsScreen;
    [SerializeField] private CanvasGroup controlsScreen;
    [SerializeField] private Animator playMenuAnim;

    [SerializeField] private PlayerStateObject playerState;

    private void Awake()
    {
        // Initialize player state upon entering title
        playerState.InitializeState();
    }

    void Start()
    {
        // Need to reset time scale in case player quit from pause menu
        Time.timeScale = 1;

        /*
        // Hide and deactivate screens at start
        settingsScreen.alpha = 0;
        settingsScreen.blocksRaycasts = false;

        controlsScreen.alpha = 0;
        controlsScreen.blocksRaycasts = false;
        */
    }

    public void OnPlay()
    {
        // Reset and initialize state before showing the play menu
        playerState.InitializeState();

        playMenuAnim.Play("TransitionIn");
    }

    public void OnControlsOpen()
    {
        controlsScreen.alpha = 1;
        controlsScreen.blocksRaycasts = true;
    }

    public void OnControlsClose()
    {
        controlsScreen.alpha = 0;
        controlsScreen.blocksRaycasts = false;
    }

    public void OnSettings()
    {
        settingsScreen.alpha = 1;
        settingsScreen.blocksRaycasts = true;
    }

    public void OnQuit()
    {
        Application.Quit();
    }
}
