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
}