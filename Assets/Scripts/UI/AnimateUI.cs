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

    private void Awake()
    {
        Cursor.visible = false;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Instance = this;
        //hide cursor because it is being overlaid with our own sprite
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
        leftEffectMask = GameObject.Find("AttackContainer").transform.Find("LeftAttackMask").GetComponent<Image>();
        rightEffectMask = GameObject.Find("AttackContainer").transform.Find("RightAttackMask").GetComponent<Image>();
        leftMaskImage = cursorOverlay.transform.Find("CursorOffset").Find("AttackCooldownIndicator").Find("LeftMask").GetComponentInChildren<Image>();
        rightMaskImage = cursorOverlay.transform.Find("CursorOffset").Find("AttackCooldownIndicator").Find("RightMask").GetComponentInChildren<Image>();

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
    // effect objects
    private static Image leftEffectMask;
    private static RawImage leftEffectImage;
    private static bool playLeftEffect = false;
    private static Image rightEffectMask;
    private static RawImage rightEffectImage;
    private static bool playRightEffect = false;
    // vars for camera shake
    [SerializeField] private new Camera camera; // set this via inspector
    static float shake = 0;
    float shakeAmount = 0.02f;
    float decreaseFactor = 1f;
    // grab every UI component window
    static private List<GameObject> components;
    static private Image monitorBackground;
    static private GameObject firewall;
    //cursor handling with attack indications
    [SerializeField] private GameObject cursorOverlay;
    private Image leftMaskImage;
    private Image rightMaskImage;

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
        if (playLeftEffect)
        {
            leftEffectImage.uvRect = new Rect(leftEffectImage.uvRect.position + new Vector2(1, 0) * Time.deltaTime * 10, leftEffectImage.uvRect.size);
            if(leftEffectImage.uvRect.x > 1)
            {
                //if effect has done a complete pass then stop animating it
                playLeftEffect = false;
                leftEffectMask.color = new Color(0, 0, 0, 0);
            }
        }
        if (playRightEffect)
        {
            rightEffectImage.uvRect = new Rect(rightEffectImage.uvRect.position + new Vector2(1, 0) * Time.deltaTime * 10, rightEffectImage.uvRect.size);
            if (rightEffectImage.uvRect.x > 1)
            {
                //if effect has done a complete pass then stop animating it
                playRightEffect = false;
                rightEffectMask.color = new Color(0, 0, 0, 0);
            }
        }
        //scale size of firewall proportionally to its health
        if (Player.maxBlockHP != 0)
        {
            firewall.transform.localScale = new Vector3(Player.currentBlockHP / Player.maxBlockHP, Player.currentBlockHP / Player.maxBlockHP, 1);
            if (firewall.transform.localScale.x < 0) firewall.transform.localScale = new Vector3(0, 0, 1);
        }

        //scale fill ratio of attack indicator proportionally to player cooldown and move it on top of mouse
        cursorOverlay.transform.position = Input.mousePosition;
        if (Player.leftItem != null)
        {
            leftMaskImage.fillAmount = Mathf.Abs((Player.leftItem.cooldown - Player.leftCooldown) / Player.leftItem.cooldown);
            //if it is 0 then remove indicator
            if (Player.leftCooldown <= 0) leftMaskImage.fillAmount = 0;
        }
        if (Player.rightItem != null) {
            rightMaskImage.fillAmount = Mathf.Abs((Player.rightItem.cooldown - Player.rightCooldown) / Player.rightItem.cooldown);
            //if it is 0 then remove indicator
            if (Player.rightCooldown <= 0) rightMaskImage.fillAmount = 0;
        }

    }

    public static void setEffect(RawImage img, bool left)
    {
        if (left)
        {
            leftEffectImage = img;
            leftEffectImage.uvRect = new Rect(new Vector2(-1, 0), leftEffectImage.uvRect.size);
            leftEffectImage.transform.localPosition = Vector3.zero;
            playLeftEffect = true;
            leftEffectMask.color = new Color(0, 0, 0, .01f);
        } else
        {
            rightEffectImage = img;
            rightEffectImage.uvRect = new Rect(new Vector2(-1, 0), rightEffectImage.uvRect.size);
            rightEffectImage.transform.localPosition = Vector3.zero;
            playRightEffect = true;
            rightEffectMask.color = new Color(0, 0, 0, .01f);
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
