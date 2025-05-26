using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System;

public class RenderGrid : MonoBehaviour
{
    private DungeonGrid grid;
    [SerializeField] TextAsset gridFile;
    private void Start()
    {
        gameObject.transform.position = Vector3.zero;
        //load in grid from json
        //load rooms
        grid = JsonUtility.FromJson<DungeonGrid>(gridFile.text);
        grid.fillGridVariable();

        //render cell objects ; cant be rendered in a class function as serializable objects cannot inherit monobehavior to be loaded from json
        renderCellGrid();
        
        
       

        
    }

    private void renderCellGrid()
    {
        List<List<DungeonCell>> cellGrid = grid.getCellGrid();
        for (int i = 0; i < cellGrid.Count; i++)
        {
            for (int j = 0; j < cellGrid[i].Count; j++)
            {
                Vector2Int cellPos = cellGrid[i][j].getPos();
                GameObject newCell = renderCell(cellGrid[i][j], cellPos.x, cellPos.y);
                grid.instantiateCellObject(newCell, cellPos.x, cellPos.y);
                newCell.transform.SetParent(gameObject.transform);
                //cellGridRow.Add(newCell);
            }
        }
    }

    private GameObject renderCell(DungeonCell cell, int cellX, int cellY)
    {
        //parent container for cell
        GameObject cellObject = new GameObject();
        string[] walls = cell.getWalls();

        //assign floor
        string floorURL = "Prefabs/" + cell.getFloorToAssign();
        GameObject floor = Instantiate(Resources.Load<GameObject>(floorURL));
        floor.transform.parent = cellObject.transform;
        //assign walls
        for (int i = 0; i < walls.Length; i++)
        {
            GameObject wall = Instantiate(Resources.Load<GameObject>("Prefabs/CellWall"));
            wall.transform.position = floor.transform.position;
            wall.transform.SetParent(cellObject.transform);
            //offset walls to the edge of the tile
            //add or subtract offset of 1/8th of the tile width (1/2 of the post radius)
            float offsetAmt = (float) cell.getWidth() / 8;
            switch (walls[i])
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
                default:
                    Debug.Log("default case?");
                    break;
            }
            
            
        }
        //models
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

    public DungeonGrid getGrid()
    {
        Debug.Log("getting grid");
        return grid;
    }

}    


