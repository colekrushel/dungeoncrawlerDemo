using UnityEngine;

public class Bee : Enemy
{
    private void Awake()
    {
        //fill enemy params with Bat's personal stats and animator
        //pos = p;
        //layer = l;
        HP = 25;
        baseDamage = 2;
        actionRange = 2;
        alertRange = 5;
        ICDBase = 1f;
        ICD = ICDBase;
        animator = gameObject.GetComponentInChildren<Animator>();
        dropAmount = 200;

    }
}
