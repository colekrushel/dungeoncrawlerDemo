using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using Unity.VisualScripting;
using System;

[Serializable]
public class DungeonGrid 
{
    //attributes
    
    public DungeonCell[] cells;
    //public CellRow[] cells;
    public int width;
    public int height;
    private List<List<DungeonCell>> cellGrid; //does public/private matter for json serializing?
    public DungeonGrid()
    {
        cellGrid = new List<List<DungeonCell>>();
    }

    //methods:
    //getCell(x, y)
    //fillGridVariable() json data is loaded in as a 1D array so this function converts it into a 2D array format
    //getCellGrid() getter

    public DungeonCell getCell(int x, int y)
    {
        return cellGrid[y][x];
    }

    public void printGrid()
    {
        Debug.Log(width);
        Debug.Log(height);
        Debug.Log(cellGrid.Count);
        Debug.Log(cellGrid[0].Count);
    }


    /*
     * POTENTIAL WAYS TO REPRESENT GRID
     * - INCLUDE X AND Y VALUES AND GIVEN WIDTH AND HEIGHT, DETERMINE ALL SPOTS WITHOUT X AND Y TO BE 'EMPTY' TILES - WOULD NOT SAVE MUCH DATA, NOT GOOD
     * - DISREGARD X AND Y VALUES IN JSON, THEY ARE LISTED IN ASCENDING ORDER (0,0, 0,1, ... 0,WIDTH-1, 1,0 ... )
     */
    public void fillGridVariable()
    {
        //Debug.Log(cells.Length);
        //fill 2D array from 1D json cell data
        int colCount = 0;
        int rowCount = 0;

        //create empty rows
        
        for (int i = 0; i < height; i++)
        {
            List<DungeonCell> trow = new List<DungeonCell>();
            cellGrid.Add(trow);
        }
       

        //fill empty rows one at a time
        for (int i = 0; i < cells.Length; i++)
        {
            cellGrid[rowCount].Add(cells[i]);
            colCount++;
            //Debug.Log(colCount);
            //Debug.Log(rowCount);
            
            //increment row
            if (colCount >= width)
            {
                rowCount++;
                colCount = 0;
            }
        }
    }

    public void instantiateCellObject(GameObject cellObject, int x, int y)
    {
        DungeonCell currCell = cellGrid[y][x];
        currCell.createCellObject(cellObject);
    }

    public bool canMoveBetween(Vector2 pos1, Vector2 pos2, char dir)
    {
        //2 grid coordinates as inpute
        //check if there are no walls between the 2 coords
        bool canMove = false;
        DungeonCell cell1 = getCell((int)pos1.x, (int)pos1.y);
        DungeonCell cell2;
        switch (dir)
        {
            case 'N':
                //check bounds
                if (pos2.y >= height) return false;

                //check if walls in the way
                cell2 = getCell((int)pos2.x, (int)pos2.y);
                if (!cell2.isTraversible() || cell1.hasWall("N") || cell2.hasWall("S"))
                {
                    return false;
                } else
                {
                    canMove = true;
                }
                break;
            case 'E':
                //check bounds
                if (pos2.x >= width) return false;

                //check if walls in the way
                cell2 = getCell((int)pos2.x, (int)pos2.y);

                if (!cell2.isTraversible() || cell1.hasWall("E") || cell2.hasWall("W"))
                {
                    return false;
                }
                else
                {
                    canMove = true;
                }
                break;
            case 'W':
                //check bounds
                if (pos2.x < 0) return false;

                //check if walls in the way
                cell2 = getCell((int)pos2.x, (int)pos2.y);
                if (!cell2.isTraversible() || cell1.hasWall("W") || cell2.hasWall("E"))
                {
                    return false;
                }
                else
                {
                    canMove = true;
                }
                break;
            case 'S':
                //check bounds
                if (pos2.y < 0) return false;

                //check if walls in the way
                cell2 = getCell((int)pos2.x, (int)pos2.y);
                if (!cell2.isTraversible() || cell1.hasWall("S") || cell2.hasWall("N"))
                {
                    return false;
                }
                else
                {
                    canMove = true;
                }
                break;
        }
        return canMove;
    }

    public List<List<DungeonCell>> getCellGrid()
    {
        return cellGrid;
    }
}
