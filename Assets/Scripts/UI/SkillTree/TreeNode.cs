using NUnit.Framework;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

//data class for a node on the skill tree

public class TreeNode : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler
{

    [SerializeField]
    public List<TreeNode> requirements; //required connections to grant access to node
    [SerializeField] public Skill nodeSkill; //skill granted when requirements are met
    public bool unlocked = false;

    //ui display handling
    private GameObject background;
    private GameObject icon;
    private GameObject border;
    public GameObject descriptionWindow;
    public HoverMe descHover;
    //grab description components at wake so they dont have to be grabbed multiple times
    private TextMeshProUGUI dname;
    private TextMeshProUGUI ddesc;
    private GameObject dstats;
    private TextMeshProUGUI dprice;
    [SerializeField] GameObject attribute;

    public void Awake()
    {
        background = transform.Find("BCKG").gameObject;
        icon = transform.Find("Icon").gameObject;
        border = transform.Find("Border").gameObject;
        dname = descriptionWindow.transform.Find("Name").GetComponent<TextMeshProUGUI>();
        ddesc = descriptionWindow.transform.Find("Description").GetComponent<TextMeshProUGUI>();
        dstats = descriptionWindow.transform.Find("Stats").gameObject;
        dprice = descriptionWindow.transform.Find("Price").transform.Find("Number").GetComponent<TextMeshProUGUI>();
    }

    public void initializeNode(bool locked)
    {
        if (locked)
        {
            lockNode();
        } else
        {
            unlockNode();
        }
    }

    private void lockNode()
    {
        unlocked = false;
        background.GetComponent<Image>().color = Color.black;
        border.GetComponent<Image>().color = Color.black;
    }

    private void unlockNode()
    {
        unlocked = true;
        background.GetComponent<Image>().color = Color.white;
        border.GetComponent<Image>().color = Color.blue;
        Player.addSkill(nodeSkill);
    }

    public void OnPointerEnter(PointerEventData pointerEventData)
    {
        //display info window and fill it with appropriate data
        fillWindow();
        descHover.resetHover();
        descriptionWindow.SetActive(true);
        descriptionWindow.transform.position = gameObject.transform.position + new Vector3(0, 220);
        HandleCursorOverlay.setState(HandleCursorOverlay.cursorState.pointer);
        //preview stats
        HandleSkillTree.fillStatsWindow(Player.playerStats.generateStatStringPreview(nodeSkill));
    }

    private void fillWindow()
    {
        //read skill info and fill the appropriate text components
        dname.text = nodeSkill.name;
        ddesc.text = nodeSkill.description;
        dprice.text = nodeSkill.price.ToString();
        if (!requirementsMet()) dprice.color = Color.red;
        else dprice.color = Color.white;
        //grab and display skill passive effects
        List<PassiveEffect> passiveSkillEffects = nodeSkill.GetPassiveSkillEffects();
        //kill the children!
        for (int i = dstats.transform.childCount - 1; i >= 0; i--)
        {
            Destroy(dstats.transform.GetChild(i).gameObject);
        }
        foreach (PassiveEffect effect in passiveSkillEffects)
        {
            GameObject newStat = Instantiate(attribute);
            newStat.name = effect.stat;
            TextMeshProUGUI text = newStat.GetComponent<TextMeshProUGUI>();
            string affix = "";
            if (!effect.flat) affix = "%";
            if (effect.boost < 0)
            {
                text.color = Color.red;
                text.text = effect.stat + " " + effect.boost.ToString() + affix;
            } else
            {
                text.text = effect.stat + " +" + effect.boost.ToString() + affix;
            }
            newStat.transform.SetParent(dstats.transform, false);
        }
    }

    public bool requirementsMet()
    {
        bool ret = true;
        foreach (var req in requirements)
        {
            if (!req.unlocked) ret = false;
        }
        //having enough currency is also a requirement
        if(Player.getCurrency() < nodeSkill.price) ret = false;
        return ret;
    }

    public void OnPointerExit(PointerEventData pointerEventData)
    {
        descriptionWindow.SetActive(false);
        HandleCursorOverlay.setState(HandleCursorOverlay.cursorState.none);
        HandleSkillTree.fillStatsWindow(Player.playerStats.generateStatString());
    }

    public void OnPointerDown(PointerEventData pointerEventData)
    {
        Debug.Log("clicked on " + nodeSkill.name);
        //attempt to purchase skill
        if (requirementsMet())
        {
            unlockNode(); //are skill effects applied here or somewhere else?
            Player.addCurrency(nodeSkill.price * -1);
        }
        HandleSkillTree.fillStatsWindow(Player.playerStats.generateStatString());
    }
}
