using NUnit.Framework;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

//data class for a node on the skill tree

public class TreeNode : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField]
    public string nodeName { get; } //unique identifier
    [SerializeField]
    public List<TreeConnection> requirements { get; } //required connections to grant access to node
    [SerializeField]
    public Skill nodeSkill { get; }//skill granted when requirements are met
    bool unlocked = false;

    //ui display handling
    private GameObject background;
    private GameObject icon;
    public GameObject descriptionWindow;

    public void Awake()
    {
        background = transform.Find("BCKG").gameObject;
        icon = transform.Find("Icon").gameObject;
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
    }

    private void unlockNode()
    {
        unlocked = true;
        background.GetComponent<Image>().color = Color.white;
    }

    public void OnPointerEnter(PointerEventData pointerEventData)
    {
        //display info window and fill it with appropriate data
        descriptionWindow.SetActive(true);
        descriptionWindow.transform.position = gameObject.transform.position + new Vector3(0, 200);
    }

    public void OnPointerExit(PointerEventData pointerEventData)
    {
        descriptionWindow.SetActive(false);
    }
}
