using Unity.VisualScripting;
using UnityEngine;

//a derived enemy class
public class Drone : Enemy
{
    private void Awake()
    {
        //fill enemy params with Drone's personal stats and animator
        //pos = p;
        //layer = l;
        HP = 10;
        baseDamage = 2;
        actionRange = 1;
        alertRange = 3;
        ICDBase = 1f;
        ICD = ICDBase;
        animator = gameObject.GetComponent<Animator>();
        
    }

    
}
