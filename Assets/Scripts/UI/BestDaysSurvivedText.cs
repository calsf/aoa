using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BestDaysSurvivedText : MonoBehaviour
{
    private Text text;

    void Start()
    {
        text = GetComponent<Text>();
        text.text = "BEST: DAY " + PlayerPrefs.GetInt("BestDaysSurvived", 0);
    }
}
