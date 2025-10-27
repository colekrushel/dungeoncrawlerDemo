using System;
using UnityEngine;

//holds all player data that needs to be serialized on save/load
[Serializable]
public class PlayerSerializableData
{
    //TODO ensure skills and equipment are serializable and serialize lists of those objects instead
    string[] skills;
    string[] equipment;
    PlayerStats playerStats;
    int currency;
    int xPos;
    int yPos;
    int layer;
    string facing;


    public PlayerSerializableData()
    {
        xPos = (int)Player.getPos().x;
        yPos = (int)Player.getPos().y;
        layer = Player.currentLayer;
        currency = Player.getCurrency();
        facing = Player.facing;
        equipment = Player.inventory.getItemsAsStringArray();
    }

}
