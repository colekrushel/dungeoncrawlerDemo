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

    public PlayerStats(PlayerStats old)
    {
        //copy values from inputted var
        baseDamage = old.baseDamage;
        damageMult = old.damageMult;
        healthMult = old.healthMult;
        baseHealth = old.baseHealth;
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
                    else damageMult += effect.boost * .01f;
                    break;
                case "health":
                    if (effect.flat) baseHealth += effect.boost;
                    else healthMult += effect.boost * .01f;
                    break;
            }
        }
    }

    //return a string of each stat in markup of
    //stat: result \n (baseStat * statMult)
    public string generateStatString()
    {
        string ret = "";
        ret += "Health: " + baseHealth * healthMult + "\n(" + baseHealth + " * " + healthMult + ")\n";
        ret += "Damage: " + baseDamage * damageMult + "\n(" + baseDamage+ " * " + damageMult + ")\n";
        return ret;
    }

    //return a stat string with changes from input skill applied and differences highlighted (stat increases in green, decreases in red)
    public string generateStatStringPreview(Skill skill)
    {
        //determine changes
        PlayerStats newstats = new PlayerStats(this);
        newstats.addSkillModifiers(skill);
        string ret = "";
        ret += "Health: " + returnDiffString("thealth", newstats.baseHealth * newstats.healthMult) + "\n(" + returnDiffString("bhealth", newstats.baseHealth) + " * " + returnDiffString("mhealth", newstats.healthMult) + ")\n";
        ret += "Damage: " + returnDiffString("tdamage", newstats.baseDamage * newstats.damageMult) + "\n(" + returnDiffString("bdamage", newstats.baseDamage) + " * " + returnDiffString("mdamage", newstats.damageMult) + ")\n";

        return ret;
    }

    public string returnDiffString(string istat, float newval)
    {
        string ret = "";
        //determine which stat to check
        float baseStat = 0;
        float statMult = 0;
        string stat = istat.Substring(1);
        switch (stat)
        {
            case "health":
                baseStat = baseHealth;
                statMult = healthMult;
                break;
            case "damage":
                baseStat = baseDamage;
                statMult = damageMult;
                break;
        }
        //determine if using total, base, or mult
        char c = istat[0];
        bool equal = false;
        switch (c)
        {
            case 't':
                if (newval > baseStat * statMult) ret += "<color=green>";
                else if (newval < baseStat * statMult) ret += "<color=red>";
                else equal = true;
                ret += newval;
                if (!equal) ret += "</color>";
                break;
            case 'b':
                if (newval > baseStat) ret += "<color=green>";
                else if (newval < baseStat) ret += "<color=red>";
                else equal = true;
                ret += newval;
                if (!equal) ret += "</color>";
                break;
            case 'm':
                if (newval > statMult) ret += "<color=green>";
                else if (newval < statMult) ret += "<color=red>";
                else equal = true;
                ret += newval;
                if (!equal) ret += "</color>";
                break;

        }
        return ret;
    }
}
