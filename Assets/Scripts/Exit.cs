using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Exit : MonoBehaviour
{
    private const float DELAY_TIME = 30; // Time before actually exiting
    private const float TEXT_DELAY = 3;

    private const int OBJECT_SEPARATION = 3;
    private const int PLAYER_SEPARATION = 30;

    [SerializeField] private Text text;
    [SerializeField] private ParticleSystem rings;
    [SerializeField] private ParticleSystem groundEffect;

    private Grid3D grid;

    private bool canActivate;
    public bool hasActivated { get; set; }

    private LayerMask playerMask;
    private LayerMask objectMask;

    public bool objectiveComplete { get; set; }
    public float exitTime { get; set; }
    private float textTime;

    [SerializeField] private string nextScene;
    [SerializeField] private GameObject firstPersonCamera;
    private GameObject player;
    private bool isExiting;

    [SerializeField] protected AudioSource audioSrcActivate;
    private AudioSource audioSrc;

    [SerializeField] protected AudioClip portalActivate;

    void Start()
    {
        // Set up audio
        audioSrc = GetComponent<AudioSource>();

        SoundManager soundManager = GameObject.FindGameObjectWithTag("SoundManager").GetComponent<SoundManager>();
        soundManager.AddAudioSource(audioSrcActivate);
        soundManager.AddAudioSource(audioSrc);

        grid = GameObject.FindGameObjectWithTag("GridAir").GetComponent<Grid3D>();

        player = GameObject.FindGameObjectWithTag("Player");

        playerMask = new LayerMask();
        playerMask = 1 << LayerMask.NameToLayer("Player");

        objectMask = new LayerMask();
        objectMask = (1 << LayerMask.NameToLayer("Boundary")
            | 1 << LayerMask.NameToLayer("Wall")
            | 1 << LayerMask.NameToLayer("Nest")
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
        objectiveComplete = false;
        isExiting = false;
        text.text = "CANNOT ACTIVATE - COMPLETE CURRENT OBJECTIVE";
    }

    void Update()
    {
        // Check for interact input
        if (objectiveComplete && canActivate && !hasActivated && Input.GetButtonDown("Interact"))
        {
            hasActivated = true;

            ActivatePortal();

            exitTime = Time.time + DELAY_TIME;

            text.text = "PORTAL ACTIVATED - TELEPORTING IN 30 SECONDS";
            textTime = Time.time + TEXT_DELAY;
        }

        // Once entered, wait until time has passed beofre actually exiting
        if (!isExiting && hasActivated && Time.time > exitTime)
        {
            isExiting = true;
            StartCoroutine(TeleportToScene());
        }

        // Once entered, wait until some time has passed before disabling text
        if (hasActivated && Time.time > textTime)
        {
            text.gameObject.SetActive(false);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player" && !hasActivated)
        {
            canActivate = true;

            if (objectiveComplete)
            {
                text.text = "PRESS [F] TO ACTIVATE";
            }

            text.gameObject.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "Player" && !hasActivated)
        {
            canActivate = false;
            text.gameObject.SetActive(false);
        }
    }

    // Change to normal color when objectives complete and can activate portal
    public void OpenPortal()
    {
        ParticleSystem.MainModule ringsPs = rings.main;
        ringsPs.startColor = Color.white;

        ParticleSystem.MainModule groundEffectPs = groundEffect.main;
        groundEffectPs.startColor = Color.white;
    }

    // For when player actually activates portal
    private void ActivatePortal()
    {
        // Play audio
        audioSrcActivate.PlayOneShot(portalActivate);

        ParticleSystem.MainModule ringsPs = rings.main;
        ringsPs.startColor = Color.yellow;

        ParticleSystem.MainModule groundEffectPs = groundEffect.main;
        groundEffectPs.startColor = Color.yellow;
    }

    private IEnumerator TeleportToScene()
    {
        // Disable components as needed to show player has "teleported"
        player.GetComponent<CharacterController>().enabled = false;
        player.GetComponent<PlayerMoveController>().enabled = false;
        player.GetComponent<PlayerWeaponController>().enabled = false;
        firstPersonCamera.SetActive(false);

        // Wait a bit
        yield return new WaitForSeconds(3);

        // Load next scene
        SceneManager.LoadScene(nextScene);
    }
}
