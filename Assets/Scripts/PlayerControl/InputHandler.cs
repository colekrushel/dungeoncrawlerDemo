using JetBrains.Annotations;
using NUnit.Framework.Constraints;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.Tilemaps;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.UIElements;
using static UnityEditor.Experimental.GraphView.GraphView;

public class InputHandler : MonoBehaviour
{
    [SerializeField] GameObject gridParent;
    [SerializeField] bool isCreator = false;
    [SerializeField] Vector2Int startpos;
    [SerializeField] new Camera camera;
    DungeonGrid[] grids;
    DungeonGrid grid;
    //Player player;
    const int cellWidth = 1;
    bool loaded = false;
    const int speedMultiplier = 3;

    //for animating player & camera movement
    //rotation
    bool isRotating = false;
    Quaternion startRotation;
    Quaternion endRotation;
    float totalRotation;
    //movement
    bool isMoving = false;
    Vector3 finalPosition = Vector3.zero;
    Vector3 moveDir = Vector3.zero;
    

    //input buffer
    char bufferedInput; //hold buffered input
    const int bufferLifetime = 30; //frames that the buffered input should be held for
    int bufferCounter = 0; //count lifetime of buffered input


    //attacking
    Vector2 leftStartPos;
    Vector2 rightStartPos;



    //ui stuff
    [SerializeField] GameObject interactWindow;
    [SerializeField] GameObject firewall;

    void Start()
    {
        Player.loadPlayerInfo();
        Player.playerObject = gameObject;
        Player.currentLayer = 0;
        SnapPlayer(startpos.x, startpos.y);
        //move player onto 0,0 tile
    }

    // Update is called once per frame
    void Update()
    {
        
        //only decrement count when an input is held in the buffer
        if(bufferedInput != ']')bufferCounter -= 1;
        //remove buffered input after its lifetime expires
        if (bufferCounter <= 0)
        {
            bufferedInput = ']';
        }
        //execute buffered input when not performing action and empty buffer
        if(!isMoving && !isRotating)
        {
            //if any items are in the action queue then execute them as soon as possible, prioritizing over all other inputs
            if(Player.actionQueue.Count > 0)
            {
                movePlayer(Player.actionQueue[0], true);
                Player.actionQueue.RemoveAt(0);
            } else if(bufferedInput != ']')
            {
                movePlayer(bufferedInput);
                bufferedInput = ']';
            }

        }
        if (isRotating)
        {
            Vector3 rotationVector = new Vector3(0, 1f, 0);
            float rotationSpeed = 180f * speedMultiplier; //degrees per second

            //rotate cam from 0 to 90 deg
            if(totalRotation < 90f)
            {
                float step = rotationSpeed * Time.deltaTime;
                transform.rotation = Quaternion.RotateTowards(transform.rotation, endRotation, step);
                totalRotation += step;
            }

            //snap to final rotation
            //add check for bizarre softlock (isRotating is stuck at true even when player rotation is 90)
            if (Mathf.Abs(totalRotation - 90f) < 2*speedMultiplier || Player.playerObject.transform.rotation.y % 90 == 0)
            {
                isRotating = false;
                Player.playerObject.transform.rotation = endRotation;
                //finished rotating, now check if ui elements need to be displayed
                Player.updateFacing();
                OnMoveEnd();
            }




        }
        if (isMoving)
        {
            float moveAmount = 1f * speedMultiplier * Time.deltaTime;
            //move player towards new cell
            Player.playerObject.transform.position += new Vector3(moveAmount * moveDir.x, moveAmount * moveDir.y, moveAmount * moveDir.z);
            

            //check if movement complete
            Vector3 dist = (Player.playerObject.transform.position - finalPosition);
            if ( Mathf.Abs(dist.x) < (.01f*speedMultiplier) && Mathf.Abs(dist.z) < (.01f * speedMultiplier))
            {
                isMoving = false;          
                Player.playerObject.transform.position = finalPosition;
                OnMoveEnd();
            }
        }
        Vector2 startPos = Mouse.current.position.ReadValue();
        if(Player.rightCooldown > 0)Player.rightCooldown -= Time.deltaTime;
        if(Player.leftCooldown > 0)Player.leftCooldown -= Time.deltaTime;
        //if player is blocking then decrease shield health; otherwise restore it
        if (Player.leftItem != null && Player.leftItem.equipType == EquipmentItem.type.Shield)
        {
            if (Player.isBlocking && Player.currentBlockHP > 0)
            {
                Player.currentBlockHP -= Time.deltaTime * Player.leftItem.shieldDecay;
            } else if(Player.currentBlockHP < Player.maxBlockHP)
            {
                Player.currentBlockHP += Time.deltaTime * Player.leftItem.shieldRegen;
            }
        } else if (Player.rightItem != null && Player.rightItem.equipType == EquipmentItem.type.Shield)
        {
            if (Player.isBlocking && Player.currentBlockHP > 0)
            {
                Player.currentBlockHP -= Time.deltaTime * Player.rightItem.shieldDecay;
            }
            else if(Player.currentBlockHP < Player.maxBlockHP)
            {
                Player.currentBlockHP += Time.deltaTime * Player.rightItem.shieldRegen;
            }
        }
            

    }


    void OnMove2(InputValue value)
    {
        if (value.isPressed)
        {
            //Debug.Log(value);
            if (!loaded) //load grid into input script on first input 
            {
                RenderGrid gridScript = gridParent.GetComponent<RenderGrid>();
                grids = gridScript.getGrids();
                grid = grids[Player.currentLayer];
                
                loaded = true;
                
            }
            Keyboard.current.onTextInput += cha => movePlayer(cha);
        }
    }

    void movePlayer(char inputKey, bool priority = false)
    {
        inputKey = char.ToLower(inputKey);
        //if player is input locked then ignore all incoming inputs
        if (Player.inputLock) { return; }
        //store last pressed input as buffered input
        bufferedInput = ']';
        if(isRotating || isMoving)
        {
            bufferedInput = inputKey;
            bufferCounter = bufferLifetime;
        }
        //different control scheme for creation view
        if (isCreator)
        {
            switch (inputKey) 
            {
                case 'w':
                    if (!isMoving)
                    {
                        movePlayer(true, 0, priority);
                    }
                    break;
                case 'a':
                    if (!isMoving)
                    {
                        movePlayer(false, 90, priority);
                    }
                    break;
                case 's':
                    if (!isMoving)
                    {
                        movePlayer(false, 0, priority);
                    }
                    break;
                case 'd':
                    if (!isMoving)
                    {
                        movePlayer(false, -90, priority);
                    }
                    break;
            }
        }
        else
        {
            switch (inputKey)
            {
                //rotation keys
                case 'd':
                    if (!isRotating && !isMoving)
                    {
                        //set up rotation animation
                        isRotating = true;
                        startRotation = Player.playerObject.transform.rotation;
                        endRotation = Quaternion.AngleAxis(90f, Vector3.up) * startRotation;
                        totalRotation = 0f;
                        OnMoveBegin();
                    }
                    break;
                case 'a':
                    if (!isRotating && !isMoving)
                    {
                        //set up rotation animation
                        isRotating = true;
                        startRotation = Player.playerObject.transform.rotation;
                        endRotation = Quaternion.AngleAxis(-90f, Vector3.up) * startRotation;
                        totalRotation = 0f;
                        OnMoveBegin();

                    }
                    break;
                //movement keys
                case 'w': //move forwards
                    if (!isRotating && !isMoving)
                    {
                        movePlayer(true, 0, priority);
                    }
                    break;
                case 's': //move backwards
                    if (!isRotating && !isMoving)
                    {
                        movePlayer(false, 0, priority);
                    }
                    break;
                case 'e': //move right but keep camera fowards
                    if (!isRotating && !isMoving)
                    {
                        movePlayer(false, -90, priority);
                    }
                    break;
                case 'q': //move left but keep camera forwaards
                    if (!isRotating && !isMoving)
                    {
                        movePlayer(false, 90, priority);
                    }
                    break;

                //debug commands
                case 'c':
                    isRotating = false;
                    isMoving = false;
                    Player.playerObject.transform.eulerAngles = Vector3.zero;
                    SnapPlayer(0, 0);
                    break;
                case 'z':
                    Debug.Log(Player.currentLayer);
                    Debug.Log("is moving: " + isMoving);
                    Debug.Log("is rotating: " + isRotating);
                    isRotating = false;
                    isMoving = false;
                    break;


            }
        }
        
    }

    void movePlayer(bool moveForwards, float dirOffset = 0, bool priority = false)
    {
        
        //180 && -180 = south (-y)
        //-90 = west (-x)
        //0 = north (+y)
        //90 = east (+x)
        float dir = Player.playerObject.transform.rotation.eulerAngles.y + dirOffset;
        if (dir == -90) dir = 270; //exception for north -> west
        bool canMove;
        Vector2 playerPos = Player.getPos();

        //handle backwards by reversing player movement direction
        if (!moveForwards)
        {
            //method doesnt work for east -> west so add exception
            if (dir == 90) dir = 450;
            dir = Mathf.Abs(dir - 180);
            
        }
        
        float vOffset = 0;

        //grant priority if player is on stairs and moving in direction opposite of stair facing
        if (grid.getCell(Player.getPos()).type == "StairsUp" && GridUtils.getOppositeDirection(grid.getCell(Player.getPos()).entity.facing) == GridUtils.getDirectionFromDegrees((int)dir)) priority = true;
        

        switch (dir)
        {
            case 180:
                //check if possible to move that way
                canMove = grid.canMoveBetween(playerPos, playerPos - new Vector2(0, 1), 'S');
                if (canMove || priority)
                {
                    //handle verticality (move to & from in-betweens)
                    vOffset = determineVerticalOffset(moveForwards, dirOffset);
                    finalPosition = Player.playerObject.transform.position + new Vector3(0, vOffset, -1) * cellWidth;
                    moveDir = new Vector3(0, vOffset, -1);
                    isMoving = true;
                    Player.updatePos(playerPos - new Vector2(0, 1));
                    OnMoveBegin();
                }
                else
                {
                    Debug.Log("can't move south");
                }

                break;
            case 270:
                //check if possible to move that way
                canMove = grid.canMoveBetween(playerPos, playerPos - new Vector2(1, 0), 'W');
                if (canMove || priority)
                {
                    vOffset = determineVerticalOffset(moveForwards, dirOffset);
                    finalPosition = Player.playerObject.transform.position + new Vector3(-1, vOffset, 0) * cellWidth;
                    moveDir = new Vector3(-1, vOffset, 0);
                    isMoving = true;
                    Player.updatePos(playerPos - new Vector2(1, 0));
                    OnMoveBegin();
                }
                else
                {
                    Debug.Log("can't move west");
                }
                break;
            case 0:
                //check if possible to move that way
                canMove = grid.canMoveBetween(playerPos, playerPos + new Vector2(0, 1), 'N');
                if (canMove || priority)
                {
                    vOffset = determineVerticalOffset(moveForwards, dirOffset);
                    finalPosition = Player.playerObject.transform.position + new Vector3(0, vOffset, 1) * cellWidth;
                    moveDir = new Vector3(0, vOffset, 1);
                    Player.updatePos(playerPos + new Vector2(0, 1));
                    isMoving = true;
                    OnMoveBegin();
                }
                else
                {
                    Debug.Log("can't move north");
                }
                break;
            case 90:
                //check if possible to move that way
                canMove = grid.canMoveBetween(playerPos, playerPos + new Vector2(1, 0), 'E');
                if (canMove || priority)
                {
                    vOffset = determineVerticalOffset(moveForwards, dirOffset);
                    finalPosition = Player.playerObject.transform.position + new Vector3(1, vOffset, 0) * cellWidth;
                    moveDir = new Vector3(1, vOffset, 0);
                    Player.updatePos(playerPos + new Vector2(1, 0));
                    isMoving = true;
                    OnMoveBegin();
                }
                else
                {
                    Debug.Log("can't move east");
                }
                break;
        }

        
    }

    float determineVerticalOffset(bool moveForwards, float dirOffset)
    {
        float returnval = 0f;
        int dirnum = (int)Player.playerObject.transform.rotation.eulerAngles.y + (int)dirOffset; 

        string dir = GridUtils.getDirectionFromDegrees(dirnum);
        if (!moveForwards) dir = GridUtils.getOppositeDirection(dir);
        //check if in-between already
        if (Player.between.Item1 != Player.between.Item2)
        {
            Debug.Log("moving while between layers!");
            //if in-between, check if going up again (moving in direction opposite of stair facing), going down (moving in direction of stair facing) or neither (can't move)
            string sfacing = grid.getCell(Player.getPos()).entity.facing;
            if (dir == sfacing)
            {
                //if directions are equal then going down (maintaining current layer)
                returnval = -0.75f;
                Player.between = new System.Tuple<float, float>(Player.currentLayer, Player.currentLayer);
            } else if (dir == GridUtils.getOppositeDirection(sfacing))
            {
                //if directions are opposite then going up (increment current layer)
                returnval = 0.25f;
                Player.currentLayer += 1;
                grid = grids[Player.currentLayer];
                Player.between = new System.Tuple<float, float>(Player.currentLayer, Player.currentLayer);

            }
        }
        //otherwise, check if moving onto stairs on same layer or stairs on bottom layer + empty on current layer
        //separate checks for up stairs and down stairs
        else if (grid.getCellInDirection(grid.getCell(Player.getPos()), dir).type == "StairsUp")
        {
            //moving up onto stairs present on current layer
            Debug.Log("moving onto stairs on current layer!");
            returnval = 0.75f;
            Player.between = new System.Tuple<float, float>(Player.currentLayer, Player.currentLayer + 1);
        } else if (Player.currentLayer != 0 && grids[Player.currentLayer].getCellInDirection(grids[Player.currentLayer].getCell(Player.getPos()), dir).type == "StairsDown")
        {
            //moving down onto stairs on lower layer
            Debug.Log("moving onto stairs on lower layer!");
            returnval = -0.25f;
            //decrement player layer when moving down
            Player.currentLayer -= 1;
            grid = grids[Player.currentLayer];
            Player.between = new System.Tuple<float, float>(Player.currentLayer, Player.currentLayer + 1);
        }

        return returnval;
    }

    void OnInteract(InputValue value)
    {
       // Debug.Log("interact");
        if (value.isPressed && !isMoving && !isRotating)
        {
            
            //call entity in cell's interact method
            //DungeonCell infront = grid.getCellInDirection(grid.getCell(Player.getPos()), Player.facing);
            //if (infront != null && infront.entity != null && infront.entity.interactable && !grid.getCell(Player.getPos()).hasWall(Player.facing) && !infront.hasWall(GridUtils.getOppositeDirection(Player.facing)))
            //{
            //    infront.entity.interact();
            //}
        }
    }

    //0
    void OnLeftDown(InputValue value)
    {
        if (Player.leftItem != null && Player.leftItem.equipType == EquipmentItem.type.Shield)
        {
            startBlock();
        } else
        {
            //save initial cursor position
            leftStartPos = Mouse.current.position.ReadValue();
        }

    }

    void OnLeftUp(InputValue value)
    {
        Vector2 startPos = Mouse.current.position.ReadValue();
        float diff = Mathf.Abs(startPos.x - leftStartPos.x + startPos.y - leftStartPos.y);
        if(Player.leftItem != null && Player.leftCooldown <= 0 && diff > 5) executeAttack(leftStartPos, startPos, false);
    }

    void OnRightDown(InputValue value)
    {
        //if shield equipped then handle it now
        if (Player.rightItem != null && Player.rightItem.equipType == EquipmentItem.type.Shield)
        {
            startBlock();
        } else
        {
            //save initial cursor position
            rightStartPos = Mouse.current.position.ReadValue();
        }

    }

    void OnRightUp(InputValue value)
    {
        Vector2 startPos = Mouse.current.position.ReadValue();
        float diff = Mathf.Abs(startPos.x - rightStartPos.x + startPos.y - rightStartPos.y);
        if (Player.rightItem != null && Player.rightCooldown <= 0 && diff > 5) executeAttack(rightStartPos, startPos, true);
    }

    //perform an attack when the mouse is lifted or when the mouse's position changes while it is being held down
    void executeAttack(Vector2 startPos, Vector2 endPos, bool rightEquip)
    {
        //get params from player data
        float range = 0;
        float damage = 0;
        float recoil = 0;
        float cooldown = 0;
        Texture effect = null;
        EquipmentItem.type type;
        EquipmentItem item = Player.rightItem;
        if (!rightEquip) item = Player.leftItem;
        range = item.range;
        damage = item.baseDamage;
        recoil = item.recoil;
        cooldown = item.cooldown;
        effect = item.effect;
        type = item.equipType;

        //different handling types
        switch (type)
        {
            case EquipmentItem.type.Strike:

                break;
            case EquipmentItem.type.Slash:

                break;
            case EquipmentItem.type.Pierce:

                break;
            case EquipmentItem.type.Shield:
                //end the blocking effect and dont execute any attack
                endBlock();
                return;
                //break;
        }

        //draw the ui element for the attack
        UIUtils.drawAttack(startPos, endPos, range, effect, !rightEquip);
        //perform calculations to find what was hit by the attack
        Vector2 diff = endPos - startPos;
        //get point range * 10 units in diff direction
        Vector2 endPoint = diff.normalized * (range*10) + startPos;
        //line cast at that point and x amount of intermediary points
        float intermediaryPointCnt = 10;

        List<GameObject> objectsHit = new List<GameObject>();
        List<RaycastHit> hits = new List<RaycastHit>();
        for(float i = 0; i < intermediaryPointCnt; i++)
        {
            Vector2 newPoint = (diff.normalized * (range*10) * (i/intermediaryPointCnt)) + startPos;
            Ray ray = camera.ScreenPointToRay(newPoint);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                //Debug.Log("Hit object name: " + hit.collider.gameObject.name);
                float dist = hit.distance;

                if(dist < 1)
                {
                    //gather all unique models hit into a list and perform a 'hit' on each one
                    GameObject objectHit = hit.collider.gameObject;
                    Debug.Log(objectHit.name);
                    if (!objectsHit.Contains(objectHit))
                    {
                        objectsHit.Add(objectHit);
                        hits.Add(hit);
                    }
                }
            }
        }
        //try to hit every unique object in list
        Enemy enemyHit = null;
        for(int i = 0; i < objectsHit.Count;i++)
        {
            GameObject objectHit = objectsHit[i];
            //play particle effects on each object
            
            //item.hitParticles.GetComponent<ParticleSystem>().Play();

            //try to get an enemy component from parents
            Enemy enemyScript = objectHit.GetComponentInParent<Enemy>();
            BreakablePart bp = objectHit.GetComponent<BreakablePart>();
            if (enemyScript != null)
            {
                enemyHit = enemyScript;
                //we want to deal damage to each part but only hit the enemy once; some effects we only want to happen once
                UIUtils.playAttackHitEffect(hits[i].point, item);
                enemyScript.hitPart(damage, objectHit);
            } else if(bp != null)
            {
                //just play an effect
                UIUtils.playAttackHitEffect(hits[i].point, item);
                bp.hitByPlayer(damage); 
            }
        }
        //now perform the actual hit on the enemy (if necessary)
        if (enemyHit != null)
        {
            enemyHit.hitByPlayer(damage);
        }
        //now apply recoil and cooldown to the player's hit action and display an indicator for this
        if(rightEquip)Player.rightCooldown = cooldown;
        else Player.leftCooldown = cooldown;
    }

    void startBlock()
    {
        Player.isBlocking = true;

        //display block fx
        UIUtils.popIn(firewall);
    }

    void endBlock()
    {
        Player.isBlocking = false;

        //stop block fx
        UIUtils.popOut(firewall);
    }


    void SnapPlayer(int cellX, int cellY)
    {
        Player.updatePos((new Vector2(cellX, cellY)));
        if (isCreator)
        {
            //move camera above grid
            //this.transform.rotation.SetEulerAngles(-90, transform.rotation.y, transform.rotation.z);
            Player.teleportPlayer(new Vector3(cellX, 5f, cellY));
        }
        else
        {
            Player.teleportPlayer(new Vector3(cellX, .5f, cellY));
        }
    }


    void OnMoveBegin()
    {
        //disable popup windows tied to entities when movement begins
        //UIUtils.popOut(interactWindow);
        
        
    }
    void OnMoveEnd()
    {
        //after player finishes movement we want to check if the tile in front of the player is interactable and not blocked by a wall
        //DungeonCell infront = grid.getCellInDirection(grid.getCell((Player.getPos())), Player.facing);
        //if (infront != null && infront.entity.interactable && !grid.getCell(Player.getPos()).hasWall(Player.facing) && !infront.hasWall(GridUtils.getOppositeDirection(Player.facing)))
        //{
        //    //display interaction prompt
        //    UIUtils.popIn(interactWindow);
        //}
        //update minimap every time player moves
        UIUtils.updateMap();
    }
}
