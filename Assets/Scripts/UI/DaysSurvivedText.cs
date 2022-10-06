using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DaysSurvivedText : MonoBehaviour
{
    [SerializeField] private PlayerStateObject playerState;

    private Text text;

    void Start()
    {
        text = GetComponent<Text>();
        text.text = "DAY " + playerState.daysSurvived;
    }
}
