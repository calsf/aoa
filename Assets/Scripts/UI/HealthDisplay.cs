using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthDisplay : MonoBehaviour
{
    [SerializeField] private PlayerStateObject playerState;
    [SerializeField] private RectTransform healthFill;
    [SerializeField] private Text healthText;

    void Update()
    {
        healthFill.localScale = new Vector3(playerState.healthCurr / playerState.healthMax, 1, 1);

        healthText.text = (int) playerState.healthCurr + " / " + (int) playerState.healthMax;
    }
}
