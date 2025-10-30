using UnityEngine;
using UnityEngine.EventSystems;

public class SkillBox : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler
{
    Skill boxSkill;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnPointerEnter(PointerEventData pointerEventData)
    {
        HandleCursorOverlay.setState(HandleCursorOverlay.cursorState.pointer);
    }
    public void OnPointerExit(PointerEventData pointerEventData)
    {
        HandleCursorOverlay.setState(HandleCursorOverlay.cursorState.none);
    }

    public void OnPointerDown(PointerEventData pointerEventData)
    {
        Debug.Log("clicked on " + this.name);
        //activate skill

    }
}
