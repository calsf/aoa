using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CurrencyDisplay : MonoBehaviour
{
    [SerializeField] private PlayerStateObject playerState;

    [SerializeField] private Text currencyText;

    void Update()
    {
        currencyText.text = playerState.bloodCurrency.ToString();
    }
}
