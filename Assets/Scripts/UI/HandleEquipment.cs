using System.Collections.Generic;
using UnityEngine;


public class HandleEquipment 
{
    [SerializeField] GameObject EquipmentBox;
    List<GameObject> EquipmentPanels;

    public void displayEquips()
    {
        //fill the grid with equipment from the player's inventory
        //add a nothing box

        //add a box for every equipment item
        foreach (EquipmentItem equipment in Player.inventory.equipmentItems)
        {

        }
    }

    public void selectEquipment()
    {
        //triggered when clicking on an item in the grid;

        //check if right click or left click
    }

}
