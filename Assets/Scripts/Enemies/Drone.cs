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
        attackRange = 1;
        alertRange = 2;
        ICDBase = 1;
        ICD = ICDBase;
        animator = gameObject.GetComponent<Animator>();
        
    }

    
}
