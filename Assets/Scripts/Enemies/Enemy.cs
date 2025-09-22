using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//base enemy class
public class Enemy : MonoBehaviour
{
    protected Vector2 pos; //position on grid
    protected int layer; //current layer
    protected int HP; //hit points
    protected int baseDamage;
    protected int actionRange; //tiles player must be within to initiate an action
    protected int alertRange; //tiles player must be within to initiate chase
    protected float ICD;  //internal cool down - when this number gets below 0, triggers an action - goes down by around 1 per second
    protected float ICDBase; //number ICD is set to when reset; smaller number means faster enemy behavior 
    protected float ICDDelay; //multiplier to delay between states; used for adjusting charge/attack times
    protected Animator animator; //this enemy's animator in the scene; also acts as the enemy's state machine
    public GameObject positionObject; //parent object used to control the enemy's position - because animation transforms are local, the enemy's transforms cant be manipulated directly
    [SerializeField] protected EnemyAction[] enemyActions;
    protected EnemyAction currentAction;
    enum enemyState { Idle, Charging, Attacking, Stunned, Moving };
    enemyState currentState = enemyState.Idle;

    //vals set and used within script

 



    private void Update()
    {
        //handle movement animation
        if (currentState == enemyState.Moving)
        {
            //if moving then increment in direction and check if done
        }

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

                    //StartCoroutine(waitForAnim());
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

    private IEnumerator waitForAnim()
    {
        //wait for the transition to end
        yield return new WaitUntil(() => animator.GetCurrentAnimatorStateInfo(0).normalizedTime <= 1.0f);
        yield return new WaitWhile(() => animator.GetCurrentAnimatorStateInfo(0).normalizedTime <= 1.0f);
        //now animation has ended, call the appropriate end behavior
        switch (currentState)
        {
            //after enemy is done charging, play attack animation
            case enemyState.Charging:
                Debug.Log("finished charging");
                break;
            //after enemy is done attacking, return to idle
            case enemyState.Attacking:
                Debug.Log("finished attacking");
                break;
        }
        //reset icd?
    }

    public Vector2 getPos()
    {
        return pos;
    }
}
