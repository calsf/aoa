using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShardCurrencyDisplay : MonoBehaviour
{
    [SerializeField] private Text currencyText;

    private int shardCurrency;

    void Start()
    {
        // Only need to set value once since currency will only ever increase right before loading new scene in Exit.cs
        shardCurrency = PlayerPrefs.GetInt("ShardCurrency", 0);
        currencyText.text = shardCurrency.ToString();
    }
}
