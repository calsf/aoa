using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TEST : MonoBehaviour
{
    [SerializeField] PlayerStateObject playerState;

    [SerializeField] Enemy[] enemies;

    private void Awake()
    {
        playerState.InitializeState();
        //InvokeRepeating("SpawnPeriodically", 3, 4);
    }

    /*
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            playerState.moveSpeedBonus = 21;
            playerState.fireRateMultiplier = 3;
            playerState.reloadMultiplier = 3;
            playerState.inaccuracyReduction = 2;
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

    public void SpawnPeriodically()
    {
        Instantiate(enemies[Random.Range(0, enemies.Length)]);
        Debug.Log("spawned");
    }*/
}
