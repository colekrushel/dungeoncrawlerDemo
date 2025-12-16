using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;

public class HandleDrag : MonoBehaviour, IBeginDragHandler, IDragHandler
{

    GameObject window;
    Vector2 lastPos;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        window = gameObject.transform.parent.gameObject;
        lastPos = window.transform.localPosition;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        //set initial point for drag
        lastPos = eventData.position;
    }

    public void OnDrag(PointerEventData eventData)
    {
        //move window by adding the distance between the current position and the next movement update position
        Vector3 diff = (lastPos - eventData.position);
        window.transform.position -= diff;
        //update drag start point
        lastPos = eventData.position;
        //prevent drag from exiting the screen bounds by checking new position OF CURSOR
#if UNITY_EDITOR
        if (Input.mousePosition.x <= 0 || Input.mousePosition.y <= 0 || Input.mousePosition.x >= Handles.GetMainGameViewSize().x - 1 || Input.mousePosition.y >= Handles.GetMainGameViewSize().y - 1) window.transform.position += diff;
#else
    if (Input.mousePosition.x <= 0 || Input.mousePosition.y <= 0 || Input.mousePosition.x >= Screen.width - 1 || Input.mousePosition.y >= Screen.height - 1) window.transform.position += diff;
#endif
        //if (CountCornersVisibleFrom(this.GetComponent<RectTransform>()) != 4)
        //{
        //    //dont update
        //    Debug.Log("only " + CountCornersVisibleFrom(this.GetComponent<RectTransform>()) + " corners visible!");
        //    //move back
        //    window.transform.position += diff;
        //} 



    }


}
