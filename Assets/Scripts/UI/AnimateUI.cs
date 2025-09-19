using System.Collections;
using System.Collections.Generic;
using TMPro.Examples;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class AnimateUI : MonoBehaviour
{
    public static MonoBehaviour Instance { get; private set; }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Instance = this;
        components = new List<GameObject> ();
        // on start grab every monitor window from the scene
        foreach (Transform child in GameObject.Find("PlayerUI").transform)
        {
            //check if gameobject is a component window 
            if (child.tag == "UIWindow")
            {
                //is a component window; grab it and store
                //Debug.Log("component found " +  child.gameObject.name);
                components.Add(child.gameObject);
            }
        }
        
    }

    // values for health bar animation
    [SerializeField] private RawImage _imageToScroll;
    [SerializeField] private float _x, _y;
    [SerializeField] private static float scrollSpeed = 1f;
    // vars for camera shake
    [SerializeField] private new Camera camera; // set this via inspector
    static float shake = 0;
    float shakeAmount = 0.02f;
    float decreaseFactor = 1f;
    // grab every UI component window
    static private List<GameObject> components;
    static private Image monitorBackground;
    void Update()
    {
        //Debug.Log(scrollSpeed);
        _imageToScroll.uvRect = new Rect(_imageToScroll.uvRect.position + new Vector2(_x, _y) * Time.deltaTime * scrollSpeed, _imageToScroll.uvRect.size);
        //camera shake
        if (shake > 0)
        {
            camera.transform.localPosition = Random.insideUnitSphere * shakeAmount;
            shake -= Time.deltaTime * decreaseFactor;

        }
        else
        {
            shake = 0f;
            //reset position
            camera.transform.localPosition = Vector3.zero;
        }
    }

    public static void updateHPMonitor(float healthPercent)
    {
        //perform feedback operations on hit by attack
        //make scroll faster
        scrollSpeed = 1f / healthPercent;
        //Debug.Log(healthPercent);
        //change color of ui to be darker red
        Color newColor = new Color(1f ,  healthPercent, healthPercent);
        foreach (var component in components)
        {
            if (component.transform.Find("Content"))
            {
                component.transform.Find("Content").gameObject.GetComponent<Image>().color = newColor;
            }
        }

        //make screen shake
        shake = .3f;
    }
}
