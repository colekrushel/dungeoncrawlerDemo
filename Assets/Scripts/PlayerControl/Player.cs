using UnityEngine;

public class Player 
{
    //player params
    private Vector2 gridPos;
    public GameObject playerObject;

    public Player(GameObject playObj)
    {
        gridPos = Vector2.zero;
        playerObject = playObj;
    }

    public void teleportPlayer(Vector3 pos)
    {
        playerObject.transform.position = pos;
    }

    public void updatePos(Vector2 newpos)
    {
        gridPos = newpos;
    }

    public Vector2 getPos()
    {
        return gridPos;
    }

    public void printPos()
    {
        Debug.Log(gridPos.x);
        Debug.Log(gridPos.y);
    }

}


