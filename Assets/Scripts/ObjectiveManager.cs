using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ObjectiveManager : MonoBehaviour
{
    [SerializeField] private NestManager nest;

    [SerializeField] private Text objectiveText;

    [SerializeField] private Exit exit;

    private List<EnemySpawnManager> enemySpawners;
    private bool hasAggroed = false;

    void Start()
    {
        enemySpawners = new List<EnemySpawnManager>();
        GameObject[] spawnerObjs = GameObject.FindGameObjectsWithTag("EnemySpawnManager");

        foreach (GameObject obj in spawnerObjs)
        {
            EnemySpawnManager enemySpawner = obj.GetComponent<EnemySpawnManager>();
            enemySpawners.Add(enemySpawner);
        }
    }

    void Update()
    {
        if (nest.nestList.Count > 0) // Nests still alive
        {
            objectiveText.text = "Destroy Nests (" + nest.nestList.Count + "/" + nest.startNum +")";
        }
        else if (exit.hasActivated) // No nests and has activated portal
        {
            float timeRemaining = Mathf.Clamp(exit.exitTime - Time.time, 0, exit.exitTime - Time.time);
            objectiveText.text = "Teleporting in " + timeRemaining.ToString("F0");
        }
        else // No nests and has not activated portal
        {
            objectiveText.text = "Find and Activate Portal";
            exit.objectiveComplete = true;
        }

        if (!hasAggroed && nest.nestList.Count == 0)
        {
            hasAggroed = true;

            // Aggro all enemies when all nests are destroyed
            AggroAllEnemies();

            // Change portal color to normal to indicate can activate portal
            exit.OpenPortal();
        }
    }

    private void AggroAllEnemies()
    {
        foreach (EnemySpawnManager enemySpawner in enemySpawners) {

            foreach (GameObject obj in enemySpawner.activeEnemies)
            {
                obj.GetComponent<Enemy>().isAggro = true;
            }
        }
    }
}
