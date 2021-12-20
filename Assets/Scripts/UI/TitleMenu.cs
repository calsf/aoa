using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleMenu : MonoBehaviour
{
    [SerializeField] private CanvasGroup settingsScreen;
    [SerializeField] private CanvasGroup controlsScreen;

    /*
    void Start()
    {
        // Hide and deactivate screens at start
        settingsScreen.alpha = 0;
        settingsScreen.blocksRaycasts = false;

        controlsScreen.alpha = 0;
        controlsScreen.blocksRaycasts = false;
    }*/

    public void OnPlay()
    {
        SceneManager.LoadScene("Level001");
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
