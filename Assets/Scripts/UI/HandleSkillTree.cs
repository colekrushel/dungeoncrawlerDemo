using NUnit.Framework;
using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class HandleSkillTree : MonoBehaviour
{
    public static MonoBehaviour Instance { get; private set; }
    public static GameObject descriptionWindow;
    public static TreeNode[] nodes;
    public void Awake()
    {
        Instance = this;
        descriptionWindow = gameObject.transform.Find("DescriptionWindow").gameObject;
        nodes = GetComponentsInChildren<TreeNode>();
    }

    public static void initializeTree(List<Skill> enabledSkills)
    {
        //called when player data is loaded, enable all skills the player already has
        for (int i = 0; i < nodes.Length; i++)
        {
            TreeNode cnode = nodes[i];
            cnode.descriptionWindow = descriptionWindow;
            cnode.descHover = descriptionWindow.GetComponent<HoverMe>();
            bool contains = enabledSkills.Contains(cnode.nodeSkill);
            if (!contains)
            {
                cnode.initializeNode(true);
            } else
            {
                //skill found
                cnode.initializeNode(false);
            }
        }
    }

  

}
