using UnityEngine;
using UnityEngine.UI;

public class HandleTray : MonoBehaviour
{

    private void Start()
    {
        closeWindow("Log");
    }

    public void closeWindow(string windowName) {
        //unactivate corresponding icon
        GameObject icon = gameObject.transform.Find("Icons").transform.Find(windowName + "Icon").gameObject;
        if (icon == null) return;
        GameObject background = icon.transform.Find("BG").gameObject;
        StartCoroutine(UIUtils.fadeObject(background, false, (float) 0.1));
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
    }
}
