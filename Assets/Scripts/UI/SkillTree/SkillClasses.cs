using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public interface ISkill 
{
    void ExecuteSkillEffects(SkillActivationArgs args);
}

public interface ISkillEffect
{
    void ActivateEffect(SkillActivationArgs args);
}

public class Skill : ScriptableObject, ISkill
{
    [SerializeReference]
    private List<ISkillEffect> skillEffects = new();

    public void ExecuteSkillEffects(SkillActivationArgs args)
    {
        foreach (var effect in skillEffects)
        {
            effect.ActivateEffect(args);
        }
    }
}

public class SkillActivationArgs
{
    
}

