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
    public int layer;
    public int width = 1; //width and height of cell in scene
    public string type; //type of cell (entrance, item, door
    public string modelToAssign; //string telling what model to assign to this cell when loading from json
    public string modelFacingDir; // N/E/S/W string telling how much to rotate the model by when placed in the scene
    public string propToAssign; //string telling what propScene to place on this cell when rendering
    public string propPlacementOrientation; //direction string (C/Null or N, SW, ect) telling where to place the prop relative to the cell's center point
    public string floorToAssign; //string telling what texture to assign to the floor
    public string floorType = "Default"; //unused ??
    public bool hasCeiling = false; //tells whether to render a ceiling tile or not on this cell
    public string[] walls; //array containing what walls will need to be rendered on this cell (W,E,S,N)
    //public Texture2D floorTexture; //texture rendered on floor
    //public GameObject model; //model associated with cell (can be decorative, door, etc)
    private GameObject cellObject; //rendered model for whole cell
    public bool traversible; //whether the tile can be traversed by players/entities or not
    public CellEntity entity; //entity used to handle interactable cell types (doors, items, levers, ect)
    public BreakableConstruct breakableConstruct = null;
    public string breakableWallDirection; //direction of breakable wall if there is one. only one breakable wall allowed on a cell.
    //when serializing the dungeon json in, the entity will always be serialized as a CellEntity object. this means that to subtype it, we have to assign subtypes manually.
    public string tilesetPath; //path to where the tileset-associated prefabs are stored (walls, floors, doorays, ceilings, props
    public bool useCeilingAsFloor = false; //whether to use the given floor tileset model or the ceiling tileset model as the floor model when above ground layer (basically whether rooftops should use a different tile as floor or not)
    public DungeonCell()
    {
        //default values are for empty cells
        width = 1;
        type = "Empty";
        modelToAssign = "None";
        floorToAssign = "None";
        traversible = false;
        //breakableConstruct = cellObject.AddComponent<BreakableConstruct>();
    }

    //methods:
    //getters
    //
    
    public void createCellObject(GameObject newCell)
    {
        cellObject = newCell;
    }

    public GameObject getCellObject() { 
        return cellObject; 
    }

    public bool hasWall(string dir)
    {
        bool hasWall = false;
        for (int i = 0; i < walls.Length; i++)
        {
            if (walls[i] == dir) hasWall = true;
        }
        //treat areas around stairs that are not enter/exit directions as walls
        if (type == "StairsUp" || type == "StairsDown")
        {
            if (dir != entity.facing && GridUtils.getOppositeDirection(dir) != entity.facing) hasWall = true;
            if (dir == entity.facing || dir == GridUtils.getOppositeDirection(entity.facing)) hasWall = false;
        }
        return hasWall;
    }

    public bool isTraversible()
    {
        //also check if cell has an enemy or player on it
        return (type != "Empty" && traversible && !EnemyManager.enemyOnPos(new Vector2(gridX, gridY), layer) && Player.getPos() != new Vector2(gridX, gridY));
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

    //breakable handling
    public void breakObject(){
        //handle break absed on breakable construct params
        switch (breakableConstruct.btype)
        {
            case BreakableConstruct.breakType.Wall:
                //remove wall
                string[] newwalls = new string[walls.Length-1];
                int wc = 0;
                for(int i = 0; i < walls.Length; i++)
                {
                    if (walls[i] != breakableWallDirection)
                    {
                        newwalls[wc] = walls[i];
                        wc++;
                    }
                   
                }
                walls = newwalls;
                UIUtils.updateMap();
                break;
            case BreakableConstruct.breakType.Item:
                Debug.Log("item broken");
                break;
            case BreakableConstruct.breakType.Door:
                //open door
                entity.interact();
                break;

        }
    }



}
