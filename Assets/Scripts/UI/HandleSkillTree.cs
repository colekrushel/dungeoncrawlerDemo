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

    public static void initializeTree(string[] enabledSkills)
    {
        //called when player data is loaded, enable all skills the player already has
        for (int i = 0; i < nodes.Length; i++)
        {
            TreeNode cnode = nodes[i];
            cnode.descriptionWindow = descriptionWindow;
            string skill = Array.Find(enabledSkills, e => e == cnode.name);
            if (skill != null)
            {
                cnode.initializeNode(false);
            } else
            {
                //skill not found in player skills; disable it
                cnode.initializeNode(true);
            }
        }
    }

    public static void setWindow(TreeNode node)
    {

    }

}
