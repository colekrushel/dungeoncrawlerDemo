using UnityEngine;
using UnityEngine.EventSystems;

public class CursorListener : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] string handleType;
    //Detect if the Cursor starts to pass over the GameObject
    [SerializeField] HandleCursorOverlay.cursorState cursorStateOnHover;
    public void OnPointerEnter(PointerEventData pointerEventData)
    {
        switch (handleType)
        {
            case "Tray":
                AnimateUI.cursorInsideTray = true;
                break;
            case "DesktopIcon":
                //for desktop icons we want to display the background on hover
                //this.transform.Find("BackgroundOverlay").gameObject.SetActive(true); //assume background is a child of this object
                StartCoroutine(UIUtils.fadeObject(this.transform.Find("BackgroundOverlay").gameObject, true, .2f));
                break;
        }
        HandleCursorOverlay.setState(cursorStateOnHover);
    }

    //Detect when Cursor leaves the GameObject
    public void OnPointerExit(PointerEventData pointerEventData)
    {
        switch (handleType)
        {
            case "Tray":
                AnimateUI.cursorInsideTray = false;
                break;
            case "DesktopIcon":
                //this.transform.Find("BackgroundOverlay").gameObject.SetActive(true); //assume background is a child of this object
                StartCoroutine(UIUtils.fadeObject(this.transform.Find("BackgroundOverlay").gameObject, false, .2f));
                break;
        }
        HandleCursorOverlay.setState(HandleCursorOverlay.cursorState.none);
    }
}
