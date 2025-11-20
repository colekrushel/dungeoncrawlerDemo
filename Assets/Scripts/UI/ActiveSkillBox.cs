using UnityEngine;

//data class for holding info needed for animating skill boxes
public class ActiveSkillBox 
{
    public GameObject transparencyOverlay;
    public RectTransform transparencyRect;
    public float cooldownTimer;
    public float cooldownLength; //seconds
    public RectTransform timerMask;
    public float skillTimer;
    public float skillLength;
    public SkillBox skillbox;
    public ActiveSkillBox(float ctimer, float clength, float stimer, float slength, RectTransform trect, RectTransform tmask, SkillBox s)
    {
        cooldownLength = clength;
        skillTimer = stimer;
        skillLength = slength;
        cooldownTimer = ctimer;
        transparencyRect = trect;
        timerMask = tmask;
        skillbox = s;

    }
}
