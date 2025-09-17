using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements.Experimental;

public static class GridUtils
{
    public static DungeonGrid[] grids;

    public static DungeonCell getCell(int x, int y, int layer)
    {
        return grids[layer].getCell(x, y);
    }
    static public string getOppositeDirection(string direction)
    {
        direction = direction.ToUpper();
        switch (direction)
        {
            case "N":
                return "S";
            case "E":
                return "W";
            case "S":
                return "N";
            case "W":
                return "E";
            default:
                return null;
                
        }
        
    }

    static public string getDirectionFromDegrees(int deg)
    {
        string facing = "NA";
        if (deg == -90) deg = 270; //exceptions for offsets
        if (deg == 360) deg = 0;
        switch (deg)
        {
            default:
                Debug.LogError("INVALID DIRECTION NUM SENT TO UTILS: " + deg);
                break;
            case 180:
                facing = "S";
                break;
            case 270:
                facing = "W";
                break;
            case 0:
                facing = "N";
                break;
            case 90:
                facing = "E";
                break;
        }
        return facing;
    }

    static public bool canMoveInDirection(Vector2 pos, int layer, string dir)
    {
        bool canMove = false;
        DungeonGrid grid = grids[layer];
        switch (dir)
        {
            case "S":
                canMove = grid.canMoveBetween(pos, pos - new Vector2(0, 1), 'S');
                break;
            case "W":
                canMove = grid.canMoveBetween(pos, pos - new Vector2(1, 0), 'W');
                break;
            case "N":                
                canMove = grid.canMoveBetween(pos, pos + new Vector2(0, 1), 'N');
                break;
            case "E":
                canMove = grid.canMoveBetween(pos, pos + new Vector2(1, 0), 'E');
                break;
        }
        return canMove;
    }
}