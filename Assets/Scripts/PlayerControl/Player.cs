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
    public static string orientation = "bottom"; //determines which side/zone player is in and their movements/rotation directions
    public static Tuple<float, float> between = new Tuple<float, float>(0, 0); //left number is lower layer, right is upper layer
    public static string facing;
    public static bool inputLock = false;
    public static List<char> actionQueue = new List<char>();
    //combat values
    public static PlayerStats playerStats = new PlayerStats();
    private static float totalHP = playerStats.getMaxHealth();
    private static float currentHP = totalHP;
    public static float leftCooldown = 0;
    public static float rightCooldown = 0;
    //public static float recoil = 0;    what is this even used for?
    public static bool isBlocking = false;
    
    //items
    public static PlayerInventory inventory = new PlayerInventory();
    public static EquipmentItem leftItem = Resources.Load<EquipmentItem>("Equipment/Slasher"); //autoequip to save time testing
    public static EquipmentItem rightItem = Resources.Load<EquipmentItem>("Equipment/Smasher");
    public static float currentBlockHP;
    public static float maxBlockHP;
    //other misc data
    private static int currencyHeld = 1000;
    public static List<Skill> skills = new List<Skill>();
    

    static public void loadPlayerInfo()
    {
        //load player info without existing data; initialize player data
        //hardcoded now for testing
        inventory.addItem("Slasher", "breacher");
        //inventory.addItem("Blocker", "breacher");
        inventory.addItem("Smasher", "breacher");
        //update equipment display
        HandleEquipment.displayEquips();
        HandleSkillTree.initializeTree(skills);
    }

    static public void loadPlayerInfo(PlayerSerializableData data)
    {
        //load player info from existing data
    }

    static public void setRotationFromOrientation()
    {
        //set the players orientation so that its axes match the side it is on
        //bottom's North is in the global +z direction
        playerObject.transform.eulerAngles = GridUtils.getZoneRotationEuler(orientation);
    }

    static public void teleportPlayer(Vector3 pos)
    {
        //add zone offset
        playerObject.transform.position = pos + GridUtils.getZoneOffset(orientation);
    }

    static public void updatePos(Vector2 newpos)
    {
        gridPos = newpos;

    }
    static public void updatePos(Vector2 newpos, int layer )
    {
        gridPos = newpos;
        currentLayer = layer;
        Debug.Log(layer);
    }

    static public Vector2 getPos()
    {
        return gridPos;
    }

    static public float getHP()
    {
        return currentHP;
    }

    static public int getCurrency()
    {
        return currencyHeld;
    }

    static public void printPos()
    {
        Debug.Log(gridPos.x);
        Debug.Log(gridPos.y);
    }

    static public void updateFacing()
    {
        //update facing by comparing the player's forward vector to its orientation's north/forward and east/right vectors
        facing = GridUtils.getDirectionOfObjectFacing(playerObject, orientation);
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

    static public void equipItem(EquipmentItem equipItem, bool left)
    {
        if (left)
        {
            leftItem = equipItem;
            leftCooldown = 0;
        } else
        {
            rightItem = equipItem;
            rightCooldown = 0;
        }
    }

    static public void addCurrency(int amt)
    {
        currencyHeld += amt;
        AnimateUI.addCurrency(amt);
    }


    static public void saveData()
    {
        //serialize player data to its own json file
    }

    //called when loading skills from data or unlocking a skill on the skill tree
    static public void addSkill(Skill newSkill)
    {
        skills.Add(newSkill);
        playerStats.addSkillModifiers(newSkill);
    }
    

}


