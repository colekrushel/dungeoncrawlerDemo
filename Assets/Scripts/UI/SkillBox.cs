using UnityEngine;
using UnityEngine.EventSystems;

public class SkillBox : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler
{
    Skill boxSkill;
    public bool onCooldown = false;
    
    public void setSkill(Skill skill)
    {
        boxSkill = skill;
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
        //activate skill
        if (!onCooldown)
        {
            //boxSkill.ExecuteSkillEffects();
            HandleSkillBar.activateBox(boxSkill);
            onCooldown = true;
        }
    }
}
