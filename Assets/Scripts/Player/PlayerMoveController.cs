using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMoveController : MonoBehaviour
{
    private const int POOL_NUM = 10;
    private const float SENS_SPEED = 5;

    [SerializeField] private PlayerStateObject playerState;

    private Settings settings;

    private const float MAX_Y = -90f;
    private const float MIN_Y = 90f;
    private const float GRAVITY = -9.81f * 3f;
    private const float SPEED_BASE = 15f;
    private const float JUMP_HEIGHT = 15f;
    private const float STRAFE_MODIFIER = .95f;
    private const float AIM_MOVE_MODIFIER_X = .5f;
    private const float AIM_MOVE_MODIFIER_Z = .7f;
    private const float AIR_ACCEL_MODIFIER = 5.5f;

    // Movement
    [SerializeField] private ParticleSystem slideEffectForward;
    [SerializeField] private ParticleSystem slideEffectBackward;
    [SerializeField] private ParticleSystem slideEffectLeft;
    [SerializeField] private ParticleSystem slideEffectRight;
    CharacterController controller;
    private float speedMax;
    private float speedCurr;
    private float currVelocityY;
    private Vector3 velocity;

    public bool isSliding { get; set; }
    public bool isAiming { get; set; }

    private bool isSlideJump;

    private int jumpMaxAvailable;
    private int jumpCurrAvailable;

    // Looking
    [SerializeField] private Transform cam;
    private float mouseSens;
    private float cameraY = 0f;

    // Possible player starting spawn positions
    [SerializeField] private Transform[] playerSpawns;

    [SerializeField] private GameObject rocketObject;
    protected List<GameObject> rocketObjectPool;

    protected float[] pitches = { 1, .9f, 1.1f };
    protected int playedCount = 0;
    [SerializeField] protected AudioSource audioSrcMovement;

    [SerializeField] protected AudioClip jump;

    void Awake()
    {
        // Set up audio
        SoundManager soundManager = GameObject.FindGameObjectWithTag("SoundManager").GetComponent<SoundManager>();
        soundManager.AddAudioSource(audioSrcMovement);

        settings = GameObject.FindGameObjectWithTag("Settings").GetComponent<Settings>();
        controller = GetComponent<CharacterController>();

        controller.enabled = false; // Disable character controller before manually changing position

        // Spawn player at a random possible spawn position
        Vector3 spawnPos = playerSpawns[Random.Range(0, playerSpawns.Length)].position;
        transform.position = spawnPos;
        transform.LookAt(new Vector3(0, (spawnPos - Vector3.zero).y, 0));

        controller.enabled = true; // Re-enable character controller

        // Initialize pool of rockets
        rocketObjectPool = new List<GameObject>();
        for (int i = 0; i < POOL_NUM; i++)
        {
            rocketObjectPool.Add(Instantiate(rocketObject, Vector3.zero, Quaternion.identity));

            rocketObjectPool[i].SetActive(false);
        }
    }

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;

        UpdateSensitivity();

        jumpMaxAvailable = 1 + (int) playerState.stats["JumpBonus"].statValue;
        speedMax = SPEED_BASE + playerState.stats["MoveSpeedBonus"].statValue;
        speedCurr = speedMax;
    }

    void OnEnable()
    {
        playerState.OnStateUpdate.AddListener(UpdatePlayerMoveState);
        settings.OnSettingsSaved.AddListener(UpdateSensitivity);
    }

    void OnDisable()
    {
        playerState.OnStateUpdate.RemoveListener(UpdatePlayerMoveState);
        settings.OnSettingsSaved.RemoveListener(UpdateSensitivity);
    }

    void Update()
    {
        // Do not run if timescale is 0
        if (Time.timeScale == 0)
        {
            return;
        }

        Look();
        Move();
    }

    // Play movement audio and change pitch
    public void PlayAudioClip(AudioClip audioClip)
    {
        if (playedCount > pitches.Length - 1)
        {
            playedCount = 0;
        }

        audioSrcMovement.pitch = pitches[playedCount];
        audioSrcMovement.PlayOneShot(audioClip);
        playedCount++;
    }

    // Update player stats
    private void UpdatePlayerMoveState()
    {
        jumpMaxAvailable = 1 + (int)playerState.stats["JumpBonus"].statValue;
        speedMax = SPEED_BASE + playerState.stats["MoveSpeedBonus"].statValue;
        speedCurr = speedMax;
    }

    // Update mouse sensitivity
    private void UpdateSensitivity()
    {
        mouseSens = PlayerPrefs.GetFloat("Sensitivity", .3f) * SENS_SPEED;
        
        // Have a min mouse sen
        if (mouseSens <= .05f)
        {
            mouseSens = .05f;
        }
    }

    private void Look()
    {
        Vector2 mouseDelta = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));

        // Set cameraY based on mouse and clamp values
        cameraY -= mouseDelta.y * mouseSens;
        cameraY = Mathf.Clamp(cameraY, MAX_Y, MIN_Y);

        // Look Y - Rotate camera along Y
        cam.localEulerAngles = Vector3.right * cameraY;

        // Look X - Rotate player itself along X, camera will also be moved since camera is a child of the player
        transform.Rotate(Vector3.up * mouseDelta.x * mouseSens);
    }

    private void Move()
    {
        // Get move direction
        Vector3 newMoveDir = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));
        newMoveDir.Normalize();

        // Check is grounded
        if (controller.isGrounded) // Reset values if grounded
        {
            jumpCurrAvailable = jumpMaxAvailable;
            currVelocityY = -1f;

            isSlideJump = false;

            // Reset speed if is not sliding and is moving faster than max speed
            if (!isSliding && speedCurr > speedMax)
            {
                speedCurr = speedMax;
            }
        }
        else // Apply gravity to velocityY if not grounded
        {
            if (isAiming && playerState.powers["AimGlide"].isActive && !controller.isGrounded) // Aim glide - apply less gravity if in air and aiming
            {
                currVelocityY += (GRAVITY / 3) * Time.deltaTime;
            }
            else
            {
                currVelocityY += GRAVITY * Time.deltaTime;
            }
        }

        // Slide input, must be moving in a direction, grounded, and not already sliding
        if (Input.GetButtonDown("Slide") && newMoveDir != Vector3.zero && !isSliding)
        {
            // If player is not grounded and does not have Air Slide, do not allow slide
            // Must be grounded OR have Air Slide which will let player slide mid air
            if (!playerState.powers["AirSlide"].isActive && !controller.isGrounded)
            {
                return;
            }

            // Play slide effect based on initial move direction (prioritize forward or backwards effects unless no forward/backward input)
            else if (newMoveDir.z > 0) // Forward
            {
                slideEffectForward.Play();
            }
            else if (newMoveDir.z < 0) // Backward
            {
                slideEffectBackward.Play();
            }
            else if (newMoveDir.x > 0) // Right
            {
                slideEffectRight.Play();
            }
            else if (newMoveDir.x < 0) // Left
            {
                slideEffectLeft.Play();
            }

            isSliding = true;
            
            StartCoroutine(Slide());
        }

        // Jump input, must have available jumps
        if (Input.GetButtonDown("Jump") && jumpCurrAvailable > 0)
        {
            // If sliding, toggle off to cancel slide and jump, should keep the slide speed until player lands again
            if (isSliding)
            {
                isSliding = false;
                isSlideJump = true;
            }

            // Consume 1 available jump
            jumpCurrAvailable--;

            currVelocityY = 0; // Reset to 0 so jump heights are always consistent
            currVelocityY += Mathf.Sqrt((JUMP_HEIGHT * -1f) * GRAVITY); // Set velocityY to be some positive velocity based on jump height

            // Play audio
            PlayAudioClip(jump);

            // Rocket Jump
            if (playerState.powers["RocketJump"].isActive)
            {
                GameObject rocket = GetFromPool(rocketObjectPool, rocketObject);
                rocket.transform.position = transform.position;
                rocket.SetActive(true);
            }
        }

        // Apply movement and velocityY
        if (isSliding && newMoveDir == Vector3.zero) // For sliding, maintain velocity even if no input
        {
            velocity.y = -1;
        }
        else if (!isSliding && !controller.isGrounded) // If in air and not sliding, apply move direction for air movement
        {
            // Have air acceleration be a multiplier of max speed
            velocity += transform.TransformVector(newMoveDir) * (AIR_ACCEL_MODIFIER * speedMax) * Time.deltaTime;

            Vector3 velocityHorizontal = Vector3.ProjectOnPlane(velocity, Vector3.up);
            velocityHorizontal = Vector3.ClampMagnitude(velocityHorizontal, isSlideJump ? speedCurr : speedMax); // Use current speed as max if was a slide jump cancel
            velocity = velocityHorizontal;
        }
        else // Else apply move direction input
        {
            // Use speedCurr by default
            float speedX = speedCurr;
            float speedZ = speedCurr;

            // Change move speed if player is aiming
            if (isAiming && !isSliding && controller.isGrounded)
            {
                speedX = speedCurr * AIM_MOVE_MODIFIER_X;
                speedZ = speedCurr * AIM_MOVE_MODIFIER_Z;
            }

            velocity = (
                (transform.right * newMoveDir.x) * (speedX * STRAFE_MODIFIER) 
                + (transform.forward * newMoveDir.z) * (speedZ));
        }

        // Apply curr Y velocity and gravity to velocity
        velocity.y += currVelocityY + GRAVITY * Time.deltaTime;

        // Reset y velocity values if sliding
        if (isSliding)
        {
            currVelocityY = 0;
            velocity.y = 0;
        }

        controller.Move(velocity * Time.deltaTime);
    }

    IEnumerator Slide()
    {
        float waitTime;
        float elapsedTime;
        float targetY;
        float originalY = transform.position.y;
        float startY;

        // Set target y position and set controller values for slide
        targetY = originalY - 1.5f;
        controller.height = .5f;
        controller.center = new Vector3(0, 2f, 0);

        // Increase move speed for slide
        speedCurr *= 1.5f;

        // Lerp y position for slide duration or until isSliding is false, which will cancel slide
        waitTime = .2f;
        elapsedTime = 0f;
        startY = transform.position.y;
        while (elapsedTime < waitTime && isSliding)
        {
            float y = Mathf.Lerp(startY, targetY, elapsedTime / waitTime);
            elapsedTime += Time.deltaTime;

            transform.position = new Vector3(transform.position.x, y, transform.position.z);

            yield return null;
        }

        // Slide duration while in sliding position
        waitTime = .35f;
        elapsedTime = 0f;
        while (elapsedTime < waitTime && isSliding)
        {
            elapsedTime += Time.deltaTime;

            yield return null;
        }

        // Set target y to original
        targetY = originalY;

        // Reset move speed to original/max speed if player is still sliding
        // isSliding would be false is slide is cancelled by jump and player should maintain the slide speed until they land
        if (isSliding)
        {
            speedCurr = speedMax;
        }

        // Lerp y position back to original position
        waitTime = .2f;
        elapsedTime = 0f;
        startY = transform.position.y;
        while (elapsedTime < waitTime) // Stop within range to avoid jitter
        {
            float y = Mathf.Lerp(startY, targetY, elapsedTime / waitTime);
            elapsedTime += Time.deltaTime;

            transform.position = new Vector3(transform.position.x, y, transform.position.z);

            yield return null;
        }

        // Reset y position to original and reset controller values
        transform.position = new Vector3(transform.position.x, originalY, transform.position.z);
        controller.height = 2;
        controller.center = new Vector3(0, 1, 0);

        // Reset slide
        isSliding = false;
    }

    protected GameObject GetFromPool(List<GameObject> pool, GameObject obj)
    {
        for (int i = 0; i < pool.Count; i++)
        {
            if (!pool[i].activeInHierarchy)
            {
                return pool[i];
            }
        }

        // If no object in the pool is available, create a new object and add to the pool
        GameObject newObj = Instantiate(obj, Vector3.zero, Quaternion.identity);
        pool.Add(newObj);
        return newObj;
    }
}
