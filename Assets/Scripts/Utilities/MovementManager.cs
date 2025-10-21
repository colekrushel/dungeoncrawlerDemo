using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MovementManager : MonoBehaviour
{
    //handles moving entities from 1 point to another and rotating entities from 1 rotation to another
    //keeps track of 'Movement Entries' and updates each one every frame 

    private static List<MovementEntry> entries = new List<MovementEntry>();
    public static MonoBehaviour Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    // Update is called once per frame
    void Update()
    {
        //for each object
        foreach (var entry in entries.ToList())
        {
            //if not rotating or moving, remove entry as it has finished
            if ((!entry.isMoving && !entry.isRotating && !entry.isShaking) || entry.objectBeingMoved == null)
            {
                entries.Remove(entry);
                continue;
            }
            if (entry.isRotating)
            {
                float rotationSpeed = 180f * entry.rotationSpeed; //degrees per second

                //rotate cam from 0 to 90 deg
                if (entry.totalRotation < 90f)
                {
                    float step = rotationSpeed * Time.deltaTime;
                    entry.objectBeingMoved.transform.rotation = Quaternion.RotateTowards(entry.objectBeingMoved.transform.rotation, entry.endRotation, step);
                    entry.totalRotation += step;
                }

                //snap to final rotation
                if (Mathf.Abs(entry.totalRotation - 90f) < 1 * rotationSpeed)
                {
                    entry.isRotating = false;
                    entry.objectBeingMoved.transform.rotation = entry.endRotation;
                    //finished rotating 
                    //Debug.Log("snapping to final rotation");
                }
            }
            if (entry.isMoving)
            {
                float moveAmount = 1f * entry.movementSpeed * Time.deltaTime;
                //move player towards new pos
                entry.objectBeingMoved.transform.position = Vector3.MoveTowards(entry.objectBeingMoved.transform.position, entry.finalPosition, entry.movementSpeed);


                //check if movement complete
                Vector3 dist = (entry.objectBeingMoved.transform.position - entry.finalPosition);
                if (Mathf.Abs(dist.x) < (.01f * entry.movementSpeed) && Mathf.Abs(dist.z) < (.01f * entry.movementSpeed) && Mathf.Abs(dist.y) < (0.01f * entry.movementSpeed))
                {
                    entry.isMoving = false;
                    entry.objectBeingMoved.transform.position = entry.finalPosition;
                    //movement complete
                }
            }
            if (entry.isShaking)
            {
                if (entry.shake > 0)
                {
                    entry.objectBeingMoved.transform.position = UnityEngine.Random.insideUnitSphere * entry.shakeAmount + entry.originalPosition;
                    entry.shake -= Time.deltaTime * entry.decreaseFactor;

                }
                else
                {
                    entry.shake = 0f;
                    //reset position
                    entry.objectBeingMoved.transform.position = entry.originalPosition;
                    entry.isShaking = false;
                    //execute finished action
                    if(entry.whenFinished != null )entry.whenFinished();
                }
            }
        }
    }

    public static void moveObject(GameObject objectToMove, Vector3 targetPos, float speed)
    {
        //check if object already present - no dupes allowed
        
        //create a movement entry
        MovementEntry entry = new MovementEntry(objectToMove.gameObject, targetPos, speed);
        if (entries.Contains(entry)) { 
        
        }
        else
        {
            entries.Add(entry);
        }
    }

    public static void rotateObject(GameObject objectToMove, Quaternion targetRotation, float speed)
    {
        //create a movement entry
        MovementEntry entry = new MovementEntry(objectToMove.gameObject, targetRotation, speed);
        entries.Add(entry);
    }

    public static void shakeObject(GameObject obj, float shakeAmt, float decreaseFact, float shakeLength, Vector3 originalPosition)
    {
        MovementEntry entry = new MovementEntry(obj, shakeAmt, decreaseFact, shakeLength, originalPosition);
        entries.Add(entry);
    }

    public static void shakeObject(GameObject obj, float shakeAmt, float decreaseFact, float shakeLength, Vector3 originalPosition, Action callback)
    {
        MovementEntry entry = new MovementEntry(obj, shakeAmt, decreaseFact, shakeLength, originalPosition);
        entry.setCallback(callback);
        entries.Add(entry);
    }

    public static Vector3 directionToVector3(string dir)
    {
        Vector3 direction = Vector3.zero;
        //handle compound directions like NE
        foreach (char c in dir)
        {
            switch (c)
            {
                case 'S':
                    direction += new Vector3(0, 0, -1);
                    break;
                case 'W':
                    direction += new Vector3(-1, 0, 0);
                    break;
                case 'N':
                    direction += new Vector3(0, 0, 1);
                    break;
                case 'E':
                    direction += new Vector3(1, 0, 0);
                    break;
            }
        }
        return direction;
    }
}
