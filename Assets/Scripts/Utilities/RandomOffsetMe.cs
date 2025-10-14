using UnityEngine;

public class RandomOffsetMe : MonoBehaviour
{
    //attach to objects you want to apply a random offset to on startup
    [SerializeField] Vector3 offsetMin = Vector3.zero;
    [SerializeField] Vector3 offsetMax = Vector3.zero;

    [SerializeField] GameObject obj;
    private void Awake()
    {
        //return;
        //float xRange = Random.Range(offsetMin.x, offsetMax.x);
        //float yRange = Random.Range(offsetMin.y, offsetMax.y);
        //float zRange = Random.Range(offsetMin.z, offsetMax.z);
        //obj.transform.localPosition += new Vector3(xRange, yRange, zRange);
    }
}
