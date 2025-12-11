using UnityEngine;
using UnityEngine.UI;

public class HandleTray : MonoBehaviour
{
    public static MonoBehaviour Instance { get; private set; }

    [SerializeField] GameObject appStoreContainer;
    [SerializeField] static GameObject desktopContainer;
    [SerializeField] int highestUIIndex; //index ui components should be set to to appear as the highest sibling but without blocking cursor/tray

    private void Awake()
    {
        Instance = this;
        desktopContainer = GameObject.Find("DesktopFull").gameObject;
    }
    private void Start()
    {
        closeWindow("Log");
        closeWindow("Weapons");
        closeWindow("SkillTree");
        closeWindow("SkillBar");
        closeWindow("Map");
        closeWindow("AppStore");
        //start the game paused/on the desktop
        desktopTransition(true);
        
    }

    public void closeWindow(string windowName) {
        //unactivate corresponding icon
        GameObject icon = gameObject.transform.Find("Icons").transform.Find(windowName + "Icon").gameObject;
        if (icon == null) return;
        GameObject background = icon.transform.Find("BG").gameObject;
        StartCoroutine(UIUtils.fadeObject(background, false, (float)0.1));
        //disable corresponding window
        GameObject window = GameObject.Find("PlayerUI").transform.Find(windowName).gameObject;
        StartCoroutine(UIUtils.fadeObject(window, false, (float)0.1));
    }

    public void openWindow(string windowName)
    {
        GameObject icon = gameObject.transform.Find("Icons").transform.Find(windowName + "Icon").gameObject;
        GameObject window = GameObject.Find("PlayerUI").transform.Find(windowName).gameObject;
        if (icon == null) return;
        //dont do anything if already enabled
        if (window.GetComponent<CanvasGroup>().alpha != 0) return;
        //activate corresponding icon
        GameObject background = icon.transform.Find("BG").gameObject;
        StartCoroutine(UIUtils.fadeObject(background, true, (float)0.1));
        //re-enable corresponding window
        window.GetComponent<Canvas>().enabled = true;
        StartCoroutine(UIUtils.fadeObject(window, true, (float)0.1));
        //move tray to top
        window.transform.SetSiblingIndex(highestUIIndex);
    }

    public void purchaseToTray(string windowName)
    {
        //if statement to get price because they're hardcoded anyway and can only pass 1 var with onClick
        int price = 10000000;
        if (windowName == "Map") price = 100;
        else if (windowName == "Weapons") price = 400;
        else if (windowName == "SkillTree") price = 100;
        else if (windowName == "SkillBar") price = 100;
        //first check if can afford
        if (price < Player.getCurrency())
        {
            Player.addCurrency(price * -1);
            GameObject icon = gameObject.transform.Find("Icons").transform.Find(windowName + "Icon").gameObject;
            icon.SetActive(true);
            //enable desktop icon too
            GameObject desktopIcon = desktopContainer.transform.Find(windowName + "Icon").gameObject;
            desktopIcon.SetActive(true);
            //after purchasing the app, remove the associated entry from the store
            GameObject entry = appStoreContainer.transform.Find(windowName + "Entry").gameObject;
            Destroy(entry);
        }
        else
        {
            return;
        }
    }

    public static void desktopTransition(bool openDesktop)
    {
        //reset cursor status to normal when transitioning because cursor state only resets when leaving the element, and the transition doesnt satisfy this for whatever reason
        HandleCursorOverlay.setState(HandleCursorOverlay.cursorState.none);
        //when returning to the desktop, the game should be paused; unpause when opening game
        if (openDesktop)
        {
            desktopContainer.SetActive(true);
            //pause player input
            Player.inputLock = true;
            //pause enemies
            EnemyManager.handlePauseEnemies(true);
            //pause other things?
            HandleSkillBar.paused = true;
        }
        else
        {
            desktopContainer.SetActive(false);
            //unpause player input
            Player.inputLock = false;
            //unpause enemies
            EnemyManager.handlePauseEnemies(false);
            //unpause other things?
            HandleSkillBar.paused = false;
        }

    }

   
}
