using UnityEngine;

public class Bee2 : Enemy
{
    private void Awake()
    {
        //fill enemy params with Bat's personal stats and animator
        //pos = p;
        //layer = l;
        HP = 50;
        baseDamage = 2;
        actionRange = 8;
        alertRange = 8;
        ICDBase = 1f;
        ICD = ICDBase;
        animator = gameObject.GetComponentInChildren<Animator>();
        dropAmount = 700;

    }
}
