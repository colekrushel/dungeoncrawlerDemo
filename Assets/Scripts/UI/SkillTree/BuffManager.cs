using System.Collections.Generic;
using UnityEngine;

//class to handle applying, removing, and observing of buffs applied to the player - only handles stats!
public class BuffManager : MonoBehaviour 
{
    List<Buff> activeBuffs;
    public static MonoBehaviour Instance { get; private set; }

    private void Update()
    {
        //decrement timers of timer based buffs

        
    }
    public BuffManager()
    {

    }

    public void addBuff(Buff b)
    {
        activeBuffs.Add(b);
        //determine if buff needs to be set now; health buffs need to be set immediately, but damage buffs are fetched when calculating damage
    }
}
