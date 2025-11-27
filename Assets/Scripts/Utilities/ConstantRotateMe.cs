using UnityEngine;

public class ConstantRotateMe : MonoBehaviour
{
    [SerializeField] float speed;
    [SerializeField] float xMul;
    [SerializeField] float yMul;
    [SerializeField] float zMul;

    // Update is called once per frame
    void Update()
    {
        this.transform.Rotate(xMul*speed*Time.deltaTime, yMul*speed * Time.deltaTime, zMul*speed * Time.deltaTime);
    }
}
