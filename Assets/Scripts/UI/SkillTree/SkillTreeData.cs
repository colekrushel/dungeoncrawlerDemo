using System;
using UnityEngine;

//serializable skill tree data so player's skills can be saved
[Serializable]
public class SkillTreeData
{
    //connections and nodes are stored in the ui and given unique name identifiers
    string[] unlockedConnections;
    string[] unlockedNodes;
}
