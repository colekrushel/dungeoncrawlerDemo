using UnityEngine;

//data class for a connection on the skill tree
public class TreeConnection
{
    [SerializeField]
    TreeNode outNode; //node this connection is leaving from
    [SerializeField]
    TreeNode inNode; //node this connection is going towards
    int price; //amount of currency to enable node on the tree
    bool enabled;
}
