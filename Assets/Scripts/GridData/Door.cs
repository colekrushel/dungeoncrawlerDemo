using System;
using System.Collections;
using UnityEngine;

public class Door : CellEntity 
{
    public bool open = true; //doors can be either open or closed
    Animator doorAnimator;

    public Door(CellEntity e)
    {
        //copy over params from entity loaded from json
        xpos = e.xpos;
        ypos = e.ypos;
        targetx = e.targetx;
        targety = e.targety;
        facing = e.facing;
        interactable = e.interactable;
        interactOnTile = e.interactOnTile;
        interactText = "i am a door!";
        entityInScene = e.entityInScene;
        doorAnimator = entityInScene.GetComponent<Animator>();
    }

    override public void interact()
    {
        //first check if door is open; if open then continue; otherwise 
        if (!open)
        {
            //door not open; display text to the player indicating such
            UIUtils.addMessageToLog("DOOROPENERROR: Insufficient Permissions", Color.red);

        } else
        {
            //when player interacts with an open door, do 2 things: play the door open animation, and move the player 2 tiles in front to the opposite side of the door, one tile at a time
            UIUtils.addMessageToLog("Opening Door", Color.green);
            doorAnimator.SetTrigger("Open");
            //lock player during animation
            Player.inputLock = true;
            //wait for animation to stop
            AnimUtils.waitForAnimationFinish(doorAnimator, "doorOpen");
            //StartCoroutine(PlayAnimationCoroutine());
            //stopped
            //Debug.Log("animation finished");
        }

    }

}
