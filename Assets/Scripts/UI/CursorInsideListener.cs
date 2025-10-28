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
        }
        HandleCursorOverlay.setState(HandleCursorOverlay.cursorState.none);
    }
}
