using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleMenu : MonoBehaviour
{
    [SerializeField] private Animator playMenuAnim;
    [SerializeField] private Animator controlsMenuAnim;
    [SerializeField] private Animator settingsMenuAnim;

    [SerializeField] private PlayerStateObject playerState;

    private CanvasGroup titleMenu;

    private void Awake()
    {
        // Initialize player state upon entering title
        playerState.InitializeState();

        titleMenu = GetComponent<CanvasGroup>();
        titleMenu.blocksRaycasts = true;
    }

    void Start()
    {
        // Need to reset time scale in case player quit from pause menu
        Time.timeScale = 1;

        Cursor.lockState = CursorLockMode.None;
    }

    public void OnPlayOpen()
    {
        // Reset and initialize state before showing the play menu
        playerState.InitializeState();

        playMenuAnim.Play("TransitionIn");
        titleMenu.blocksRaycasts = false;
    }
    public void OnPlayClose()
    {
        playMenuAnim.Play("TransitionOut");
        titleMenu.blocksRaycasts = true;
    }

    public void OnControlsOpen()
    {
        controlsMenuAnim.Play("TransitionIn");
        titleMenu.blocksRaycasts = false;
    }

    public void OnControlsClose()
    {
        controlsMenuAnim.Play("TransitionOut");
        titleMenu.blocksRaycasts = true;
    }

    public void OnSettingsOpen()
    {
        settingsMenuAnim.Play("TransitionIn");
        titleMenu.blocksRaycasts = false;
    }

    public void OnSettingsClose()
    {
        settingsMenuAnim.Play("TransitionOut");
        titleMenu.blocksRaycasts = true;
    }

    public void OnQuit()
    {
        Application.Quit();
    }
}
