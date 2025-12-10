using UnityEngine;
using UnityEngine.UI;

public class ScrollUVMe : MonoBehaviour
{
    //put this script on a rawimage that you want to scroll the uvs of forever (to animate a loop)
    [SerializeField] float yMul;
    [SerializeField] float xMul;
    [SerializeField] int speed;
    RawImage img;
    void Start()
    {
        img = this.gameObject.GetComponent<RawImage>();
    }

    // Update is called once per frame
    void Update()
    {
        img.uvRect = new Rect(img.uvRect.x + Time.deltaTime * xMul * speed, img.uvRect.y + Time.deltaTime * yMul * speed, 1, 1);
    }
}
