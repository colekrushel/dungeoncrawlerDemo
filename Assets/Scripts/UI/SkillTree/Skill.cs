using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Skill", menuName = "Scriptable Objects/Skill")]
public class Skill : ScriptableObject
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
