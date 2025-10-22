using UnityEngine;
using UnityEngine.EventSystems;

public class CursorListener : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] string handleType;
    //Detect if the Cursor starts to pass over the GameObject
    public void OnPointerEnter(PointerEventData pointerEventData)
    {
        switch (handleType)
        {
            case "Tray":
                AnimateUI.cursorInsideTray = true;
                break;
        }
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
    }
}
