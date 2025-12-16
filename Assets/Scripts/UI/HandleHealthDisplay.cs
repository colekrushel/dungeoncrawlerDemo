using UnityEngine;

public class HandleHealthDisplay : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public static MonoBehaviour Instance;
    void Start()
    {
        Instance = this;
    }

    public static void initializeHP()
    {
        //called from player after data loaded

        //player health is an int; each hit point is represented by an individual sprite; hp display is a grid of these sprites.
        //instantiate as many sprites as there are current/total hit points and place them into the canvas grid
    }

    public static void subtractHP()
    {
        //called when player is hit

        //check for temphp first
    }

    public static void addTempHP()
    {

    }

    
}
