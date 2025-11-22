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

[Serializable]
public class BuffEffect : ISkillEffect
{
    [SerializeReference, SubclassSelector] public List<PassiveEffect> buffs;

    public void ActivateEffect()
    {
        //apply buff to the player
        foreach (PassiveEffect buff in buffs)
        {
            Player.playerStats.addBuff(buff);
        }
        
    }
}



