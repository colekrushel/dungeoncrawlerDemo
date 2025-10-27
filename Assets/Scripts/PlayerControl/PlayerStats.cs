using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

//store player stats and modifiers from skills so they dont have to be constantly dereferenced
public class PlayerStats 
{
    float baseDamage;
    float damageMult = 1;

    float baseHealth;
    float healthMult = 1;

    /* unimplemented stats
    float actionSpeedMult = 1;

    float moveSpeedMult = 1;

    float baseArmor
    */

    //misc specific effects

    public PlayerStats()
    {
        //fill with default values on initialization
        baseHealth = 10;
        baseDamage = 5;
    }

    public float getMaxHealth()
    {
        return baseHealth * healthMult;
    }

    //damage formula: (weapon dmg + player base dmg * player mult) - enemy armor
    public float getDamage()
    {
        return baseDamage * damageMult;
    }

    public void addSkillModifiers(Skill newSkill)
    {
        List<PassiveEffect> fx = newSkill.GetPassiveSkillEffects();
        foreach (PassiveEffect effect in fx)
        {
            switch (effect.stat.ToLower())
            {
                case "damage":
                    if (effect.flat) baseDamage += effect.boost;
                    else damageMult += effect.boost;
                    break;
                case "health":
                    if (effect.flat) baseHealth += effect.boost;
                    else healthMult += effect.boost;
                    break;
            }
        }
    }
}
