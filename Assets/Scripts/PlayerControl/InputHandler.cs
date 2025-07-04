using NUnit.Framework.Constraints;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class InputHandler : MonoBehaviour
{
    [SerializeField] GameObject gridParent;
    [SerializeField] bool isCreator = false;
    DungeonGrid grid;
    Player player;
    const int cellWidth = 1;
    bool loaded = false;
    const int speedMultiplier = 2;

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

    void Start()
    {
        player = new Player(gameObject);
        //get grid info from script

        //move player onto 0,0 tile
        SnapPlayer(0, 0, new Vector3(0, 0, 0));
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
            movePlayer(bufferedInput);
            bufferedInput = ']';
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
                player.playerObject.transform.rotation = endRotation;
            }




        }
        if (isMoving)
        {
            float moveAmount = 1f * speedMultiplier * Time.deltaTime;
            //move player towards new cell
            player.playerObject.transform.position += new Vector3(moveAmount * moveDir.x, 0, moveAmount * moveDir.z);
            

            //check if movement complete
            Vector3 dist = (player.playerObject.transform.position - finalPosition);
            if ( Mathf.Abs(dist.x) < (.01f*speedMultiplier) && Mathf.Abs(dist.z) < (.01f * speedMultiplier))
            {
                isMoving = false;
                player.playerObject.transform.position = finalPosition;
            }
        }
    }


    void OnMove2(InputValue value)
    {
        if (value.isPressed)
        {
            if (!loaded) //load grid into input script on first input 
            {
                RenderGrid gridScript = gridParent.GetComponent<RenderGrid>();
                grid = gridScript.getGrid();
                loaded = true;
            }
            Keyboard.current.onTextInput += cha => movePlayer(cha);
        }
    }

    void movePlayer(char inputKey)
    {
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
                        movePlayer(true);
                    }
                    break;
                case 'a':
                    if (!isMoving)
                    {
                        movePlayer(false, 90);
                    }
                    break;
                case 's':
                    if (!isMoving)
                    {
                        movePlayer(false);
                    }
                    break;
                case 'd':
                    if (!isMoving)
                    {
                        movePlayer(false, -90);
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
                        startRotation = player.playerObject.transform.rotation;
                        endRotation = Quaternion.AngleAxis(90f, Vector3.up) * startRotation;
                        totalRotation = 0f;
                    }
                    break;
                case 'a':
                    if (!isRotating && !isMoving)
                    {
                        //set up rotation animation
                        isRotating = true;
                        startRotation = player.playerObject.transform.rotation;
                        endRotation = Quaternion.AngleAxis(-90f, Vector3.up) * startRotation;
                        totalRotation = 0f;

                    }
                    break;
                //movement keys
                case 'w': //move forwards
                    if (!isRotating && !isMoving)
                    {
                        movePlayer(true);
                    }
                    break;
                case 's': //move backwards
                    if (!isRotating && !isMoving)
                    {
                        movePlayer(false);
                    }
                    break;
                case 'e': //move right but keep camera fowards
                    if (!isRotating && !isMoving)
                    {
                        movePlayer(false, -90);
                    }
                    break;
                case 'q': //move left but keep camera forwaards
                    if (!isRotating && !isMoving)
                    {
                        movePlayer(false, 90);
                    }
                    break;

                //debug commands
                case 'c':
                    isRotating = false;
                    isMoving = false;
                    player.playerObject.transform.eulerAngles = Vector3.zero;
                    SnapPlayer(0, 0, new Vector3(0, 0, 0));
                    break;
                case 'z':
                    Debug.Log(isMoving);
                    Debug.Log(isRotating);
                    break;


            }
        }
        
    }

    void movePlayer(bool moveForwards, float dirOffset = 0)
    {
        //180 && -180 = south (-y)
        //-90 = west (-x)
        //0 = north (+y)
        //90 = east (+x)
        float dir = player.playerObject.transform.rotation.eulerAngles.y + dirOffset;
        if (dir == -90) dir = 270; //exception for north -> west
        bool canMove;
        Vector2 playerPos = player.getPos();

        //handle backwards by reversing player movement direction
        if (!moveForwards)
        {
            //method doesnt work for east -> west so add exception
            if (dir == 90) dir = 450;
            dir = Mathf.Abs(dir - 180);
            
        }

        switch (dir)
        {
            case 180:
                //check if possible to move that way
                canMove = grid.canMoveBetween(playerPos, playerPos - new Vector2(0, 1), 'S');
                if (canMove)
                {
                    finalPosition = player.playerObject.transform.position + new Vector3(0, 0, -1) * cellWidth;
                    moveDir = new Vector3(0, 0, -1);
                    isMoving = true;
                    player.updatePos(playerPos - new Vector2(0, 1));
                }
                else
                {
                    Debug.Log("can't move");
                }

                break;
            case 270:
                //check if possible to move that way
                canMove = grid.canMoveBetween(playerPos, playerPos - new Vector2(1, 0), 'W');
                if (canMove)
                {
                    finalPosition = player.playerObject.transform.position + new Vector3(-1, 0, 0) * cellWidth;
                    moveDir = new Vector3(-1, 0, 0);
                    isMoving = true;
                    player.updatePos(playerPos - new Vector2(1, 0));

                }
                else
                {
                    Debug.Log("can't move");
                }
                break;
            case 0:
                //check if possible to move that way
                canMove = grid.canMoveBetween(playerPos, playerPos + new Vector2(0, 1), 'N');
                if (canMove)
                {
                    finalPosition = player.playerObject.transform.position + new Vector3(0, 0, 1) * cellWidth;
                    moveDir = new Vector3(0, 0, 1);
                    player.updatePos(playerPos + new Vector2(0, 1));
                    isMoving = true;
                }
                else
                {
                    Debug.Log("can't move");
                }
                break;
            case 90:
                //check if possible to move that way
                canMove = grid.canMoveBetween(playerPos, playerPos + new Vector2(1, 0), 'E');
                if (canMove)
                {
                    finalPosition = player.playerObject.transform.position + new Vector3(1, 0, 0) * cellWidth;
                    moveDir = new Vector3(1, 0, 0);
                    player.updatePos(playerPos + new Vector2(1, 0));
                    isMoving = true;
                }
                else
                {
                    Debug.Log("can't move");
                }
                break;
        }

        
    }

    void OnInteract(InputValue value)
    {
        if (value.isPressed)
        {
            Debug.Log("interact");
        }
    }


    void SnapPlayer(int cellX, int cellY, Vector3 camRotation)
    {
        player.updatePos((new Vector2(cellX, cellY)));
        if (isCreator)
        {
            //move camera above grid
            //this.transform.rotation.SetEulerAngles(-90, transform.rotation.y, transform.rotation.z);
            player.teleportPlayer(new Vector3(cellX, 5f, cellY));
        }
        else
        {
            player.teleportPlayer(new Vector3(cellX, .25f, cellY));
        }
    }
}
