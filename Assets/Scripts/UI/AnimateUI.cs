using UnityEngine;
using UnityEngine.UI;

public class AnimateUI : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    [SerializeField] private RawImage _imageToScroll;
    [SerializeField] private float _x, _y;
    void Update()
    {
        _imageToScroll.uvRect = new Rect(_imageToScroll.uvRect.position + new Vector2(_x, _y) * Time.deltaTime, _imageToScroll.uvRect.size);
    }
}
