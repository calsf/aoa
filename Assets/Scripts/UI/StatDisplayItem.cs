using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatDisplayItem : MonoBehaviour
{
    [SerializeField] private string statName;
    [SerializeField] private PlayerStateObject playerState;
    public PlayerStateObject.Stat stat { get; set; }

    void Start()
    {
        stat = playerState.stats[statName];
    }
}
