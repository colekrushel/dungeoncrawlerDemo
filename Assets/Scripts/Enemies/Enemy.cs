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
    protected int attackRange; //tiles player must be within to initiate an attack
    protected int alertRange; //tiles player must be within to initiate chase
    protected float ICD;  //internal cool down - when this number gets below 0, triggers an action - goes down by around 1 per second
    protected float ICDBase; //number ICD is set to when reset; smaller number means faster enemy behavior 
    protected Animator animator; //this enemy's animator in the scene; also acts as the enemy's state machine
    public GameObject positionObject; //parent object used to control the enemy's position - because animation transforms are local, the enemy's transforms cant be manipulated directly
    enum enemyState { Idle, Active, Charging, Attacking, Stunned, Moving };
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
            Debug.Log("TRIGGER ACTION");
            triggerAction();
            ICD = ICDBase;
        }
    }

    private void triggerAction()
    {
        //state machine to decide what to do next
        switch (currentState)
        {
            //if enemy is idle, move it to a random nearby tile and then check if player is within alert range
            case enemyState.Idle:
                move();
                break;
            //if enemy is active, then start charging an attack 
            case enemyState.Active:
                animator.SetTrigger("Charge");
                StartCoroutine(waitForAnim());
                break;
            //if enemy is charging an attack, then play the attack animation and handling
            case enemyState.Charging:
                animator.SetTrigger("Attack");
                StartCoroutine(waitForAnim());
                break;
            //if enemy is stunned, un-stun it (back to active)
            case enemyState.Stunned:

                break;
        }
    }

    private void move()
    {
        //get first random legal move by picking a random direction and attempting to move in that direction until a legal move is found
        List<string> dirs = new List<string>();
        dirs[0] = "N"; dirs[1] = "E"; dirs[2] = "S"; dirs[3] = "W";
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
        MovementManager.moveObject(positionObject, newPos, .1f);

    }

    public void snap(Vector2 p, int l)
    {
        //snap enemy to position on grid
        pos = p;
        layer = l;
        positionObject.transform.position = new Vector3(pos.x, l, pos.y);
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

                break;
            //after enemy is done attacking, deal damage and return to active state
            case enemyState.Attacking:

                break;
        }
        //reset icd?
    }
}
