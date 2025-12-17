using UnityEngine;
using UnityEngine.EventSystems;

public class SkillBox : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler
{
    public GameObject boxObject;
    public Skill boxSkill;
    public bool onCooldown = false;
    public bool skillActive = false;
    public bool skillFinished = false;

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

    public void activateSkill()
    {
        if (!onCooldown)
        {
            onCooldown = true;
            skillActive = true;
            skillFinished = false;
            boxSkill.ExecuteSkillEffects();
            HandleSkillBar.activateBox(boxSkill);

            //refresh stats window to reflect skill changes
            HandleSkillTree.fillStatsWindow(Player.playerStats.generateStatString());
        }

    }

    public void OnPointerDown(PointerEventData pointerEventData)
    {
        activateSkill();
    }
}
