using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DamageNumber : MonoBehaviour
{
    private Color bodyShotColor = Color.yellow;
    private Color headShotColor = Color.red;
    private Color clonedBodyShotColor = new Color(1, .94f, .63f, .6f);
    private Color clonedHeadShotColor = new Color(1, .65f, 0, .6f);
    private Color otherColor = Color.white;

    private Text damageText;
    private Camera cam;
    private RectTransform parentRect; // Main parent object container
    [SerializeField] RectTransform textRect; // Child text object

    private Vector3 worldPosition;

    void Awake()
    {
        damageText = GetComponentInChildren<Text>();
        cam = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
        parentRect = GetComponent<RectTransform>();
    }

    void Update()
    {
        // Keep updating parent rect's screen point position based on world position so it stays in correct position while child text object moves
        parentRect.position = cam.WorldToScreenPoint(worldPosition);

        // Need to avoid having damage number that is behind the player showing up in front of the camera/screen
        if (parentRect.position.z < 0)
        {
            parentRect.position *= -1;
        }
    }

    public void Display(float damage, Vector3 hitPos, bool isHeadshot, bool isClonedShot, bool isOther)
    {
        // Set text color
        if (isClonedShot)
        {
            damageText.color = isHeadshot ? clonedHeadShotColor : clonedBodyShotColor;
        }
        else if (isOther)
        {
            damageText.color = otherColor;
        }
        else
        {
            damageText.color = isHeadshot ? headShotColor : bodyShotColor;
        }

        // Set text
        int damageRounded = Mathf.RoundToInt(damage);

        /*
        // If rounded damage is 0, ignore display
        if (damageRounded == 0)
        {
            return;
        }
        */

        damageText.text = damageRounded.ToString();

        // Set world position
        worldPosition = hitPos;

        // Set screen point position and reset child text position
        parentRect.position = cam.WorldToScreenPoint(worldPosition);
        textRect.localPosition = Vector3.zero;

        gameObject.SetActive(true);

        StartCoroutine(AnimateDamageNumber());
    }

    private IEnumerator AnimateDamageNumber()
    {
        // Move random amount along x
        float xMovement = Random.Range(-25, 25);

        Vector3 firstTarget = textRect.localPosition + (Vector3.up * 6f) + ((Vector3.left * xMovement) / 2);
        Vector3 secondTarget = textRect.localPosition - (Vector3.up * 8f) + (Vector3.left * xMovement);

        float firstTargetTime = .2f;
        float secondTargetTime = .3f;

        // Move to first target position
        float elapsedTime = 0;
        Vector3 startLocalPos = textRect.localPosition;
        while (elapsedTime < firstTargetTime)
        {
            textRect.localPosition = Vector3.Lerp(startLocalPos, firstTarget, elapsedTime / firstTargetTime);
            elapsedTime += Time.deltaTime;

            yield return null;
        }

        // Move to second target position
        elapsedTime = 0;    // Reset elapsed time
        startLocalPos = textRect.localPosition;     // Reset start local position
        while (elapsedTime < secondTargetTime)
        {
            textRect.localPosition = Vector3.Lerp(startLocalPos, secondTarget, elapsedTime / secondTargetTime);
            elapsedTime += Time.deltaTime;

            yield return null;
        }

        // Deactivate damage number after animation plays
        gameObject.SetActive(false);
    }
}
