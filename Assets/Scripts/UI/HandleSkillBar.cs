using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HandleSkillBar : MonoBehaviour
{
    public static MonoBehaviour Instance { get; private set; }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public static List<Skill> addedSkills = new List<Skill>();
    //to animate the timer for a skill's cooldown, we need the transparent cooldown overlay object, a timer, and finishTime for each skill

    //to animate the timer for a skill's effect, we need the timer mask object, a timer, and finishTime for each skill

    public static List<ActiveSkillBox> activeSkillBoxes= new List<ActiveSkillBox>();

    [SerializeField] public static int boxHeight = 120;
    //offsets for boxes cause grid rect messes up recttransform anchors
    public static float xOffset;
    public static float yOffset;
    public static bool paused = false;


    public void Awake()
    {
        Instance = this;
    }
    public static void reloadSkillBoxes()
    {
        //when opened, fill the ui grid component with all of the buff skills that the player has
        foreach (Skill skill in Player.skills)
        {
            if (addedSkills.Contains(skill)) continue;
            foreach(ISkillEffect skeffect in skill.GetSkillEffects())
            {
                bool stop = false;
                if(skeffect is BuffEffect)
                {
                    //add a skillbox to the ui with the appropriate data
                    if (stop) continue;
                    addBox(skill);
                    stop = true;
                }
            }
        }
    }

    public static void addBox(Skill s)
    {
        GameObject sbox = Instantiate(Resources.Load<GameObject>("Prefabs/UI/SkillBox"));
        sbox.transform.Find("Icon").GetComponent<Image>().sprite = s.icon;
        sbox.transform.Find("Name").GetComponent<TextMeshProUGUI>().text = s.name;
        sbox.GetComponent<SkillBox>().setSkill(s);
        sbox.name = s.name; //match skillname with box name; skill names must be unique
        sbox.transform.SetParent(Instance.transform, false);
        addedSkills.Add(s);
    }

    public static void activateBox(Skill s)
    {
        //fetch box
        GameObject sbox = Instance.transform.Find(s.name).gameObject;
        xOffset = sbox.GetComponent<RectTransform>().anchoredPosition.x;
        yOffset = sbox.GetComponent<RectTransform>().anchoredPosition.y;
        //fetch transparency object
        sbox.transform.Find("CooldownOverlay").gameObject.SetActive(true);
        RectTransform transparency = sbox.transform.Find("CooldownOverlay").GetComponent<RectTransform>();

        //assume all buffs are timer based for now 
        RectTransform timer = sbox.transform.Find("Timer").Find("TimerMask").GetComponent<RectTransform>();
        ActiveSkillBox activesbox = new ActiveSkillBox(0, s.cooldown, 0, s.time, transparency, timer, sbox.GetComponent<SkillBox>());

        //set initial positions of rects
        transparency.offsetMin = new Vector2(transparency.offsetMin.x, (-1*boxHeight/2) - yOffset); //bottom
        transparency.offsetMax = new Vector2(transparency.offsetMax.x, (boxHeight/2) + yOffset); //top
        activeSkillBoxes.Add(activesbox);
    }


    // Update is called once per frame
    void Update()
    {
        if(paused) return;
        foreach (ActiveSkillBox box in activeSkillBoxes)
        {
            //handle cooldown/transparency layer
            //only when appropriate
            if(box.skillbox.onCooldown && box.cooldownTimer <= boxHeight) //timer is in seconds; to get timer as a 
            {
                box.cooldownTimer += (120 / box.cooldownLength) * Time.deltaTime;
                box.transparencyRect.offsetMin = new Vector2(box.transparencyRect.offsetMin.x, box.cooldownTimer - boxHeight + 60 - yOffset);
            } else
            {
                //end animation and reset params
                box.skillbox.onCooldown = false;
                box.cooldownTimer = 0;
                //when cooldown is finished, remove this box from the update loop because it has done its job
                activeSkillBoxes.Remove(box);
                break;
            }

            //handle timer 
            if(box.skillbox.skillActive && box.skillTimer <= boxHeight)
            {
                box.skillTimer += (120 / box.skillLength) * Time.deltaTime;
                //assuming boxheight is same as the width of timer bar
                box.timerMask.offsetMax = new Vector2((boxHeight - box.skillTimer - boxHeight), box.timerMask.offsetMax.y);
            } else if(!box.skillbox.skillFinished)
            {
                //skill is over; reset ui and stats
                box.skillTimer = 0;
                box.skillbox.skillActive = false;
                Player.playerStats.removeBuffs(box.skillbox.boxSkill);
                box.skillbox.skillFinished = true;
                HandleSkillTree.fillStatsWindow(Player.playerStats.generateStatString());
            }

        }


    }
}
