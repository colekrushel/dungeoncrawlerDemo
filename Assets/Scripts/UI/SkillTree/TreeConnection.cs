using UnityEngine;

//data & ui class for a connection on the skill tree
public class TreeConnection : MonoBehaviour
{

    [SerializeField]
    TreeNode outNode; //node this connection is leaving from
    [SerializeField]
    TreeNode inNode; //node this connection is going towards
    [SerializeField]
    private string connectionName; //generated from the names of the out and in node
    [SerializeField]
    public int price { get; } //amount of currency to enable node on the tree
    bool unlocked = false;

    private void Awake()
    {
        //initialize name before anything else happens
        connectionName = outNode.nodeName + inNode.nodeName;
    }
}
