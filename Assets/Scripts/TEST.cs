using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TEST : MonoBehaviour
{
    [SerializeField] PlayerStateObject playerState;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            playerState.moveSpeedBonus = 21;
            playerState.OnStateUpdate.Invoke();
            Debug.Log("upgrade");
        }

        if (Input.GetKeyDown(KeyCode.N))
        {
            SceneManager.LoadScene("TESTSCENE");
        }
    }

    public void OnPlay()
    {
        playerState.InitializeState();
        SceneManager.LoadScene("Level001");
    }
}
