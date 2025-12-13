using System;
using UnityEngine;

//parent class for breakable objects; stores references to each breakable part and is treated as broken when all the parts are broken
public class BreakableConstruct : MonoBehaviour
{
    public BreakablePart[] breakableParts;
    public enum breakType { Wall, Item, Door, None, FieldItem}
    public breakType btype;
    public Action onBreak;
    public bool isBroken = false;

    public void setParts(BreakablePart[] parts)
    {
        breakableParts = parts;
        btype = parts[0].breakType;
    }

    public void partBreak(BreakablePart bp)
    {
        if (isBroken) return; //dont trigger breaks multiple times if multiple parts are broken at once
        bool broken = true;
        //when one part is broken check if whole construct is broken
        foreach (var part in breakableParts)
        {
            if (part.HP > 0) broken = false;
        }

        if (broken || breakableParts.Length == 0)
        {
            onBreak();
            isBroken = true;
        }

    }

  
}
