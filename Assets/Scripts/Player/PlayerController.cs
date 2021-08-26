using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private const float MAX_Y = -90f;
    private const float MIN_Y = 90f;
    private const float GRAVITY = -9.81f * 2.5f;
    private const float SPEED_BASE = 10f;
    private const float JUMP_HEIGHT = 6f;
    private const float STRAFE_MODIFIER = .8f;
    private const float AIM_MOVE_MODIFIER_X = .35f;
    private const float AIM_MOVE_MODIFIER_Z = .6f;

    // Movement
    CharacterController controller;
    private float speedMax;
    private float speedCurr;
    private float currVelocityY;
    private Vector3 velocity;

    public bool isSliding { get; set; }
    public bool isAiming { get; set; }

    private int jumpMaxAvailable;
    private int jumpCurrAvailable;

    // Looking
    [SerializeField] private Transform cam;
    private float mouseSens = 1.5f;
    private float cameraY = 0f;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;

        controller = GetComponent<CharacterController>();

        jumpMaxAvailable = 3;
        speedMax = SPEED_BASE;
        speedCurr = speedMax;
    }

    void Update()
    {
        Look();
        Move();
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
        Vector2 newMoveDir = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        newMoveDir.Normalize();

        // Check is grounded
        if (controller.isGrounded) // Reset velocityY if grounded
        {
            jumpCurrAvailable = jumpMaxAvailable;
            currVelocityY = 0f;

            // Reset speed if is not sliding and is moving faster than max speed
            if (!isSliding && speedCurr > speedMax)
            {
                speedCurr = speedMax;
            }
        }
        else // Apply gravity to velocityY if not grounded
        {
            currVelocityY += GRAVITY * Time.deltaTime;
        }

        // Slide input, must be moving in a direction, grounded, and not already sliding
        if (Input.GetButtonDown("Slide") && newMoveDir != Vector2.zero && controller.isGrounded && !isSliding)
        {
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
            }

            // Consume 1 available jump
            jumpCurrAvailable--;

            // Set velocityY to be some positive velocity based on jump height
            if (currVelocityY < 0)  // Reset to 0 if necessary so first jump height is accurate
            {
                currVelocityY = 0;
            }
            currVelocityY += Mathf.Sqrt((JUMP_HEIGHT * -1f) * GRAVITY);
        }

        // Apply movement and velocityY
        if ((isSliding || !controller.isGrounded) && newMoveDir == Vector2.zero) // If no move input and is sliding or in air, do not apply movement
        {
            // Reset y velocity to 0
            velocity.y = 0;
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
                + (transform.forward * newMoveDir.y) * (speedZ));
        }

        // Apply curr Y velocity and gravity to velocity
        velocity.y += currVelocityY + GRAVITY * Time.deltaTime;

        controller.Move(velocity * Time.deltaTime);
    }

    IEnumerator Slide()
    {
        float waitTime;
        float elapsedTime;
        float targetY;
        float originalY = transform.position.y;

        // Set target y position and set controller values for slide
        targetY = originalY - 1;
        controller.height = 1;
        controller.center = new Vector3(0, 1.5f, 0);

        // Increase move speed for slide
        speedCurr *= 2;

        // Lerp y position for slide duration or until isSliding is false, which will cancel slide
        waitTime = .5f;
        elapsedTime = 0f;
        while (elapsedTime < waitTime && isSliding)
        {
            transform.position = Vector3.Lerp(transform.position, new Vector3(transform.position.x, targetY, transform.position.z), elapsedTime / waitTime);
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
        while (elapsedTime < waitTime && Mathf.Abs(transform.position.y - originalY) > .1f) // Stop within range to avoid jitter
        {
            transform.position = Vector3.Lerp(transform.position, new Vector3(transform.position.x, targetY, transform.position.z), elapsedTime / waitTime);
            elapsedTime += Time.deltaTime;
 
            yield return null;
        }

        // Reset y position to original and reset controller values
        transform.position = new Vector3(transform.position.x, originalY, transform.position.z);
        controller.height = 2;
        controller.center = new Vector3(0, 1, 0);

        // Reset slide
        isSliding = false;
    }
}
