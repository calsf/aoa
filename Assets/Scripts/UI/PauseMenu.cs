using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] private CanvasGroup settingsScreen;
    [SerializeField] private CanvasGroup controlsScreen;
    [SerializeField] private UpgradesDisplay upgradesDisplay;
    [SerializeField] private PausedUpgradesDisplay pausedUpgradesDisplay;

    private CanvasGroup pauseScreen;
    private bool isPaused = false;
    private Animator fade;

    public bool canPause { get; set; }

    void Start()
    {
        pauseScreen = GetComponent<CanvasGroup>();

        fade = GameObject.FindGameObjectWithTag("Fade").GetComponent<Animator>();

        // Hide and deactivate screens at start
        pauseScreen.alpha = 0;
        pauseScreen.blocksRaycasts = false;

        settingsScreen.alpha = 0;
        settingsScreen.blocksRaycasts = false;

        controlsScreen.alpha = 0;
        controlsScreen.blocksRaycasts = false;

        canPause = true;
    }

    void Update()
    {
        if (!canPause)
        {
            if (isPaused)
            {
                Unpause();
            }

            return;
        }

        // Pause input
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (!isPaused)
            {
                upgradesDisplay.HideDisplay(); // Hide display first

                Pause();
            }
            else
            {
                Unpause();
            }
        }

        // Hide or show paused version of upgrades display depending on other pause screens
        if (Time.timeScale == 0)
        {
            if (settingsScreen.alpha == 0 && controlsScreen.alpha == 0)
            {
                pausedUpgradesDisplay.ShowDisplay();
            }
            else
            {
                pausedUpgradesDisplay.HideDisplay();
            }
        }
    }

    public void DeathPause()
    {
        Time.timeScale = 0;
    }

    // Pause and show pause screen
    private void Pause()
    {
        isPaused = true;

        Time.timeScale = 0;
        Cursor.lockState = CursorLockMode.None;

        pauseScreen.alpha = 1;
        pauseScreen.blocksRaycasts = true;
    }

    // Unpause, rehide everything
    private void Unpause()
    {
        isPaused = false;

        Time.timeScale = 1;
        Cursor.lockState = CursorLockMode.Locked;

        pauseScreen.alpha = 0;
        pauseScreen.blocksRaycasts = false;

        settingsScreen.alpha = 0;
        settingsScreen.blocksRaycasts = false;

        controlsScreen.alpha = 0;
        controlsScreen.blocksRaycasts = false;
    }

    public void OnResume()
    {
        Unpause();
    }

    public void OnControlsOpen()
    {
        if (controlsScreen.alpha == 1)
        {
            controlsScreen.alpha = 0;
            controlsScreen.blocksRaycasts = false;
        }
        else
        {
            // Close other screen
            settingsScreen.alpha = 0;
            settingsScreen.blocksRaycasts = false;

            // Activate this screen
            controlsScreen.alpha = 1;
            controlsScreen.blocksRaycasts = true;
        }
    }

    public void OnControlsClose()
    {
        controlsScreen.alpha = 0;
        controlsScreen.blocksRaycasts = false;
    }

    public void OnSettings()
    {
        if (settingsScreen.alpha == 1)
        {
            settingsScreen.alpha = 0;
            settingsScreen.blocksRaycasts = false;
        }
        else
        {
            // Close other screen
            controlsScreen.alpha = 0;
            controlsScreen.blocksRaycasts = false;

            // Activate this screen
            settingsScreen.alpha = 1;
            settingsScreen.blocksRaycasts = true;
        }
    }

    public void OnQuit()
    {
        canPause = false;
        isPaused = false;

        StartCoroutine(Quit());
    }

    private IEnumerator Quit()
    {
        fade.Play("FadeOut");
        fade.updateMode = AnimatorUpdateMode.UnscaledTime;

        // Wait a bit
        yield return new WaitForSecondsRealtime(.55f);

        Time.timeScale = 1;
        SceneManager.LoadScene("TitleScene");
    }
}
