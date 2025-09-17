using UnityEngine;

public class MovementEntry
{
    public GameObject objectBeingMoved;
    //movement data
    public bool isMoving = false;
    public Vector3 finalPosition = Vector3.zero;
    public float movementSpeed;
    //rotation data
    public bool isRotating = false;
    public Quaternion startRotation;
    public Quaternion endRotation;
    public float totalRotation = 0f;
    public float rotationSpeed;

    //separate constructors for moving and rotating (and both
    public MovementEntry(GameObject obj, Vector3 finalpos, float speed)
    {
        objectBeingMoved = obj;
        finalPosition = finalpos;
        movementSpeed = speed;
        isMoving = true;
    }

    public MovementEntry(GameObject obj, Quaternion end, float speed) {
        objectBeingMoved = obj;
        startRotation = obj.transform.rotation;
        endRotation = end;
        rotationSpeed = speed;
        isRotating = true;
    }

    public MovementEntry(GameObject obj, Vector3 finalpos, float mspeed, Quaternion end, float rspeed)
    {
        objectBeingMoved = obj;
        finalPosition = finalpos;
        movementSpeed = mspeed;
        isMoving = true;
        startRotation = obj.transform.rotation;
        endRotation = end;
        rotationSpeed = rspeed;
        isRotating = true;
    }
}
