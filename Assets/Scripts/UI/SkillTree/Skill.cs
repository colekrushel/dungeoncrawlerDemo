using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Skill", menuName = "Scriptable Objects/Skill")]
public class Skill : ScriptableObject, ISkill
{
    [SerializeField] Sprite icon;

    [SerializeField]
    public string description;

    [SerializeField]
    public int price;
    [SerializeReference, SubclassSelector]
    private List<ISkillEffect> skillEffects = new();

    public void ExecuteSkillEffects()
    {
        foreach (var effect in skillEffects)
        {
            effect.ActivateEffect();
        }
    }

    public List<ISkillEffect> GetSkillEffects() { return skillEffects; }
    public List<PassiveEffect> GetPassiveSkillEffects() {
        List<PassiveEffect> pfx = new List<PassiveEffect>();
        foreach (var effect in skillEffects)
        {
            if(effect is PassiveEffect)
            {
                pfx.Add((PassiveEffect)effect);
            }
        }
        return pfx;
    }
}
