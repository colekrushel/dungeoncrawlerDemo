using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

//data class for a node on the skill tree
public class TreeNode 
{
    [SerializeField]
    List<TreeConnection> requirements; //required connections to grant access to node
    [SerializeField]
    Skill nodeSkill; //skill granted when requirements are met
}
