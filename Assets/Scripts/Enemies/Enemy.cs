using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

//base enemy class
public class Enemy : MonoBehaviour
{
    protected Vector2 pos; //position on grid
    protected int layer; //current layer
    protected float HP; //hit points
    protected int baseDamage;
    protected int actionRange; //tiles player must be within to initiate an action
    protected int alertRange; //tiles player must be within to initiate chase
    protected float ICD;  //internal cool down - when this number gets below 0, triggers an action - goes down by around 1 per second
    protected float ICDBase; //number ICD is set to when reset; smaller number means faster enemy behavior 
    protected float ICDDelay = 1; //multiplier to delay between states; used for adjusting charge/attack times
    protected Animator animator; //this enemy's animator in the scene; also acts as the enemy's state machine
    public GameObject positionObject; //parent object used to control the enemy's position - because animation transforms are local, the enemy's transforms cant be manipulated directly
    [SerializeField] protected EnemyAction[] enemyActions;
    //all enemies also have parts. parts are tied to gameobjects on the enemy model and have their own hp bar. they are also tied to specific actions, such that some actions cannot be performed if one or all parts are dead.
    [SerializeField] protected EnemyPart[] parts;
    protected EnemyAction currentAction;
    enum enemyState { Idle, Charging, Attacking, Stunned};
    enemyState currentState = enemyState.Idle;

    private void Update()
    {
        //handle ICD
        ICD -= Time.deltaTime;
        if (ICD < 0)
        {
            triggerAction();
            ICD = ICDBase * ICDDelay;
        }
    }

    private void triggerAction()
    {
        //state machine to decide what to do next
        switch (currentState)
        {
            //if enemy is idle, move it 
            case enemyState.Idle:
                //check if the player is within range first; if so, then queue an attack. otherwise just move
                RaycastHit hit = playerCheck();
                if (hit.collider.gameObject && hit.collider.gameObject.name == "Player" && hit.distance < actionRange)
                {
                    setupAction();
                } else {
                    move(hit);
                }
                    
                break;
            //if enemy is charging an attack, then play the attack animation and handling
            case enemyState.Charging:
                animator.Play("Action");
                ICDDelay = currentAction.attackDelay;
                currentState = enemyState.Attacking;
                //StartCoroutine(waitForAnim());
                break;
            case enemyState.Attacking:
                currentState = enemyState.Idle;
                break;
            //if enemy is stunned, un-stun it (back to active)
            case enemyState.Stunned:
                currentState = enemyState.Idle;
                break;
        }
    }

    private void setupAction()
    {
        //pick action from the list
        int ranIndex = Random.Range(0, enemyActions.Length);
        EnemyAction selectedAction = enemyActions[ranIndex];
        currentAction = selectedAction;
        //set up override animator and attack values
        AnimatorOverrideController overrideController = new AnimatorOverrideController(animator.runtimeAnimatorController);
        overrideController["DroneCharge"] = selectedAction.chargeAnim;
        overrideController["DroneAttack"] = selectedAction.actionAnim;
        ICDDelay = currentAction.chargeDelay;
        animator.runtimeAnimatorController = overrideController;
        //Debug.Log(animator["ChargeAction"].name);
        animator.Play("ChargeAction");
        currentState = enemyState.Charging;
    }

    private void move(RaycastHit hit)
    {
        //at the start of movement, cast a line to see if the enemy should move towards the player or randomly
        bool playerFound = (hit.collider.gameObject.name == "Player" && hit.distance < alertRange);
        //different movement behavior based on whether enemy is active or not; unactive enemy moves randomly but active enemy follows player
        if (!playerFound)
        {
            //get first random legal move by picking a random direction and attempting to move in that direction until a legal move is found
            List<string> dirs = new List<string>();
            dirs.Add("N"); dirs.Add("S"); dirs.Add("E"); dirs.Add("W");
            bool canmove = false;
            string ranDir = "N";
            while (!canmove)
            {
                int ranIndex = Random.Range(0, dirs.Count);
                ranDir = dirs[ranIndex];
                canmove = GridUtils.canMoveInDirection(pos, layer, ranDir);
            }
            //now that a valid direction has been found, move enemy in direction (and rotate it?)
            Vector3 newPos = positionObject.transform.position + MovementManager.directionToVector3(ranDir);
            MovementManager.moveObject(positionObject, newPos, .01f);
            //update pos and clear old pos on map
            //UIUtils.updateSingleMapCell((int)pos.x, (int)pos.y, GridDicts.typeToSprite["None"]);
            pos.x = newPos.x; pos.y = newPos.z;
            
        } else
        {
            //Debug.Log("move toward player");
            //if a player was found, use the offset in positions between the player and enemy to determine what direction to move 
            string dir = GridUtils.getDirectionBetween(positionObject.transform.position, Player.playerObject.transform.position);
            Vector3 newPos = positionObject.transform.position + MovementManager.directionToVector3(dir);
            MovementManager.moveObject(positionObject, newPos, .01f);
            //update pos 
            //update pos and clear old pos on map
            //UIUtils.updateSingleMapCell((int)pos.x, (int)pos.y, GridDicts.typeToSprite["None"]);
            pos.x = newPos.x; pos.y = newPos.z;
        }
        //after movement update the map
        UIUtils.updateMap();
        //UIUtils.updateSingleMapCell((int)pos.x, (int)pos.y, GridDicts.typeToSprite["Enemy"]);

    }

    RaycastHit playerCheck()
    {
        //do a linecast to the player and return its hit info
        RaycastHit hit;
        //move cast out of the floor
        Physics.Linecast(positionObject.transform.position + new Vector3(0, .5f, 0), Player.playerObject.transform.position, out hit);
        return hit;
    }
    public void snap(Vector2 p, int l)
    {
        //snap enemy to position on grid
        pos = p;
        layer = l;
        positionObject.transform.position = new Vector3(pos.x, l, pos.y);
    }

    public void attackHit()
    {
        //check if attack actually hit player


        int damage = baseDamage * 1; //attacks will have some kind of attack damage modifier
        UIUtils.addMessageToLog("Enemy hit player for " + damage + " damage", Color.red);
        Player.hitPlayer(damage);
    }

    public void hitPart(float damage, GameObject partHit)
    {
        EnemyPart part = getPartFromObject(partHit);
        if (part != null)
        {
            part.currentHP -= damage;
            if (part.currentHP < 0)
            {
                //broke the part; now apply an effect and change the part to its broken appearance
                part.partModel.GetComponent<MeshRenderer>().enabled = false;
                part.isBroken = true;
                //if a part was broken while the current action uses it, stagger the enemy
                if (currentAction.associatedParts.Contains(part.name)) //doesnt work
                {
                    Debug.Log("stagger enemy");
                    animator.Play("Stagger");
                    currentState = enemyState.Stunned;
                }

            }
        }
    }
    public void hitByPlayer(float damage)
    {

        UIUtils.addMessageToLog("player hit enemy for " + damage + " damage", Color.green);

        //apply the onhit effect (shake object)
        MovementManager.shakeObject(positionObject, .04f, 1f, .5f, positionObject.transform.position);
        //check if enemy was killed 
        HP -= damage;
        if(HP < 0)
        {
            //play death animation/fx

            //destroy object
        }
    }

    private EnemyPart getPartFromObject(GameObject obj)
    {
        //loop through parts and check if they match the partname of the given game object
        EnemyPart objPart = obj.GetComponent<EnemyPart>();
        EnemyPart returnVal = null;
        if(objPart != null)
        {
            foreach (EnemyPart part in parts)
            {
                if (part == objPart)
                {
                    returnVal = part;
                }
            }
        }
        return returnVal;
 
    }

    public Vector2 getPos()
    {
        return pos;
    }
}
