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
        //apply passive effects to the player
    }
    
}



