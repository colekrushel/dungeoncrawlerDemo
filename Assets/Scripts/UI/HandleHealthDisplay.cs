using UnityEngine;

public class HandleHealthDisplay : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public static MonoBehaviour Instance;
    public static GameObject currentParent;
    public static GameObject totalParent;
    public static GameObject temporaryParent;


    void Awake()
    {
        Instance = this;
        currentParent = Instance.transform.Find("CurrentHealth").gameObject;
        totalParent = Instance.transform.Find("TotalHealth").gameObject;
        temporaryParent = Instance.transform.Find("TemporaryHealth").gameObject;
    }

    public static void initializeHP()
    {
        //called from player after data loaded

        //player health is an int; each hit point is represented by an individual sprite; hp display is a grid of these sprites.
        //instantiate as many sprites as there are current/total hit points and place them into the canvas grid
        int maxhp = Player.playerStats.getMaxHealth();
        int currenthp = Player.getHP();
        //add total/empty hitpoints
        for (int i = 0; i < maxhp; i++)
        {
            //add empty health objects to the ui
            GameObject ehitpoint = Instantiate<GameObject>(Resources.Load<GameObject>("Prefabs/UI/EmptyHitpoint"));
            ehitpoint.transform.SetParent(totalParent.transform, false);
        }
        //add current/active hitpoints
        for (int i = 0; i < currenthp; i++)
        {
            //add current health objects to the ui
            GameObject chitpoint = Instantiate<GameObject>(Resources.Load<GameObject>("Prefabs/UI/ActiveHitpoint"));
            chitpoint.transform.SetParent(currentParent.transform, false);
        }
    }

    public static void restoreHP()
    {
        //add current hps until they match the amount of total hps
        while (currentParent.transform.childCount < totalParent.transform.childCount)
        {
            GameObject chitpoint = Instantiate<GameObject>(Resources.Load<GameObject>("Prefabs/UI/ActiveHitpoint"));
            chitpoint.transform.SetParent(currentParent.transform, false);
        }
    }


    //public accessor to remove current hp
    public static void takeDamage(int dmg)
    {

        int dtcount = 0;
        int dacount = 0;
        //remove temporary health first
        for (int i = 0; i < dmg; i++)
        {
            
            if (temporaryParent.transform.childCount > dtcount)
            {
                Destroy(temporaryParent.transform.GetChild(dtcount).gameObject);
                dtcount++;
            } else
            {
                if(currentParent.transform.childCount > dacount)
                {
                    Destroy(currentParent.transform.GetChild(dacount).gameObject);
                    dacount++;
                }
                else
                {
                    Debug.Log("player is dead, cant destroy any more hearts");
                    //player dead; dont remove any more hearts
                }
            }
        }
    }

    private static void removeTempHP()
    {
        //clear out temp hp 
        for(int i = temporaryParent.transform.childCount; i >= 0; i--)
        {
            Destroy(temporaryParent.transform.GetChild(i).gameObject);
        }
    }

    public static void addTempHP(int amt)
    {
        for (int i = 0; i < amt; i++)
        {
            //add temp health objects to the ui
            GameObject thitpoint = Instantiate<GameObject>(Resources.Load<GameObject>("Prefabs/UI/TemporaryHitpoint"));
            thitpoint.transform.SetParent(temporaryParent.transform, false);
        }
    }

    public static void addMaxHP(int amt)
    {
        //called when player has an increase in max hp from skill unlock
        //add total/empty hitpoints
        for (int i = 0; i < amt; i++)
        {
            //add empty health objects to the ui
            GameObject ehitpoint = Instantiate<GameObject>(Resources.Load<GameObject>("Prefabs/UI/EmptyHitpoint"));
            ehitpoint.transform.SetParent(totalParent.transform, false);
        }
        //add current/active hitpoints
        for (int i = 0; i < amt; i++)
        {
            //add empty health objects to the ui
            GameObject chitpoint = Instantiate<GameObject>(Resources.Load<GameObject>("Prefabs/UI/ActiveHitpoint"));
            //match idle animation playback time to the rest
            chitpoint.GetComponent<Animator>().Play(0, -1, currentParent.GetComponentInChildren<Animator>().GetCurrentAnimatorStateInfo(0).normalizedTime);
            chitpoint.transform.SetParent(currentParent.transform, false);
        }
    }

    
}
