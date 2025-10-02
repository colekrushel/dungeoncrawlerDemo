using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public static class Player 
{
    //player params
    private static Vector2 gridPos = Vector2.zero;
    public static GameObject playerObject;
    public static int currentLayer = 0;
    public static Tuple<float, float> between = new Tuple<float, float>(0, 0); //left number is lower layer, right is upper layer
    public static string facing;
    public static bool inputLock = false;
    public static List<char> actionQueue = new List<char>();
    //combat values
    private static int totalHP = 50;
    private static int currentHP = 50;
    public static float leftCooldown = 0;
    public static float rightCooldown = 0;
    public static float recoil = 0;
    public static bool isBlocking = false;
    //items
    public static PlayerInventory inventory = new PlayerInventory();
    public static EquipmentItem leftItem = null;
    public static EquipmentItem rightItem = null;
    public static float currentBlockHP;
    public static float maxBlockHP;

    static public void loadPlayerInfo()
    {
        //load in inventory from json
        //hardcoded now for testing
        inventory.addItem("Slasher", "breacher");
        //inventory.addItem("Blocker", "breacher");
        inventory.addItem("Smasher", "breacher");
        //Resources.Load<EquipmentItem>("Equipment/Slasher");
        //Resources.Load<EquipmentItem>("Equipment/Blocker");
        //update equipment display
        HandleEquipment.displayEquips();
}

    static public void teleportPlayer(Vector3 pos)
    {
        playerObject.transform.position = pos;
    }

    static public void updatePos(Vector2 newpos)
    {
        gridPos = newpos;

    }

    static public Vector2 getPos()
    {
        return gridPos;
    }

    static public void printPos()
    {
        Debug.Log(gridPos.x);
        Debug.Log(gridPos.y);
    }

    static public void updateFacing()
    {
        //180 && -180 = south (-y)
        //-90 = west (-x)
        //0 = north (+y)
        //90 = east (+x)
        float dir = Player.playerObject.transform.rotation.eulerAngles.y;
        facing = GridUtils.getDirectionFromDegrees((int)dir);
        //switch (dir)
        //{
        //    case 180:
        //        facing = "S";
        //        break;
        //    case 270:
        //        facing = "W";
        //        break;
        //    case 0:
        //        facing = "N";
        //        break;
        //    case 90:
        //        facing = "E";
        //        break;
        //}
    }

    static public void hitPlayer(int damage)
    {
        //deal damage to the player and perform feedback operations
        //check if blocking, if so then deal damage to the block instead
        if (isBlocking && currentBlockHP > 0)
        {
            //deal shield damage
            currentBlockHP -= damage;
            GameObject block = GameObject.Find("Firewall");
            MovementManager.shakeObject(block, 20f, 1f, .3f, block.transform.position);
        } else
        {
            currentHP = currentHP - damage;
            AnimateUI.updateHPMonitor(((float)currentHP / (float)totalHP));
        }

    }

    

}


