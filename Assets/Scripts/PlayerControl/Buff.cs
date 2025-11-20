using System;
using UnityEngine;

[Serializable]
public class Buff
{
    [SerializeField]
    public string stat;
    [SerializeField]
    public float boost; //% increase or decrease
    [SerializeField]
    public bool flat; //whether boost is flat or multiplicative (%)

}
