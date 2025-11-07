using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Diagnostics;
using UnityEngine.UIElements;

//base enemy class
public class Enemy : MonoBehaviour, IHittable
{
    [SerializeField]protected Vector2 pos; //position on grid
    protected int layer = 0; //current layer
    [SerializeField] protected string zone;
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
    enum enemyState { Idle, Charging, Attacking, Stunned, None};
    enemyState currentState = enemyState.Idle;
    [SerializeField] string currMovementDir = "";
    protected int dropAmount; //amount of currency to drop on kill

    private void Update()
    {
        if (EnemyManager.switchingLock) return;
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
                //use positions for checking attack range instead of the raycast length as we dont know where on the player the hit might have been [and attack ranges are tile-based]
                Vector3 distanceBetween = (Player.playerObject.transform.position - positionObject.transform.position); distanceBetween.y = 0;
                float magn = (Player.getPos() - pos).magnitude;    
                
                if (hit.collider.gameObject && hit.collider.gameObject.name == "Player" && magn <= actionRange)
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
        //check if action is possible to be performed (parts are not broken; all associated parts must be broken for the action to be disabled)
        int c = 0;
        while(!canPerformAction(selectedAction) && c < 100)
        {
            //if cant perform the action then pick a new one until one we can perform has been picked
            ranIndex = Random.Range(0, enemyActions.Length);
            selectedAction = enemyActions[ranIndex];
            c++;
        }
        currentAction = selectedAction;
        //before attacking, determine if attack would actually hit the player. if not then turn to move to/face the player.
        bool startAction = true;
        if (currentAction.hitsFront)
        {
            //check if player is in front of the enemy
            if (GridUtils.getDirectionBetween(pos, Player.getPos()) == GridUtils.getDirectionOfObjectFacing(positionObject, zone))
            {
                startAction = true;
            }
            else
            {
                //cant reach with attack; turn to face player
                float angle = GridUtils.getDegBetweenDirections(GridUtils.getDirectionOfObjectFacing(positionObject, zone), GridUtils.getDirectionBetween(pos, Player.getPos()));

                Quaternion targetRotation = Quaternion.AngleAxis(angle, positionObject.transform.up) * positionObject.transform.rotation;
                MovementManager.rotateObject(positionObject, targetRotation, 1f);
                startAction = false;
            }
        }
        if(startAction)//start attack
        {
            //set up override animator and attack values
            AnimatorOverrideController overrideController = new AnimatorOverrideController(animator.runtimeAnimatorController);
            overrideController["DroneCharge"] = selectedAction.chargeAnim;
            overrideController["DroneAttack"] = selectedAction.actionAnim;
            ICDDelay = currentAction.chargeDelay;
            animator.runtimeAnimatorController = overrideController;
            animator.Play("ChargeAction");
            currentState = enemyState.Charging;
        }

    }

    private bool canPerformAction(EnemyAction action)
    {
        bool retval = false;
        if (action.associatedParts.Length == 0) return true;
        //return true if a single part is not broken or action has no parts; false otherwise
        foreach (EnemyPart part in parts)
        {
            if (action.associatedParts.Contains(part.partName) && !part.isBroken){
                retval = true; break;
            }
        }
        return retval;
    }

    private void move(RaycastHit hit)
    {
        //first step; determine a direction to move in if there is not one currently set (currmovedir)

        //at the start of movement, cast a line to see if the enemy should move towards the player or randomly
        bool playerFound = (hit.collider.gameObject.name == "Player" && hit.distance < alertRange);
        Vector3 newPos = Vector3.zero;
        char dir = ' ';
        if (currMovementDir != "")dir = currMovementDir[0];
        //different movement behavior based on whether enemy is active or not; unactive enemy moves randomly but active enemy follows player
        if (!playerFound && currMovementDir == "") //determine movement direction
        {
            //get first random legal move by picking a random direction and attempting to move in that direction until a legal move is found
            List<string> dirs = new List<string>();
            dirs.Add("N"); dirs.Add("S"); dirs.Add("E"); dirs.Add("W");
            bool canmove = false;
            string ranDir = "N";
            int c = 100;
            while (!canmove && c > 0)
            {
                int ranIndex = Random.Range(0, dirs.Count);
                ranDir = dirs[ranIndex];
                canmove = GridUtils.canMoveInDirection(pos, layer, ranDir);
                c--;
            }
            //now that a valid direction has been found, move enemy in direction (and rotate it?)
            dir = ranDir[0];
            currMovementDir = dir.ToString();
        } if (playerFound)
        {
            
            //TODO
            //rework facing system (maybe do this later when an enemy with a proper 'face' is implemented
            //sometimes enemy will continue to move even though the player is right in front of them?
            //enemies can still move into each other sometimes (has to do with when theyre attacking?)


            //if a player was found, use the offset in positions between the player and enemy to determine what direction to move 
            string playerdir = GridUtils.getDirectionBetween(pos, Player.getPos());
            //if direction found is a compound direction then pick one and check if it is legal; if not then pick the other one.
            dir = playerdir[0];
            if (playerdir.Length >= 2 && !GridUtils.canMoveInDirection(pos, layer, dir.ToString())) dir = playerdir[1];
            currMovementDir = dir.ToString();
        }
        //second step: check if move is legal or not
        if (currMovementDir != "" && !GridUtils.canMoveInDirection(pos, layer, dir.ToString()))
        {
            //if move would be illegal then dont move and reset movement direction
            currMovementDir = "";
            return;
        }
        //third step: rotate enemy if not facing, otherwise execute movement
        //now that we have a valid direction for the enemy to move in, we want to check if the enemy is facing that direction.
        if (GridUtils.getDirectionOfObjectFacing(positionObject, zone) == dir.ToString())
        {
            //enemy is facing the movement direction; move enemy forwards
            newPos = positionObject.transform.position + positionObject.transform.forward;
            Vector2 newV2 = GridUtils.directionToGridCoords(dir.ToString());
            pos.x = pos.x + newV2.x; pos.y = pos.y + newV2.y;
            MovementManager.moveObject(positionObject, newPos, .01f);
            //after movement update the map
            //UIUtils.handleEnemyMoveUpdate(new Vector2(pos.x - newV2.x, pos.y - newV2.y), new Vector2(pos.x, pos.y));
            //reset movement direction
            currMovementDir = "";
        }
        else
        {
            
            //enemy is not facing the movement direction; rotate enemy towards the direction
            //get amount to rotate (either 90 or -90)


            float angle = GridUtils.getDegBetweenDirections(GridUtils.getDirectionOfObjectFacing(positionObject, zone), dir.ToString());

            Quaternion targetRotation = Quaternion.AngleAxis(angle, positionObject.transform.up) * positionObject.transform.rotation;
            MovementManager.rotateObject(positionObject, targetRotation, 1f);

        }
        //newPos = positionObject.transform.position + MovementManager.directionToVector3(dir.ToString());
        //pos.x = newPos.x; pos.y = newPos.z;
        //MovementManager.moveObject(positionObject, newPos, .01f);

        
        //UIUtils.updateSingleMapCell((int)pos.x, (int)pos.y, GridDicts.typeToSprite["Enemy"]);

    }

    RaycastHit playerCheck()
    {
        //do a linecast to the player and return its hit info
        RaycastHit hit;
        //move cast out of the floor
        Physics.Linecast(positionObject.transform.position + .5f * positionObject.transform.up, Player.playerObject.transform.position, out hit);
        return hit;
    }
    public void snap(Vector2 p, int l)
    {
        //snap enemy to position on grid
        pos = p;
        layer = l;
        Vector3 newPos = GridUtils.coordToWorld(p, zone, l);
       // positionObject.transform.position = new Vector3(pos.x, l, pos.y);
        positionObject.transform.position = newPos;
    }

    public void attackHit()
    {
        //check if attack actually hit player
        Vector3 distanceBetween = (Player.playerObject.transform.position - positionObject.transform.position);
        distanceBetween.y = 0;
        float magn = distanceBetween.magnitude;
        if(magn > actionRange)
        {
            //if out of range then dont apply attack
        } else
        {
            int damage = baseDamage * 1; //attacks will have some kind of attack damage modifier
            UIUtils.addMessageToLog("Enemy hit player for " + damage + " damage", Color.red);
            Player.hitPlayer(damage);
        }

    }

    public void hitPart(float damage, GameObject partHit)
    {
        EnemyPart part = getPartFromObject(partHit);
        if (part != null && currentAction != null)
        {
            //only deal damage if part hit is associated with the current action 
            bool inUse = currentAction.associatedParts.Contains(part.partName);
            if (!inUse) return;
            part.currentHP -= damage;

            if (part.currentHP < 0)
            {
                //broke the part; now apply an effect and change the part to its broken appearance
                part.partModel.GetComponent<MeshRenderer>().enabled = false;
                part.isBroken = true;
                //if a part was broken then stagger the enemy
                animator.Play("Stagger");
                ICD = ICDBase * 2;
                currentState = enemyState.Stunned;
                currentAction = null;
            }
        }
    }
    public void hitByPlayer(float damage)
    {

        UIUtils.addMessageToLog("player hit enemy for " + damage + " damage", Color.green);

        //apply the onhit effect (shake object)
        MovementManager.shakeObject(positionObject, .04f, 1f, .2f, positionObject.transform.position);
        //check if enemy was killed 
        HP -= damage;
        //Debug.Log("dealt " + damage + " damage to enemy");
        if (HP < 0)
        {
            //play death animation
            animator.Play("Death");
            currentState = enemyState.None;
            
        }
    }

    //called from point in death animation where death should be handled
    public void onDeath()
    {
        //tell manager to destroy object
        EnemyManager.killEnemy(this, this.dropAmount);
    }

    private EnemyPart getPartFromObject(GameObject obj)
    {
        //loop through parts and check if they match the partname of the given game object
        EnemyPart objPart = obj.GetComponent<EnemyPart>();
        EnemyPart returnVal = null;
        if(objPart != null)
        {
            //ignore broken parts
            if (objPart.isBroken) return null;
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

    public int getLayer()
    {
        return layer;
    }

    public void initialize(int l, string z)
    {
        this.layer = l;
        this.zone = z;
        positionObject.transform.rotation = Quaternion.Euler(GridUtils.getZoneRotationEuler(z));
    }
}
