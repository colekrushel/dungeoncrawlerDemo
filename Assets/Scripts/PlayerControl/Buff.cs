using UnityEngine;

public class Buff
{
    [SerializeField]
    public string stat;
    [SerializeField]
    public float boost; //% increase or decrease
    [SerializeField]
    public bool flat; //whether boost is flat or multiplicative (%)
    [SerializeField]
    public float time; //length in seconds of the buff

    public enum expireType { time, onHit, takeDamage } //ways for the buff to expire
    public expireType expire;
}
