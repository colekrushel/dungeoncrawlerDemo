using UnityEngine;

public class Bat : Enemy
{
    private void Awake()
    {
        //fill enemy params with Bat's personal stats and animator
        //pos = p;
        //layer = l;
        HP = 100;
        baseDamage = 2;
        actionRange = 1;
        alertRange = 5;
        ICDBase = 1f;
        ICD = ICDBase;
        animator = gameObject.GetComponentInChildren<Animator>();
        dropAmount = 200;

    }
}
