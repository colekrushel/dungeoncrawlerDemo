using Unity.VisualScripting;
using UnityEngine;

public static class Player 
{
    //player params
    private static Vector2 gridPos = Vector2.zero;
    public static GameObject playerObject;
    public static int currentLayer = 0;
    public static string facing;


    static public void teleportPlayer(Vector3 pos)
    {
        playerObject.transform.position = pos;
    }

    static public void updatePos(Vector2 newpos)
    {
        gridPos = newpos;

    }

    static public Vector2 getPos()
    {
        return gridPos;
    }

    static public void printPos()
    {
        Debug.Log(gridPos.x);
        Debug.Log(gridPos.y);
    }

    static public void updateFacing()
    {
        //180 && -180 = south (-y)
        //-90 = west (-x)
        //0 = north (+y)
        //90 = east (+x)
        float dir = Player.playerObject.transform.rotation.eulerAngles.y;
        switch (dir)
        {
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
    }

}


