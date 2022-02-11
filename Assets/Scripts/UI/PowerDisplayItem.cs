using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PowerDisplayItem : MonoBehaviour
{
    [SerializeField] private Image img;
    [SerializeField] private Text text;

    public PlayerStateObject.Power power { get; set; }

    void Start()
    {
        img.enabled = false;
        text.enabled = false;
    }

    public void ShowPower(Sprite sprite, string powerName)
    {
        img.sprite = sprite;
        text.text = powerName;
        img.enabled = true;
        text.enabled = true;
    }
}
