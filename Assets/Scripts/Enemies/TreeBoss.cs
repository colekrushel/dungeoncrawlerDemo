using UnityEngine;

public class TreeBoss : Enemy
{
    private void Awake()
    {
        //fill enemy params with Bat's personal stats and animator
        //pos = p;
        //layer = l;
        HP = 500;
        baseDamage = 2;
        actionRange = 10;
        alertRange = 10;
        ICDBase = 2f;
        ICD = ICDBase;
        animator = gameObject.GetComponentInChildren<Animator>();
        dropAmount = 10000;
        phaseThreshold = HP / 2;
    }
}
