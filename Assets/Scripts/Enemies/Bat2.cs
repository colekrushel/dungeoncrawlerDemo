using UnityEngine;

public class Bat2 : Enemy
{
    private void Awake()
    {
        //fill enemy params with Bat's personal stats and animator
        //pos = p;
        //layer = l;
        HP = 200;
        baseDamage = 5;
        actionRange = 1;
        alertRange = 8;
        ICDBase = 1f;
        ICD = ICDBase;
        animator = gameObject.GetComponentInChildren<Animator>();
        dropAmount = 2000;

    }
}
