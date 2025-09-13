using Unity.VisualScripting;
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
    }
}
