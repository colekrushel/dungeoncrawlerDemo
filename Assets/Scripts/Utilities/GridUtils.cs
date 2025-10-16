using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements.Experimental;
using static UnityEditor.PlayerSettings;

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
            case -90:
                facing = "W";
                break;
            case 360:
                facing = "N";
                break;
        }
        return facing;
    }

    static public int getDegreesFromDirection(string dir)
    {
        int rdeg = 0;
        switch (dir)
        {
            case "N":
                rdeg = 0;
                break;
            case "E":
                rdeg = 90;
                break;
            case "S":
                rdeg = 180;
                break;
            case "W":
                rdeg = 270;
                break;
        }
        return rdeg;
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

    static public int getBorderRotation(List<string> matchingNeighbors)
    {
        int deg = 0;
        switch (matchingNeighbors.Count)
        {
            case 3:
                //border is on north side; convert dir to deg
                //get dir that isnt present in the neighbors
                string d = "";
                if(!matchingNeighbors.Contains("N")) d = "N";
                else if(!matchingNeighbors.Contains("E")) d = "E";
                else if (!matchingNeighbors.Contains("S")) d = "S";
                else if (!matchingNeighbors.Contains("W")) d = "W";
                deg = getDegreesFromDirection(d);
                break;
            case 2:
                //check if parallel or corner
                if (matchingNeighbors[0] == getOppositeDirection(matchingNeighbors[1]))
                {
                    //parallel; rotate 90 if one of the directions is "N" or "S"
                    if (matchingNeighbors[0] == "E" || matchingNeighbors[0] == "E") deg = 90;
                    
                }
                else
                {

                    //corner; default corner alignment is between "S" and "E"
                    matchingNeighbors.Sort(); //array is sorted so order will always be "E" "N" "S" "W"
                    if (matchingNeighbors[0] == "N" && matchingNeighbors[1] == "W") deg = 180;
                    else if (matchingNeighbors[0] == "S" && matchingNeighbors[1] == "W") deg = 90;
                    else if (matchingNeighbors[0] == "E" && matchingNeighbors[1] == "N") deg = 270;
                }
                break;
            case 1:
                //opening is on south side; rotate by 180 and then convert dir to deg
                deg = getDegreesFromDirection(matchingNeighbors[0]) + 180;
                break;
        }

        return deg;
    }

    public static Vector3 getZoneOffset(string zone)
    {
        Vector3 ret = new Vector3();
        switch (zone)
        {
            case "bottom":
                ret = Vector3.zero;
                break;
            case "west":
                ret = new Vector3 (-4, 14, 0);
                break;
            case "south":
                ret = new Vector3(0, 14, -4);
                break;
        }
        return ret;
    }

    public static Vector3 getZoneNorthVector(string zone)
    {
        Vector3 ret = new Vector3();
        switch (zone)
        {
            case "bottom":
                ret = new Vector3(0, 1, 0);
                break;
            case "north":
                ret = new Vector3(0, 1, 0);
                break;
            case "east":
                ret = new Vector3(0, 0, 1);
                break;
            case "south":
                ret = new Vector3(0, -1, 0);
                break;
            case "west":
                ret = new Vector3(0, 0, 1);
                break;
        }
        return ret;
    }

    public static Vector3 getZoneEastVector(string zone)
    {
        Vector3 ret = new Vector3();
        switch (zone)
        {
            case "bottom":
                ret = new Vector3(1, 0, 0);
                break;
            case "north":
                ret = new Vector3(1, 0, 0);
                break;
            case "east":
                ret = new Vector3(0, 1, 0);
                break;
            case "south":
                ret = new Vector3(1, 0, 0);
                break;
            case "west":
                ret = new Vector3(0, -1, 0);
                break;
        }
        return ret;
    }

    public static string getDirectionOfObjectFacing(GameObject obj, string zone)
    {
        string ret = null;
        Vector3 forwards = obj.transform.forward;
        if (forwards == getZoneNorthVector(zone)) ret = "N";
        else if (forwards == getZoneNorthVector(zone) * -1) ret = "S";
        else if (forwards == getZoneEastVector(zone)) ret = "E";
        else if (forwards == getZoneEastVector(zone) * -1) ret = "W";
        return ret;
    }

    public static Vector2 directionToGridCoords(string direction)
    {
        Vector2 ret = Vector2.zero;
        switch(direction)
        {
            case "S":
                ret = new Vector2(0, -1);
                break;
            case "W":
                ret = new Vector2(-1, 0);
                break;
            case "N":
                ret = new Vector2(0, 1);
                break;
            case "E":
                ret = new Vector2(1, 0);
                break;
        }
        return ret;
    }



    //static public bool canMoveBetween(Vector2 pos1, Vector2 pos2, char dir, int layer)
    //{
    //    return grids[layer].canMoveBetween(pos1, pos2, dir);
    //}

}