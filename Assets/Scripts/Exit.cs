using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Exit : MonoBehaviour
{
    private const int OBJECT_SEPARATION = 3;
    private const int PLAYER_SEPARATION = 30;

    [SerializeField] private Text text;

    private Grid3D grid;

    private bool canEnter;
    private bool hasEntered;

    private LayerMask playerMask;
    private LayerMask objectMask;

    void Start()
    {
        grid = GameObject.FindGameObjectWithTag("GridAir").GetComponent<Grid3D>();

        playerMask = new LayerMask();
        playerMask = 1 << LayerMask.NameToLayer("Player");

        objectMask = new LayerMask();
        objectMask = (1 << LayerMask.NameToLayer("Boundary")
            | 1 << LayerMask.NameToLayer("Wall")
            | 1 << LayerMask.NameToLayer("Altar"));

        text.gameObject.SetActive(false);

        // Spawn within grid bounds
        Vector3 spawnPos = Vector3.zero;
        do
        {
            float x = Random.Range(-grid.gridBounds.x, grid.gridBounds.x);
            float y = 0;
            float z = Random.Range(-grid.gridBounds.z, grid.gridBounds.z);

            spawnPos = new Vector3(x, y, z);

        } while (Physics.CheckSphere(spawnPos, OBJECT_SEPARATION, objectMask) || Physics.CheckSphere(spawnPos, PLAYER_SEPARATION, playerMask)); // Keep certain distance between objects and player

        transform.position = spawnPos;
    }

    void Update()
    {
        // Check for interact input
        if (canEnter && !hasEntered && Input.GetButtonDown("Interact"))
        {
            hasEntered = true;
            text.gameObject.SetActive(false);

            // TODO: Go to next level
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player" && !hasEntered)
        {
            canEnter = true;
            text.gameObject.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "Player" && !hasEntered)
        {
            canEnter = false;
            text.gameObject.SetActive(false);
        }
    }
}
