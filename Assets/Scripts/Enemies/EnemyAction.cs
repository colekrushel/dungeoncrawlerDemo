using UnityEngine;

[CreateAssetMenu(fileName = "EnemyAction", menuName = "Scriptable Objects/EnemyAction")]
public class EnemyAction : ScriptableObject
{
    //an enemy action object holding info about a potential action an enemy can choose to do
    [SerializeField] public AnimationClip chargeAnim;
    [SerializeField] public float chargeDelay; //for longer/shorter charge animations; multiplier
    [SerializeField] public AnimationClip actionAnim;
    [SerializeField] public float attackDelay;//for longer/shorter attack animations; multiplier
    [SerializeField] public float damage;
    public enum ActionType { Attack, Buff, Debuff, CellAttack } 
    [SerializeField] public ActionType actionType;
    [SerializeField] public string effectInfo; //for extra effects the action may have
    [SerializeField] public float actionRange; //within how many tiles should the action effect the player or other entities
    [SerializeField] public bool hitsFront; //whether the attack hits in front of the enemy or does not care if the player is in front of the enemy when performing
    [SerializeField] public GameObject associatedObject; //extra object associated with the action, like spawning a projectile or particle effects, ect
    [SerializeField] public int weight; //how likely this action is to be picked; higher the better
    [SerializeField] public string[] associatedParts; //set to the same name as the parts because you cannot serialize components from prefabs
    [SerializeField] public CellAttack cellAttack; //cellattack object to be executed from the animation event if the actiontype is cellattack
}
