using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;

public interface ISkill 
{
    void ExecuteSkillEffects();
}

public interface ISkillEffect
{
    void ActivateEffect();
}

[Serializable]
public class PassiveEffect : ISkillEffect 
{
    [SerializeField]
    public string stat;
    [SerializeField]
    public float boost; //% increase or decrease
    [SerializeField]
    public bool flat; //whether boost is flat or multiplicative (%)
    public void ActivateEffect()
    {
        //effects are applied when the skill is added
    }
    
}

public class BuffEffect : ISkillEffect
{
    [SerializeReference] Buff buff;
    [SerializeField]
    public float cost; //cost in bits/mp
    [SerializeField]
    public float cooldown; //time between expire and use again in seconds
    public void ActivateEffect()
    {
        //apply buff to the player
        Player.playerStats.addBuff(buff);
    }
}



