using System;
using System.Globalization;
using UnityEngine;

[Serializable]
public class CellEntity 
{
    public int xpos; //ints for serializing (cant serialize vectors... i think)
    public int ypos;
    public int targetx; //'target' position - used for entities that need to reference another position, like a teleporter or lever
    public int targety;
    public string facing; // N/E/S/W direction entity is facing (should align with model in game scene)
    public bool interactable = false; //whether player can interact with this entity to do something
    public bool interactOnTile = false; //whether this entity is interactable from a neighboring tile or when standing at same pos (+ direction?) (mutually exclusive)
    public string interactText = "placeholder";
    public GameObject entityInScene;
    public int layer;


    //method called; delegate to entity subclasses
    virtual public void interact() {Debug.Log("default interact");}

    
}
