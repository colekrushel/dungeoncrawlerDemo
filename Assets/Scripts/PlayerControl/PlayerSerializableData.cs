using System;
using UnityEngine;

//holds all player data that needs to be serialized on save/load
[Serializable]
public class PlayerSerializableData
{
    string[] skills;
    string[] equipment;
    int currency;
    int xPos;
    int yPos;
    int layer;
    int currHealth;
    string facing;

    public PlayerSerializableData()
    {
        xPos = (int)Player.getPos().x;
        yPos = (int)Player.getPos().y;
        layer = Player.currentLayer;
        currHealth = Player.getHP();
        currency = Player.getCurrency();
        facing = Player.facing;
        equipment = Player.inventory.getItemsAsStringArray();
    }

}
