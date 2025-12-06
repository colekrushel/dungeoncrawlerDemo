
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static UnityEditor.Progress;


//base enemy class
public class Enemy : MonoBehaviour, IHittable
{
    [SerializeField] protected Vector2 pos; //position on grid
    protected int layer = 0; //current layer
    [SerializeField] protected string zone;
    protected float HP; //hit points
    protected int baseDamage;
    protected float actionRange; //tiles player must be within to initiate an action - same as longest action the enemy has
    protected int alertRange; //tiles player must be within to initiate chase
    protected float ICD;  //internal cool down - when this number gets below 0, triggers an action - goes down by around 1 per second
    protected float ICDBase; //number ICD is set to when reset; smaller number means faster enemy behavior 
    protected float ICDDelay = 1; //multiplier to delay between states; used for adjusting charge/attack times
    protected Animator animator; //this enemy's animator in the scene; also acts as the enemy's state machine
    public GameObject positionObject; //parent object used to control the enemy's position - because animation transforms are local, the enemy's transforms cant be manipulated directly
    [SerializeField] protected EnemyAction[] enemyActions;
    //all enemies also have parts. parts are tied to gameobjects on the enemy model and have their own hp bar. they are also tied to specific actions, such that some actions cannot be performed if one or all parts are dead.
    [SerializeField] protected EnemyPart[] parts;
    [SerializeField] protected GameObject breakFX;
    protected EnemyAction currentAction;
    enum enemyBehavior { Chase, Stationary, Wander}
    [SerializeField] enemyBehavior behavior;
    enum enemyState { Idle, Charging, Attacking, Stunned, Ragdoll, None };
    enemyState currentState = enemyState.Idle;
    [SerializeField] string currMovementDir = "";
    protected int dropAmount; //amount of currency to drop on kill

    private void Update()
    {
        if (EnemyManager.switchingLock || EnemyManager.pauseEnemies) return;
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
                    setupAction(magn);
                } else {
                    if(behavior == enemyBehavior.Chase)
                    {
                        move(hit);
                    }
                    
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

    private void setupAction(float dist)
    {
        //pick action from the list
        int ranIndex = Random.Range(0, enemyActions.Length);
        EnemyAction selectedAction = enemyActions[ranIndex];
        //check if action is possible to be performed (parts are not broken; all associated parts must be broken for the action to be disabled)
        int c = 0;
        while (!canPerformAction(selectedAction, dist) && c < 100)
        {
            //if cant perform the action then pick a new one until one we can perform has been picked
            ranIndex = Random.Range(0, enemyActions.Length);
            selectedAction = enemyActions[ranIndex];
            c++;
        }
        currentAction = selectedAction;
        //before attacking, determine if attack would actually hit the player. if not then turn to move to/face the player.
        //dont do this for anything with a range of 1.4 or greater (2+ tiles)
        bool startAction = true;
        if (selectedAction.actionRange <= 1.4 && currentAction.hitsFront)
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
        //if cell attack then assign the associated cellAction prefab with the enemy's startAction script; this lets enemies have multiple different cellAttacks.
        if(selectedAction.actionType == EnemyAction.ActionType.CellAttack)
        {
            StartCellAttack sca = positionObject.GetComponentInChildren<StartCellAttack>();
            sca.setAttack(selectedAction.cellAttack);
        }
        if (startAction)//start attack
        {
            actionRange = selectedAction.actionRange;
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

    private bool canPerformAction(EnemyAction action, float dist)
    {
        bool retval = false;
        //check for associated parts availability and dist between player and enemy
        if (action.associatedParts.Length == 0 && dist <= action.actionRange) return true;
        //return true if a single part is not broken or action has no parts; false otherwise
        foreach (EnemyPart part in parts)
        {
            if (action.associatedParts.Contains(part.partName) && !part.isBroken && dist <= action.actionRange) {
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
        if (currMovementDir != "") dir = currMovementDir[0];
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
            UIUtils.updateMap();
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
        //layermask to ignore other enemies
        int enemyLayer = LayerMask.NameToLayer("Enemy");
        int layerMask = ~(1 << enemyLayer); //Exclude enemy layer
        Physics.Linecast(positionObject.transform.position + .5f * positionObject.transform.up, Player.playerObject.transform.position, out hit, layerMask);
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
        //subtract player height offset 
        Vector3 distanceBetween = ((Player.playerObject.transform.position - Player.playerObject.transform.up * .5f) - positionObject.transform.position);
        float magn = distanceBetween.magnitude;
        if (magn > actionRange)
        {
            //if out of range then dont apply attack
            //Debug.Log("enemy attack missed, dist between is " + magn + " action range is " + actionRange);
        } else
        {
            int damage = baseDamage * 1; //attacks will have some kind of attack damage modifier
            UIUtils.addMessageToLog("Enemy hit player for " + damage + " damage", Color.red);
            Player.hitPlayer(damage);
        }

    }

    public float hitPart(float damage, GameObject partHit)
    {
        float effectiveness = 1;

        if (currentState == enemyState.Stunned) effectiveness = 2;
        EnemyPart part = getPartFromObject(partHit);
        if (part != null)
        {
            //only deal damage if part hit is associated with the current action 
            //bool inUse = currentAction.associatedParts.Contains(part.partName);
            //inUse = true; //override for now
            //if (!inUse) return effectiveness;

            //deal damage to each part; bonus if stunned
            if (currentState != enemyState.Stunned) effectiveness = part.effectiveness;
            damage *= effectiveness;

            //part onhit fx
            Renderer r = part.partModel.GetComponent<Renderer>();
            StartCoroutine(partHitFX(.5f, r));
            

            part.currentHP -= damage;
            HP -= damage;

            if (part.currentHP < 0)
            {
                //broke the part; now apply an effect and change the part to its broken appearance
                effectiveness = 2;
                part.partModel.GetComponent<MeshRenderer>().enabled = false;
                part.partModel.GetComponent<BoxCollider>().enabled = false;
                part.isBroken = true;
                //if a part was broken then stagger the enemy
                animator.Play("Stagger");
                UIUtils.playPartBreakEffect(part.partModel.transform.position, part, breakFX);
                ICD = ICDBase;
                currentState = enemyState.Stunned;
                currentAction = null;
            }
        }
        return effectiveness;
    }

    public IEnumerator partHitFX(float d, Renderer targetRenderer)
    {
        targetRenderer.material.color = Color.red;
        yield return new WaitForSeconds(d);
        targetRenderer.material.color = Color.white;
    }
    public float hitByPlayer(float damage, EquipmentItem.type type)
    {
        float effectiveness = 1;
        ////if enemy is stunned then do more damage
        //if(currentState == enemyState.Stunned)
        //{
        //    damage *= 2;
        //    effectiveness *= 2;
        //}
        //UIUtils.addMessageToLog("player hit enemy for " + damage + " damage", Color.green);

        ////apply the onhit effect (shake object)
        MovementManager.shakeObject(positionObject, .04f, 1f, .2f, positionObject.transform.position);

        ////check if enemy was killed 
        //HP -= damage;
        //Debug.Log("dealt " + damage + " damage to enemy");
        if (HP < 0)
        {
            //play death animation
            //animator.Play("Death");

            

            //first time death
            if (currentState != enemyState.Ragdoll)
            {
                //generic death animation
                animator.enabled = false;
                Physics.gravity = GridUtils.getZoneUpVector(zone) * -1 * Physics.gravity.magnitude;
                //make each part ragdoll by setting their rigidbodies to be kinematic at death

                Rigidbody[] bodies = positionObject.GetComponentsInChildren<Rigidbody>();
                foreach (Rigidbody b in bodies)
                {
                    b.isKinematic = false;
                    b.AddForce(Player.playerObject.transform.forward * 100);
                }

                //positionObject.transform.get
                //foreach (EnemyPart p in parts)
                //{
                //    GameObject partobj = p.partModel.gameObject;
                //    partobj.AddComponent<Rigidbody>();
                //    partobj.GetComponent<Rigidbody>().AddForce(Player.playerObject.transform.forward * 100);
                //}

                currentState = enemyState.Ragdoll;
                EnemyManager.removeEnemy(this);
                Player.addCurrency(this.dropAmount);
                StartCoroutine(onDeath());
            } else
            {
                //beating ragdoll; only apply physics and maybe give reduced drops per hit
                positionObject.GetComponent<Rigidbody>().AddForce(Player.playerObject.transform.forward * 100);
                Player.addCurrency(this.dropAmount / 10);
            }
            

            //set gravity to match orientation of current zone




        }
        //return effectiveness
        return effectiveness;
    }

    //called from point in death animation where death should be handled


    public IEnumerator onDeath()
    {
        yield return new WaitForSeconds(1);
        EnemyManager.killEnemy(this, this.dropAmount);
    }

    public void pause(bool paused)
    {
        //handle pausing animator and other params
        if (paused)
        {
            animator.speed = 0;
        } else
        {
            animator.speed = 1;
        }
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
