using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;


public static class GridUtils
{
    public static DungeonGrid[] grids;
    public static DungeonGrid[] bottomGrids;
    public static DungeonGrid[] northGrids;
    public static DungeonGrid[] eastGrids;
    public static DungeonGrid[] southGrids;
    public static DungeonGrid[] westGrids;

    public static DungeonCell getCell(int x, int y, int layer)
    {
        return grids[layer].getCell(x, y);
    }
    static public string getOppositeDirection(string direction)
    {
        direction = direction.ToUpper();
        switch (direction)
        {
            case "T": //tutorial
                return "T";
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
            //edge cases
            case -90:
                facing = "W";
                break;
            case 360:
                facing = "N";
                break;
            case 450:
                facing = "E";
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

    static public bool canMoveInDirection(Vector2 pos, int layer, string dir, bool enemy = false)
    {
        bool canMove = false;
        DungeonGrid grid = grids[layer];
        DungeonCell cell = new DungeonCell();
        switch (dir)
        {
            case "S":
                cell = grid.getCell(pos - new Vector2(0, 1));
                canMove = grid.canMoveBetween(pos, pos - new Vector2(0, 1), 'S');
                break;
            case "W":
                cell = grid.getCell(pos - new Vector2(1, 0));
                canMove = grid.canMoveBetween(pos, pos - new Vector2(1, 0), 'W');
                break;
            case "N":
                cell = grid.getCell(pos + new Vector2(0, 1));
                canMove = grid.canMoveBetween(pos, pos + new Vector2(0, 1), 'N');
                break;
            case "E":
                cell = grid.getCell(pos + new Vector2(1, 0));
                canMove = grid.canMoveBetween(pos, pos + new Vector2(1, 0), 'E');
                break;
        }
        if (cell != null && enemy && cell.type == "StairsUp") return false; //prevent enemies from moving onto stairs
        return canMove;
    }

    static public bool canTransportInDirection(Vector2 pos, int layer, string dir)
    {
        bool canMove = false;
        DungeonGrid grid = grids[layer];
        //check if player is moving out of bounds and there is no wall blocking them to indicate a valid transportation (transfer between zones)
        switch (dir)
        {
            case "S":
                canMove = !grid.getCell(pos).walls.Contains(dir) && grid.cellOutOfBounds(new Vector2Int((int)pos.x, (int)pos.y) - new Vector2Int(0, 1));
                break;
            case "W":
                canMove = !grid.getCell(pos).walls.Contains(dir) && grid.cellOutOfBounds(new Vector2Int((int)pos.x, (int)pos.y) - new Vector2Int(1, 0));
                break;
            case "N":
                canMove = !grid.getCell(pos).walls.Contains(dir) && grid.cellOutOfBounds(new Vector2Int((int)pos.x, (int)pos.y) + new Vector2Int(0, 1));
                break;
            case "E":
                canMove = !grid.getCell(pos).walls.Contains(dir) && grid.cellOutOfBounds(new Vector2Int((int)pos.x, (int)pos.y) + new Vector2Int(1, 0));
                break;
        }
        return canMove;
    }

    static public string getDirectionBetween(Vector2 p1, Vector2 p2)
    {
        //can return compound directions ie NE or SW
        Vector2 diff = p1 - p2;
        string returnStr = "";
        //use grid coordinates; +y is north
        if (diff.y != 0)
        {
            if (diff.y < 0) returnStr += "N";
            else if (diff.y > 0) returnStr += "S";
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
                ret = new Vector3 (-4, 19.5f, 0);
                break;
            case "south":
                ret = new Vector3(0, 19.5f, -4);
                break;
            case "east":
                ret = new Vector3(23, .5f, 0);
                break;
            case "north":
                ret = new Vector3(0, .5f, 23);
                break;
            case "tutorial":
                ret = new Vector3(0f, -50f, 0);
                break;

        }
        return ret;
    }

    public static Vector3 getZoneNorthVector(string zone)
    {
        Vector3 ret = new Vector3();
        switch (zone)
        {
            case "tutorial":
            case "bottom":
                ret = new Vector3(0, 0, 1);
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
            case "tutorial":
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

    public static Vector3 getZoneUpVector(string zone)
    {
        Vector3 ret = new Vector3();
        switch (zone)
        {
            case "tutorial":
            case "bottom":
                ret = new Vector3(0, 1, 0);
                break;
            case "north":
                ret = new Vector3(0, 0, -1);
                break;
            case "east":
                ret = new Vector3(-1, 0, 0);
                break;
            case "south":
                ret = new Vector3(0, 0, 1);
                break;
            case "west":
                ret = new Vector3(1, 0, 0);
                break;
        }
        return ret;
    }

    public static Vector3 getZoneRotationEuler(string zone)
    {
        Vector3 ret = Vector3.zero;
        switch (zone)
        {
            case "tutorial":
            case "bottom":
                ret = new Vector3(0, 0, 0);
                break;
            case "north":
                ret = new Vector3(-90, 0, 0);
                break;
            case "east":
                ret = new Vector3(0, 0, 90);
                break;
            case "south":
                ret = new Vector3(90, 0, 0);
                break;
            case "west":
                ret = new Vector3(0, 0, -90);
                break;
        }
        return ret;
    }

    public static string getDirectionOfObjectFacing(GameObject obj, string zone)
    {
        string ret = null;
        Vector3 forwards = obj.transform.forward;
        //Debug.Log("forwards v: " + forwards);
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

    public static string getTransportDestinationZone(Vector2 pos, string dir, string currzone)
    {
        string destZone = null;
        switch (currzone)
        {
            case "tutorial":
                destZone = "bottom";
                break;
            case "bottom":
                switch (dir)
                {
                    case "N":
                        destZone = "north";
                        break;
                    case "E":
                        destZone = "east";
                        break;
                    case "S":
                        destZone = "south";
                        break;
                    case "W":
                        destZone = "west";
                        break;
                }

                break;
            case "north":
                switch (dir)
                {
                    case "N":
                        destZone = null;
                        break;
                    case "E":
                        destZone = "east";
                        break;
                    case "S":
                        destZone = "bottom";
                        break;
                    case "W":
                        destZone = "west";
                        break;
                }
                break;
            case "east":
                switch (dir)
                {
                    case "N":
                        destZone = "north";
                        break;
                    case "W":
                        destZone = "bottom";
                        break;
                    case "S":
                        destZone = "south";
                        break;
                    case "E":
                        destZone = null;
                        break;
                }
                break;
            case "south":
                switch (dir)
                {
                    case "N":
                        destZone = "bottom";
                        break;
                    case "E":
                        destZone = "east";
                        break;
                    case "S":
                        destZone = null;
                        break;
                    case "W":
                        destZone = "west";
                        break;
                }
                break;
            case "west":
                switch (dir)
                {
                    case "N":
                        destZone = "north";
                        break;
                    case "E":
                        destZone = "bottom";
                        break;
                    case "S":
                        destZone = "south";
                        break;
                    case "W":
                        destZone = null;
                        break;
                }
                break;
        }
        return destZone;
    }

    public static Vector2 getTransportDestinationCoord(Vector2 pos, string dir, string currzone)
    {
        string destZone = getTransportDestinationZone(pos, dir, currzone);
        //possible operations when changing grid orientation
        //keep one coord and inverse the other
        bool keepX = false;
        bool keepY = false;
        //swap resulting x or y with initial x or y
        //set result x or y to 0 or width/height -1

        //determine the new coord to set the player to
        Vector2 newcoord = Vector2.zero;
        switch (currzone)
        {
            case "tutorial": //hardcode tutorial arrival coord
                newcoord.x = 1;
                newcoord.y = 1;
                break;
            case "bottom":
                switch (destZone)
                {
                    case "north":
                        keepX = true;
                        keepY = false;
                        break;
                    case "east":
                        keepX = false;
                        keepY = true;
                        break;
                    case "south":
                        keepX = true;
                        keepY = false;
                        break;
                    case "west":
                        keepX = false;
                        keepY = true;
                        break;
                }
                if (keepX) newcoord.x = pos.x;
                else newcoord.x = (Mathf.Abs((grids[0].width - 1) - pos.x));
                if (keepY) newcoord.y = pos.y;
                else newcoord.y = (Mathf.Abs((grids[0].height - 1) - pos.y));
                break;
            case "north":
                switch (destZone)
                {
                    case "east":
                        newcoord.x = pos.y;
                        newcoord.y = pos.x;
                        break;
                    case "bottom":
                        newcoord.x = pos.x;
                        newcoord.y = (Mathf.Abs((grids[0].height - 1) - pos.y));
                        break;
                    case "west":
                        newcoord.x = (Mathf.Abs((grids[0].height - 1) - pos.y));
                        newcoord.y = grids[0].height - 1;
                        break;
                }
                break;
            case "east":
                switch (destZone)
                {
                    case "north":
                        newcoord.x = pos.y;
                        newcoord.y = pos.x;
                        break;
                    case "bottom":
                        newcoord.x = (Mathf.Abs((grids[0].width - 1) - pos.x));
                        newcoord.y = pos.y;
                        break;
                    case "south":
                        newcoord.x = grids[0].width - 1;
                        newcoord.y = (Mathf.Abs((grids[0].width - 1) - pos.x));
                        break;
                }
                break;
            case "south":
                switch (destZone)
                {
                    case "east":
                        newcoord.x = (Mathf.Abs((grids[0].height - 1) - pos.y));
                        newcoord.y = 0;
                        break;
                    case "bottom":
                        newcoord.x = pos.x;
                        newcoord.y = (Mathf.Abs((grids[0].height - 1) - pos.y));
                        break;
                    case "west":
                        newcoord.x = pos.y;
                        newcoord.y = 0;
                        break;
                }
                break;
            case "west":
                switch (destZone)
                {
                    case "north":
                        newcoord.y = (Mathf.Abs((grids[0].width - 1) - pos.x));
                        newcoord.x = 0;
                        break;
                    case "bottom":
                        newcoord.x = (Mathf.Abs((grids[0].width - 1) - pos.x));
                        newcoord.y = pos.y;
                        break;
                    case "south":
                        newcoord.x = 0;
                        newcoord.y = pos.x;
                        break;
                }
                break;
        }
        return newcoord;
    }

    public static Vector3 getTransportDestinationWorldpos(Vector2 pos, string dir, string currzone)
    {
        //map each zone's edge to another zone's edge
        //assume dir is the direction the entity is exiting the zone from
        
        Vector3 ret = Vector3.zero;
        DungeonGrid[] destGrids = grids;
        //when player is transporting to a zone, they will conserve x or y and reverse the other coord
        string destZone = getTransportDestinationZone(pos, dir, currzone);

        //and put them on the highest traversible tile
   
        //convert coord and orientation into world pos
        Vector2 newcoord = getTransportDestinationCoord(pos, dir, currzone);
        int layer = getHighestTraversibleLayer(newcoord, destZone);
        ret = coordToWorld(newcoord, destZone, layer);
        return ret;
    }

    public static int getHighestTraversibleLayer(Vector2 pos, string zone)
    {
        int layer = 0;
        for (int i = grids.Length - 1; i > 0; i--)
        {
            if (grids[i].getCell(pos).isTraversible())
            {
                layer = i;
                break;
            }
        }
        return layer;
    }

    public static Vector3 coordToWorld(Vector2 coord, string zone, int layer = 0)
    {
        //x is right vector; y is forward vector; layer is up vector
        Vector3 ret = getZoneOffset(zone) + coord.x * getZoneEastVector(zone) + coord.y * getZoneNorthVector(zone) + getZoneUpVector(zone) * layer;
        return ret;
    }

    public static void switchZone(string newZone)
    {
        //Player.orientation = newZone;
        switch (newZone)
        {

            case "bottom":
                grids = bottomGrids;
                break;
            case "north":
                grids = northGrids;
                break;
            case "east":
                grids = eastGrids;
                break;
            case "south":
                grids = southGrids;
                break;
            case "west":
                grids = westGrids;
                break;

        }
    }

    public static Quaternion getTransportDestinationQuaternion(string zone, string direction)
    {
        Quaternion ret = Quaternion.identity;
        Quaternion northQuat = Quaternion.Euler(getZoneRotationEuler(zone));

        //and rotating it accordingly 
        switch (direction.ToUpper())
        {
            case "T": //tutorial
            case "N":
                ret = northQuat;
                break;
            case "E":
                //rotate 90
                ret = Quaternion.AngleAxis(90f, getZoneUpVector(zone)) * northQuat;
                break;
            case "S":
                //rotate 180
                ret = Quaternion.AngleAxis(180f, getZoneUpVector(zone)) * northQuat;
                break;
            case "W":
                //rotate -90
                ret = Quaternion.AngleAxis(-90f, getZoneUpVector(zone)) * northQuat;
                break;
            
        }
        return ret;
    }

    public static float getDegBetweenDirections(string currentDir, string targetDir)
    {
        float ret = 0;
        //handle opposite dirs and return 90 or -90 randomly
        if (Mathf.Abs(getDegreesFromDirection(currentDir) - getDegreesFromDirection(targetDir)) / 180 == 1)
        {
            if(Random.Range(1,3) == 1)
            {
                ret = 90f;
            }
            else
            {
                ret = -90f;
            }
            
        }

        
        switch (currentDir)
        {
            case "N":
                switch (targetDir)
                {
                    case "E":
                        ret = 90f;
                        break;
                    case "W":
                        ret = -90f;
                        break;
                }
                break;
            case "E":
                switch (targetDir)
                {
                    case "N":
                        ret = -90f;
                        break;
                    case "S":
                        ret = 90f;
                        break;
                }
                break;
            case "S":
                switch (targetDir)
                {
                    case "E":
                        ret = -90f;
                        break;
                    case "W":
                        ret = 90f;
                        break;
                }
                break;
            case "W":
                switch (targetDir)
                {
                    case "N":
                        ret = 90f;
                        break;
                    case "S":
                        ret = -90f;
                        break;
                }
                break;
        }
        return ret;
    }

    public static Vector3 DirStringToWorldOffset(string dirString, string zone)
    {
        //takes a direction and returns an offset (based on center of a cell) in world pos; used for placing props on edges of cell
        /*MODIFIERS - symbols that effect the next direction character; used for more specific orienting
         * : - halves the next offset
         */
        Vector3 ret = Vector3.zero;
        float modifier = 1f; //modifier resets after each letter
        char[] chars = dirString.ToLower().ToCharArray();
        foreach (char c in chars)
        {
            switch (c)
            {
                case 'n':
                    ret += getZoneNorthVector(zone) * .5f * modifier;
                    modifier = 1f;
                    break;
                case 's':
                    ret -= getZoneNorthVector(zone) * .5f * modifier;
                    modifier = 1f;
                    break;
                case 'e':
                    ret += getZoneEastVector(zone) * .5f * modifier;
                    modifier = 1f;
                    break;
                case 'w':
                    ret -= getZoneEastVector(zone) * .5f * modifier;
                    modifier = 1f;
                    break;
                case ':':
                    modifier *= .5f;
                break;
                //IGNORE MODIFIERS FOR ROTATION SETTING
                case '/':
                    modifier = 0f;
                    break;
            }
        }
        return ret;
    }
    public static void RotatePropToDirection(string dirString, string zone, GameObject prop)
    {
        //perop's local right vector is always in the 'E' direction; rotate accordingly

        Quaternion ret = Quaternion.identity;
        float modifier = 0f; //modifier only appleis after a rotation char is detected (/)
        char[] chars = dirString.ToLower().ToCharArray();
        foreach (char c in chars)
        {
            switch (c)
            {
                case 'n':
                    ret = Quaternion.AngleAxis(-90f * modifier, prop.transform.up) * prop.transform.rotation;
                    modifier = 0f;
                    break;
                case 's':
                    ret = Quaternion.AngleAxis(90f * modifier, prop.transform.up) * prop.transform.rotation;

                    modifier = 0f;
                    break;
                case 'e':
                    modifier = 0f;
                    break;
                case 'w':
                    ret = Quaternion.AngleAxis(180f * modifier, prop.transform.up) * prop.transform.rotation;
                    modifier = 0f;
                    break;
                //IGNORE MODIFIERS FOR ROTATION SETTING
                case '/':
                    modifier = 1f;
                    break;
            }
        }
        prop.transform.rotation = ret;
    }





}