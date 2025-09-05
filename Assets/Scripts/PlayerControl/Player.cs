using UnityEngine;

public static class Player 
{
    //player params
    private static Vector2 gridPos = Vector2.zero;
    public static GameObject playerObject;
    public static int currentLayer = 0;


    static public void teleportPlayer(Vector3 pos)
    {
        playerObject.transform.position = pos;
    }

    static public void updatePos(Vector2 newpos)
    {
        gridPos = newpos;
        Debug.Log("player moved to ");
        Debug.Log(gridPos);
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

}


