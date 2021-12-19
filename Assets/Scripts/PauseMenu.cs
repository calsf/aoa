using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] private CanvasGroup settingsScreen;
    [SerializeField] private CanvasGroup controlsScreen;

    private CanvasGroup pauseScreen;
    private bool isPaused = false;

    void Start()
    {
        pauseScreen = GetComponent<CanvasGroup>();

        // Hide and deactivate screens at start
        pauseScreen.alpha = 0;
        pauseScreen.blocksRaycasts = false;

        settingsScreen.alpha = 0;
        settingsScreen.blocksRaycasts = false;

        controlsScreen.alpha = 0;
        controlsScreen.blocksRaycasts = false;
    }

    void Update()
    {
        // Pause input
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (!isPaused)
            {
                Pause();
            }
            else
            {
                Unpause();
            }
        }
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
        SceneManager.LoadScene("MainMenu");
    }
}
