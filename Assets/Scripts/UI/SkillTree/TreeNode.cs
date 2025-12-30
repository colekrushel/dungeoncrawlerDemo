using NUnit.Framework;
using System;
using System.Collections.Generic;
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

    public GameObject descriptionWindow;
    public HoverMe descHover;
    //grab description components at wake so they dont have to be grabbed multiple times
    private TextMeshProUGUI dname;
    private TextMeshProUGUI ddesc;
    private GameObject dstats;
    private TextMeshProUGUI dprice;
    [SerializeField] GameObject attribute;
    [SerializeField] string displayName;
    [SerializeField] Sprite onImage;
    [SerializeField] Sprite offImage;
    [SerializeField] GameObject[] outgoingConnections;
    [SerializeField]
    public List<TreeNode> linkedNodes; //linked nodes; only 1 linked node can be activated at a time.

    public void Awake()
    {
        background = transform.Find("Border").gameObject;
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
        background.GetComponent<Image>().sprite = offImage;
        //border.GetComponent<Image>().color = Color.black;
    }

    private void unlockNode()
    {
        unlocked = true;
        background.GetComponent<Image>().sprite = onImage;
        Player.addSkill(nodeSkill);
        //when this node is unlocked, enable all outgoing connections
        foreach (GameObject conn in outgoingConnections)
        {
            enableConnection(conn);
        }
        //when this node is unlocked, lock all linked nodes (they can still be bought but effects are removed)
        foreach (TreeNode node in linkedNodes)
        {
            node.removeNode();
        }
    }

    public void removeNode()
    {
        //remove node from player's skills and lock it if necessary
        if (unlocked)
        {
            initializeNode(true);
            Player.removeSkill(nodeSkill);
        }

    }

    private void enableConnection(GameObject connection)
    {
        //bring uv coords over from off into on
        Rect uvrect = connection.transform.Find("Off").GetComponentInChildren<RawImage>().uvRect;
        connection.transform.Find("On").GetComponentInChildren<RawImage>().uvRect = uvrect;
        //disable off
        //connection.transform.Find("Off").gameObject.SetActive(false);
        StartCoroutine(UIUtils.fadeObject(connection.transform.Find("Off").Find("RI").gameObject, false, .5f));
        //enable on
        StartCoroutine(UIUtils.fadeObject(connection.transform.Find("On").Find("RI").gameObject, true, .5f));
        //connection.transform.Find("On").gameObject.SetActive(true);


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
        dname.text = displayName;
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
            bool flipPositive = false;
            if(effect.stat == "Cooldown") flipPositive = true;
            if (effect.boost < 0)
            {
                if(!flipPositive) text.color = Color.red;
                else text.color = Color.green;

                text.text = effect.stat + " " + effect.boost.ToString() + affix;
            } else
            {
                if(flipPositive) text.color = Color.red;
                text.text = effect.stat + " +" + effect.boost.ToString() + affix;
            }
            newStat.transform.SetParent(dstats.transform, false);
        }
        //grab skill effects
        List<BuffEffect> buffSkillEffects = nodeSkill.GetBuffSkillEffects();
        List<PassiveEffect> passiveBuffSkillEffects = new List<PassiveEffect>();
        foreach (BuffEffect effect in buffSkillEffects)
        {
            foreach (PassiveEffect buff in effect.buffs)
            {
                passiveBuffSkillEffects.Add(buff);
            }
        }
        //put skill effects as stats
        foreach (PassiveEffect effect in passiveBuffSkillEffects)
        {
            GameObject newStat = Instantiate(attribute);
            newStat.name = effect.stat;
            TextMeshProUGUI text = newStat.GetComponent<TextMeshProUGUI>();
            string affix = "";
            if (!effect.flat) affix = "%";
            bool flipPositive = false;
            if (effect.stat == "Cooldown") flipPositive = true;
            if (effect.boost < 0)
            {
                if (!flipPositive) text.color = Color.red;
                else text.color = Color.green;

                text.text = effect.stat + " " + effect.boost.ToString() + affix;
            }
            else
            {
                if (flipPositive) text.color = Color.red;
                text.text = effect.stat + " +" + effect.boost.ToString() + affix;
            }
            text.text += " [A]";
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
        //attempt to purchase skill
        if (requirementsMet() && !unlocked)
        {
            unlockNode(); 
            Player.addCurrency(nodeSkill.price * -1);
            AudioManager.playUISelect();
        }
        HandleSkillTree.fillStatsWindow(Player.playerStats.generateStatString());
    }
}
