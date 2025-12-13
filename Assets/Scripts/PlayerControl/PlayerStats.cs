using System.Collections.Generic;

//store player stats and modifiers from skills so they dont have to be constantly dereferenced
public class PlayerStats 
{    

    float baseDamage;
    float damageMult = 1;

    float baseHealth;
    float healthMult = 1;

    float baseCooldown = 1;
    float cooldownMult = 1;

    float currencyMult = 1;

    bool real;//stop playerstats created for preview purposes from affecting the actual stats 

    /* unimplemented stats
    float moveSpeedMult = 1;

    float baseArmor
    */

    //misc specific effects

    public PlayerStats()
    {
        //fill with default values on initialization
        baseHealth = 10;
        baseDamage = 2;
        baseCooldown = 1;
        real = true;
    }

    public PlayerStats(PlayerStats old)
    {
        //copy values from inputted var
        baseDamage = old.baseDamage;
        damageMult = old.damageMult;
        healthMult = old.healthMult;
        baseHealth = old.baseHealth;
        baseCooldown = old.baseCooldown;
        cooldownMult = old.cooldownMult;
        real = false; 
    }

    public float getMaxHealth()
    {
        return baseHealth * healthMult;
    }

    //damage formula: (player base dmg * player mult * weapon mult) - enemy armor
    public float getDamage()
    {
        return baseDamage * damageMult;
    }

    public float getCooldown()
    {
        return baseCooldown * cooldownMult;
    }
    
    public float getCurrencyMult()
    {
        return currencyMult;
    }

    public void addSkillModifiers(Skill newSkill)
    {
        List<PassiveEffect> fx = newSkill.GetPassiveSkillEffects();
        foreach (PassiveEffect effect in fx)
        {
           addSkillModifiers(effect);
        }
    }

    public void addSkillModifiers(Skill newSkill, float mult)
    {
        List<PassiveEffect> fx = newSkill.GetPassiveSkillEffects();
        foreach (PassiveEffect effect in fx)
        {
            addSkillModifiers(effect, mult);
        }
    }

    public void addSkillModifiers(PassiveEffect effect, float mult = 1)
    {
        float boost = effect.boost * mult;
        switch (effect.stat.ToLower())
        {
            case "damage":
                if (effect.flat) baseDamage += boost;
                else damageMult += boost * .01f;
                break;
            case "health":
                //when health is added increase player's current and total health by that amt
                float addedamt = 0;
                if (effect.flat) { baseHealth += boost; addedamt = boost; }
                else { healthMult += boost * .01f; addedamt = baseHealth * (boost * .01f); }
                if(real)Player.addMaxHP(addedamt);
                break;
            case "cooldown":
                if (effect.flat) baseCooldown += boost;
                else cooldownMult += boost * .01f;
                break;
            case "currency":
                currencyMult += boost * .01f;
                break;
        }
        
    }

    //return a string of each stat in markup of
    //stat: result \n (baseStat * statMult)
    public string generateStatString()
    {
        string ret = "";
        ret += "Health: " + baseHealth * healthMult + "\n(" + baseHealth + " * " + healthMult + ")\n";
        ret += "Damage: " + baseDamage * damageMult + "\n(" + baseDamage+ " * " + damageMult + ")\n";
        ret += "Cooldown: " + baseCooldown * cooldownMult + "\n(" + baseCooldown + " * " + cooldownMult + ")\n";
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
        ret += "Cooldown: " + returnDiffString("tcooldown", newstats.baseCooldown * newstats.cooldownMult) + "\n(" + returnDiffString("bcooldown", newstats.baseCooldown) + " * " + returnDiffString("mcooldown", newstats.cooldownMult) + ")\n";
        return ret;
    }

    public string returnDiffString(string istat, float newval)
    {
        string ret = "";
        //determine which stat to check
        float baseStat = 0;
        float statMult = 0;
        bool flipPositive = false; //for some stats a decrease should be labeled as positive(green) rather than red
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
            case "cooldown":
                baseStat = baseCooldown;
                statMult = cooldownMult;
                flipPositive = true;
                break;
        }
        //determine if using total, base, or mult
        char c = istat[0];
        bool equal = false;
        switch (c)
        {
            case 't':
                if (newval > baseStat * statMult) {if (!flipPositive) ret += "<color=green>"; else ret += "<color=red>";}
                else if (newval < baseStat * statMult) { if (!flipPositive) ret += "<color=red>"; else ret += "<color=green>"; }
                else equal = true;
                ret += newval;
                if (!equal) ret += "</color>";
                break;
            case 'b':
                if (newval > baseStat) { if (!flipPositive) ret += "<color=green>"; else ret += "<color=red>"; }
                else if (newval < baseStat) { if (!flipPositive) ret += "<color=red>"; else ret += "<color=green>"; }
                else equal = true;
                ret += newval;
                if (!equal) ret += "</color>";
                break;
            case 'm':
                if (newval > statMult) { if (!flipPositive) ret += "<color=green>"; else ret += "<color=red>"; }
                else if (newval < statMult) { if (!flipPositive) ret += "<color=red>"; else ret += "<color=green>"; }
                else equal = true;
                ret += newval;
                if (!equal) ret += "</color>";
                break;

        }
        return ret;
    }


    //buffs are just passive effects that are designated to be removed later
    public void addBuff(PassiveEffect buff)
    {
        //apply the buff directly; its ok because passive effects are all applied additively.
        addSkillModifiers(buff);
        //buff will be removed when timer reaches 0 on the skillbox.
    }

    public void removeBuffs(Skill newSkill)
    {
        //to remove skill modifiers just inverse the effect because they are added additively
        List<BuffEffect> fx = newSkill.GetBuffSkillEffects();
        foreach (BuffEffect effect in fx)
        {
            foreach(PassiveEffect buff in effect.buffs)
            addSkillModifiers(buff, -1);
        }
    }

}
