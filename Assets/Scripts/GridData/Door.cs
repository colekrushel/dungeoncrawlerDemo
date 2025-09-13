using UnityEngine;

public class Door : CellEntity
{
    public bool open = true; //doors can be either open or closed

    new public void interact()
    {
        //first check if door is open; if open then continue; otherwise 
        if (!open)
        {
            //door not open; display text to the player indicating such
            Debug.Log("door closed!");

        } else
        {
            //when player interacts with an open door, do 2 things: play the door open animation, and move the player 2 tiles in front to the opposite side of the door.
        }

    }
}
