using NUnit.Framework.Constraints;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class InputHandler : MonoBehaviour
{
    [SerializeField] GameObject gridParent;
    [SerializeField] bool isCreator = false;
    [SerializeField] Vector2Int startpos;
    DungeonGrid grid;
    //Player player;
    const int cellWidth = 1;
    bool loaded = false;
    const int speedMultiplier = 2;

    //for animating player & camera movement
    //rotation
    bool isRotating = false;
    Quaternion startRotation;
    Quaternion endRotation;
    float totalRotation;
    string facingQ;
    //movement
    bool isMoving = false;
    Vector3 finalPosition = Vector3.zero;
    Vector3 moveDir = Vector3.zero;
    

    //input buffer
    char bufferedInput; //hold buffered input
    const int bufferLifetime = 30; //frames that the buffered input should be held for
    int bufferCounter = 0; //count lifetime of buffered input


    //verticality
    //int verticalO

    

    //ui stuff
    [SerializeField] GameObject interactWindow;
    [SerializeField] GameObject mapperGrid;

    void Start()
    {
        Player.playerObject = gameObject;
        Player.currentLayer = 0;
        SnapPlayer(startpos.x, startpos.y);
        //get grid info from script

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
            } else
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
            if (Mathf.Abs(totalRotation - 90f) < 1)
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
            Player.playerObject.transform.position += new Vector3(moveAmount * moveDir.x, 0, moveAmount * moveDir.z);
            

            //check if movement complete
            Vector3 dist = (Player.playerObject.transform.position - finalPosition);
            if ( Mathf.Abs(dist.x) < (.01f*speedMultiplier) && Mathf.Abs(dist.z) < (.01f * speedMultiplier))
            {
                isMoving = false;          
                Player.playerObject.transform.position = finalPosition;
                OnMoveEnd();
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
                grid = gridScript.getGrid(Player.currentLayer);
                loaded = true;
                
            }
            Keyboard.current.onTextInput += cha => movePlayer(cha);
        }
    }

    void movePlayer(char inputKey, bool priority = false)
    {
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
                    Debug.Log(isMoving);
                    Debug.Log(isRotating);
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
        float vOffset = determineVerticalOffset();

        switch (dir)
        {
            case 180:
                //check if possible to move that way
                canMove = grid.canMoveBetween(playerPos, playerPos - new Vector2(0, 1), 'S');
                if (canMove || priority)
                {
                    //handle verticality (move to & from in-betweens)
                    finalPosition = Player.playerObject.transform.position + new Vector3(0, 0, -1) * cellWidth;
                    moveDir = new Vector3(0, 0, -1);
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
                    finalPosition = Player.playerObject.transform.position + new Vector3(-1, 0, 0) * cellWidth;
                    moveDir = new Vector3(-1, 0, 0);
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
                    finalPosition = Player.playerObject.transform.position + new Vector3(0, 0, 1) * cellWidth;
                    moveDir = new Vector3(0, 0, 1);
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
                    
                    finalPosition = Player.playerObject.transform.position + new Vector3(1, 0, 0) * cellWidth;
                    moveDir = new Vector3(1, 0, 0);
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

    float determineVerticalOffset()
    {
        float returnval = 0f;
        //check if in-between already

        //if in-between, check if going up again (moving in direction opposite of stair facing), going down (moving in direction of stair facing) or neither (can't move)

        //otherwise, check if moving onto stairs on same layer or stairs on bottom layer + empty on current layer

        return returnval;
    }

    void OnInteract(InputValue value)
    {
       // Debug.Log("interact");
        if (value.isPressed && !isMoving && !isRotating)
        {
            
            //call entity in cell's interact method
            DungeonCell infront = grid.getCellInDirection(grid.getCell(Player.getPos()), Player.facing);
            if (infront != null && infront.entity != null && infront.entity.interactable)
            {
                infront.entity.interact();
            }
        }
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
            Player.teleportPlayer(new Vector3(cellX, .25f, cellY));
        }
    }


    void OnMoveBegin()
    {
        //disable popup windows tied to entities when movement begins
        UIUtils.popOut(interactWindow);
        
        
    }
    void OnMoveEnd()
    {
        //after player finishes movement we want to check if the tile in front of the player is interactable
        if(grid.getCellInDirection(grid.getCell( (Player.getPos())), Player.facing).entity.interactable)
        {
            //display interaction prompt
            UIUtils.popIn(interactWindow);
        }
        //update minimap every time player moves
        UIUtils.updateMap(mapperGrid, grid);
    }
}
