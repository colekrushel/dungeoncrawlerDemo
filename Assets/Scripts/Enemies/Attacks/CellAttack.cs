using UnityEngine;

[CreateAssetMenu(fileName = "CellAttack", menuName = "Scriptable Objects/CellAttack")]
public class CellAttack : ScriptableObject
{
    public enum cellAttackType { Ring, Random, Within} //behavior of cell selection; ring selects a ring of cells at radius centered at the enemy(including diagonals), random chooses random amt of cells within range, within selects all cells within range
    [SerializeField] public cellAttackType type; //req
    [SerializeField] public int range; //req
    [SerializeField] public GameObject attackObject; //prefab with an animation and attackhit script to execute after charging is done; req
    [SerializeField] public CellAttack nextAttack; //set a next attack to chain attacks; optional

    //type specific options
    //random
    [SerializeField] public int chance; //chance out of 100 for each cell to be selected for a random attack; req
    [SerializeField] public int randMax; //maximum amount of cells possible to be selected by a random type attack; optional
    
}
