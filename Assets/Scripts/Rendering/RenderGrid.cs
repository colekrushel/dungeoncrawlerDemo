using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System;
using Unity.VisualScripting;

public class RenderGrid : MonoBehaviour
{
    private DungeonGrid[] grids;
    [SerializeField] TextAsset[] gridFiles;
    private void Start()
    {
        gameObject.transform.position = Vector3.zero;
        grids = new DungeonGrid[gridFiles.Length];
        //for each layer
        for (int i = 0; i < gridFiles.Length; i++)
        {
            TextAsset file = gridFiles[i];
            //load rooms
            grids[i] = JsonUtility.FromJson<DungeonGrid>(file.text);
            grids[i].fillGridVariable();
            grids[i].layer = i+1;
            //render cell objects ; cant be rendered in a class function as serializable objects cannot inherit monobehavior to be loaded from json
            GameObject newlayer = new GameObject();
            newlayer.transform.SetParent(gameObject.transform);
            newlayer.name = "layer" + (i + 1);
            renderCellGrid(grids[i], newlayer);
            newlayer.transform.position = gameObject.transform.position + new Vector3(0, i, 0);
        }
        

        
    }

    private void renderCellGrid(DungeonGrid grid, GameObject layer)
    {
        List<List<DungeonCell>> cellGrid = grid.getCellGrid();
        for (int i = 0; i < cellGrid.Count; i++)
        {
            for (int j = 0; j < cellGrid[i].Count; j++)
            {
                //dont render cell if type is empty
                if (cellGrid[i][j].type == "Empty") continue;
                Vector2Int cellPos = cellGrid[i][j].getPos();
                GameObject newCell = renderCell(cellGrid[i][j], cellPos.x, cellPos.y, grid);
                grid.instantiateCellObject(newCell, cellPos.x, cellPos.y);
                newCell.transform.SetParent(layer.transform);
                //cellGridRow.Add(newCell);
            }
        }
    }

    private GameObject renderCell(DungeonCell cell, int cellX, int cellY, DungeonGrid grid)
    {
        //parent container for cell
        GameObject cellObject = new GameObject();
        string[] walls = cell.getWalls();

        //assign floor
        string floorURL = "Prefabs/" + cell.getFloorToAssign();
        GameObject floor = Instantiate(Resources.Load<GameObject>(floorURL));
        floor.transform.parent = cellObject.transform;
        //assign walls

        //assign doorways (between ceiling and non ceiling tiles
        List<String> doorways = new List<String>();
        if (cell.hasCeiling)
        {
            DungeonCell N = grid.getCellInDirection(cell, "N");
            if (N != null && !N.hasCeiling) doorways.Add("N");
            DungeonCell E = grid.getCellInDirection(cell, "E");
            if (E != null && !E.hasCeiling) doorways.Add("E");
            DungeonCell S = grid.getCellInDirection(cell, "S");
            if (S != null && !S.hasCeiling) doorways.Add("S");
            DungeonCell W = grid.getCellInDirection(cell, "W");
            if (W != null && !W.hasCeiling) doorways.Add("W");

        }
        for (int i = 0; i < walls.Length; i++)
        {
            //determine wall based on whether cell has a ceiling or not
            GameObject wall;
            //handle doorways for cells with a ceiling
            
            if (cell.hasCeiling)
            {
                wall = Instantiate(Resources.Load<GameObject>("Prefabs/BuildingWall1"));
            }
            else
            {
                wall = Instantiate(Resources.Load<GameObject>("Prefabs/CellWall"));
            }
            wall.transform.position = floor.transform.position;
            wall.transform.SetParent(cellObject.transform);


            //offset walls to the edge of the tile
            //add or subtract offset of 1/8th of the tile width (1/2 of the post radius)
            float offsetAmt = (float)-cell.getWidth() / 16; ; //indoor wall offset
            if(!cell.hasCeiling) offsetAmt = (float)cell.getWidth() / 8;
            switch (walls[i])
            {
                case "N":
                    wall.transform.Rotate(0, 180, 0); //xyz
                    wall.transform.position += new Vector3(0, 0, offsetAmt);
                    if(cell.hasCeiling) doorways.Remove("N");
                    break;
                case "E":
                    wall.transform.Rotate(0, 270, 0); //xyz
                    wall.transform.position += new Vector3(offsetAmt, 0, 0);
                    if (cell.hasCeiling) doorways.Remove("E");
                    break;
                case "S":
                    wall.transform.Rotate(0, 0, 0); //xyz
                    wall.transform.position += new Vector3(0, 0, offsetAmt * -1);
                    if (cell.hasCeiling) doorways.Remove("S");
                    break;
                case "W":
                    wall.transform.Rotate(0, 90, 0); //xyz
                    wall.transform.position += new Vector3(offsetAmt * -1, 0, 0);
                    if (cell.hasCeiling) doorways.Remove("W");
                    break;
                default:
                    Debug.Log("default case?");
                    break;
            }


        }
        //make doorways
        for (int i = 0; i < doorways.Count; i++)
        {

            GameObject wall = Instantiate(Resources.Load<GameObject>("Prefabs/BuildingDoorway1"));
            wall.transform.position = floor.transform.position;
            wall.transform.SetParent(cellObject.transform);
            //offset walls to the edge of the tile
            //add or subtract offset of 1/8th of the tile width (1/2 of the post radius)
            float offsetAmt = (float)-cell.getWidth() / 16; ; //indoor wall offset

            //only render a doorway if there is a non-ceiling tile in the direction of the doorway
            switch (doorways[i])
            {
                case "N":
                    wall.transform.Rotate(0, 180, 0); //xyz
                    wall.transform.position += new Vector3(0, 0, offsetAmt);
                    break;
                case "E":
                    wall.transform.Rotate(0, 270, 0); //xyz
                    wall.transform.position += new Vector3(offsetAmt, 0, 0);
                    break;
                case "S":
                    wall.transform.Rotate(0, 0, 0); //xyz
                    wall.transform.position += new Vector3(0, 0, offsetAmt * -1);
                    break;
                case "W":
                    wall.transform.Rotate(0, 90, 0); //xyz
                    wall.transform.position += new Vector3(offsetAmt * -1, 0, 0);
                    break;
            }
        }
        //assign ceiling
        if (cell.hasCeiling)
        {
            GameObject ceil = Instantiate(Resources.Load<GameObject>("Prefabs/Ceiling1"));
            ceil.transform.SetParent(cellObject.transform);
            ceil.transform.position = new Vector3(0, 1, 0);
        }
        //models

        //handle models for types (set when saving?
        string modelURL = "Prefabs/" + cell.getModelToAssign();
        if(cell.getModelToAssign() != "None")
        {
            Debug.Log(modelURL);
            GameObject model = Instantiate(Resources.Load<GameObject>(modelURL));
            model.transform.position = floor.transform.position;
            model.transform.SetParent(cellObject.transform);
        }


        //add rendered object to global array so it doesnt have to be loaded again
        cellObject.transform.position = new Vector3(cellX, 0, cellY);
        return cellObject;
    }

    public DungeonGrid getGrid(int layer)
    {
        Debug.Log("getting layer " + layer);
        return grids[layer];
    }

}    


