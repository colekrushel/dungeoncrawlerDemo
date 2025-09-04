using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditorInternal;
using System;

[Serializable]
public class DungeonCell 
{
    //attributes
    public int gridX; //xpos of cell on grid
    public int gridY; //ypos of cell on grid
    public int width = 1; //width and height of cell in scene
    public string type; //type of cell (entrance, item, door
    public string modelToAssign; //string telling what model to assign to this cell when loading from json
    public string floorToAssign; //string telling what texture to assign to the floor
    public string floorType = "Default";
    public bool hasCeiling = false;
    public string[] walls; //array containing what walls will need to be rendered on this cell (W,E,S,N)
    //public Texture2D floorTexture; //texture rendered on floor
    //public GameObject model; //model associated with cell (can be decorative, door, etc)
    private GameObject cellObject; //rendered model for whole cell
    public bool traversible; //whether the tile can be traversed by players/entities or not
    public DungeonCell()
    {
        //default values are for empty cells
        width = 1;
        type = "Empty";
        modelToAssign = "None";
        floorToAssign = "None";
        traversible = false;
    }

    //methods:
    //getters
    //

    public void createCellObject(GameObject newCell)
    {
        cellObject = newCell;
    }


    public bool hasWall(string dir)
    {
        bool hasWall = false;
        for (int i = 0; i < walls.Length; i++)
        {
            if (walls[i] == dir) hasWall = true;
        }
        return hasWall;
    }

    public bool isTraversible()
    {
        return traversible;
    }


    public void printCell()
    {
        Debug.Log(gridX);
        Debug.Log(gridY);
    }

    //getters
    public Vector2Int getPos()
    {
        return new Vector2Int(gridX, gridY);
    }

    public int getWidth()
    {
        return width;
    }

    public string getType()
    {
        return type;
    }

    public string getModelToAssign()
    {
        return modelToAssign;
    }

    public string getFloorToAssign()
    {
        return floorToAssign;
    }

    public string[] getWalls()
    {
        return walls;
    }




}
