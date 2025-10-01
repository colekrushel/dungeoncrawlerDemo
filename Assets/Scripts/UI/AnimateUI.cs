using System;
using System.Collections;
using System.Collections.Generic;
using TMPro.Examples;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using static TMPro.SpriteAssetUtilities.TexturePacker_JsonArray;

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
            if(child.name == "Firewall") firewall = child.gameObject;
        }
        attackMaskImage = attackIndicator.GetComponentInChildren<Image>();

    }

    //animate constant scrolling effects
    [SerializeField] private static float scrollSpeed = 1f; //rework
    [SerializeField] private RawImage[] imagesToScroll;
    [SerializeField] private float[] scrollXSpeeds; //separate because cant serialize tuples
    [SerializeField] private float[] scrollYSpeeds;
    //animate constant shaking effects
    [SerializeField] private GameObject[] boxesToShake;
    float shakeDelay = .2f; //time between shakes for boxes
    float shakeCount = 0;
    // effect object
    private static RawImage effectImage;
    private static bool playEffect = false;
    // vars for camera shake
    [SerializeField] private new Camera camera; // set this via inspector
    static float shake = 0;
    float shakeAmount = 0.02f;
    float decreaseFactor = 1f;
    // grab every UI component window
    static private List<GameObject> components;
    static private Image monitorBackground;
    static private GameObject firewall;
    //attack indicator
    [SerializeField] private GameObject attackIndicator;
     private Image attackMaskImage;
    void Update()
    {
        //scroll rawimages
        for(int i = 0; i < imagesToScroll.Length; i++)
        {
            RawImage image = imagesToScroll[i];
            Tuple<float, float> scrollBase = Tuple.Create(scrollXSpeeds[i], scrollYSpeeds[i]);
            image.uvRect = new Rect(image.uvRect.position + new Vector2(scrollBase.Item1, scrollBase.Item2) * Time.deltaTime, image.uvRect.size);

        }
        //shake boxes
        if(shakeCount <= 0)
        {
            foreach (GameObject box in boxesToShake)
            {
                box.transform.localPosition = UnityEngine.Random.insideUnitSphere * 2;
            }
            shakeCount = shakeDelay;
        }
        shakeCount -= Time.deltaTime;
        //camera shake
        if (shake > 0)
        {
            camera.transform.localPosition = UnityEngine.Random.insideUnitSphere * shakeAmount;
            shake -= Time.deltaTime * decreaseFactor;

        }
        else
        {
            shake = 0f;
            //reset position
            camera.transform.localPosition = Vector3.zero;
        }
        //animate effects
        if (playEffect)
        {
            effectImage.uvRect = new Rect(effectImage.uvRect.position + new Vector2(1, 0) * Time.deltaTime * 10, effectImage.uvRect.size);
            if(effectImage.uvRect.x > 1)
            {
                //if effect has done a complete pass then stop animating it
                playEffect = false;
            }
        }
        //scale size of firewall proportionally to its health
        if(Player.maxBlockHP != 0)
        {
            firewall.transform.localScale = new Vector3(Player.currentBlockHP / Player.maxBlockHP, Player.currentBlockHP / Player.maxBlockHP, 1);
            if (firewall.transform.localScale.x < 0) firewall.transform.localScale = new Vector3(0, 0, 1);
        }

        //scale fill ratio of attack indicator proportionally to player cooldown and move it on top of mouse
        attackMaskImage.fillAmount = Mathf.Abs(1 - Player.leftCooldown);
        attackIndicator.transform.position = Mouse.current.position.ReadValue();
        //if it is 0 then remove indicator
        if(Player.leftCooldown <= 0) attackMaskImage.fillAmount = 0;

    }

    public static void setEffect(RawImage img)
    {
        effectImage = img;
        effectImage.uvRect = new Rect(new Vector2(-1, 0), effectImage.uvRect.size);
        effectImage.transform.localPosition = Vector3.zero;
        playEffect = true;
        
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
