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

    static public string getDirectionBetween(Vector3 p1, Vector3 p2)
    {
        //can return compound directions ie NE or SW
        Vector3 diff = p1 - p2;
        string returnStr = "";
        //x indicates E/W, +x is W, -x is E
        //z indicaes N/S; +Z is S, -z is N
        if (diff.z != 0)
        {
            if (diff.z < 0) returnStr += "N";
            else if (diff.z > 0) returnStr += "S";
        }
        if (diff.x != 0)
        {
            if (diff.x < 0) returnStr += "E";
            else if (diff.x > 0) returnStr += "W";
        }
        return returnStr;
    }

    //static public bool canMoveBetween(Vector2 pos1, Vector2 pos2, char dir, int layer)
    //{
    //    return grids[layer].canMoveBetween(pos1, pos2, dir);
    //}

}