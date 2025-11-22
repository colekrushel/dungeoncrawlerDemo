using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Skill", menuName = "Scriptable Objects/Skill")]
public class Skill : ScriptableObject, ISkill
{
    [SerializeField] public Sprite icon;

    [SerializeField]
    public string description;

    [SerializeField]
    public int price;
    [SerializeReference, SubclassSelector]
    private List<ISkillEffect> skillEffects = new();

    //params that only apply to buff/temporary effects
    [SerializeField]
    public float time; //length in seconds of the buff
    [SerializeField]
    public float cooldown; //time between casts of the buff in seconds; must be longer than the time
    public enum expireType { time, onHit, takeDamage } //ways for the buff to expire
    [SerializeField]
    public expireType expire;

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

    public List<BuffEffect> GetBuffSkillEffects()
    {
        List<BuffEffect> pfx = new List<BuffEffect>();
        foreach (var effect in skillEffects)
        {
            if (effect is BuffEffect)
            {
                pfx.Add((BuffEffect)effect);
            }
        }
        return pfx;
    }
}
