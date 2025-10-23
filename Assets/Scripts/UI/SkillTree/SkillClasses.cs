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


public class SkillActivationArgs
{
    
}

