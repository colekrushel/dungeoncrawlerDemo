using UnityEngine;

public class RedirectHitEvent : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    [SerializeField] Enemy enemyscript;
 

    //because animationevents need to be on the same object as the animator, and imported models have weird animator positions, redirect the event to the actual enemy script
    public void attackHit()
    {
        enemyscript.attackHit();
    }


    }
