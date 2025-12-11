using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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
        currencyChangeObject = GameObject.Find("CurrencyChangePreview").transform.Find("Amt").gameObject;
        currencyChangeText = currencyChangeObject.GetComponent<TextMeshProUGUI>();
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
    private static float leftSpeedMult = 1;
    private static Image rightEffectMask;
    private static RawImage rightEffectImage;
    private static bool playRightEffect = false;
    private static float rightSpeedMult = 1;
    // vars for camera shake
    [SerializeField] private new Camera camera; // set this via inspector
    static float shake = 0;
    float shakeAmount = 0.02f;
    float decreaseFactor = 1f;
    // grab every UI component window
    private static List<GameObject> components;
    private static Image monitorBackground;
    private static GameObject firewall;
    //cursor handling with attack indications
    [SerializeField] private GameObject cursorOverlay;
    private Image leftMaskImage;
    private Image rightMaskImage;
    // currency
    static int currencyToBeAdded = 0;
    static int currencyAddedPerUpdate = 1;
    [SerializeField] private TextMeshProUGUI currencyText;
    //hide taskbar
    [SerializeField] GameObject iconTray;
    public static bool cursorInsideTray = false; //keep track of when cursor enters and exits taskbar instead of constantly checking its position
    public static int trayIdleThreshold = 2; //if taskbar is left alone for this many seconds then it will recede
    public static float trayIdleTimer = 0; //keep track of time since taskbar was not idle (cursor inside it)
    private static int trayOffset = 112; //amount to move tray down/back up by
    private static float moveTrayAmt = 0;
    private static float totalMovement = 0;
    private static bool trayUp = true;
    private static bool forceTrayOn;
    //fade in currency gain/loss display
    private static GameObject currencyChangeObject;
    private TextMeshProUGUI currencyChangeText;

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
            leftEffectImage.uvRect = new Rect(leftEffectImage.uvRect.position + new Vector2(1, 0) * leftSpeedMult * Time.deltaTime * 10, leftEffectImage.uvRect.size);
            if(leftEffectImage.uvRect.x > 1)
            {
                //if effect has done a complete pass then stop animating it
                playLeftEffect = false;
                leftEffectMask.color = new Color(0, 0, 0, 0);
            }
        }
        if (playRightEffect)
        {
            rightEffectImage.uvRect = new Rect(rightEffectImage.uvRect.position + new Vector2(1, 0) * rightSpeedMult * Time.deltaTime * 10, rightEffectImage.uvRect.size);
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

        //increment currency every frame
        if(currencyToBeAdded > 0)
        {
            //increment currency count
            currencyText.text = (int.Parse(currencyText.text.ToString()) + currencyAddedPerUpdate).ToString();
            currencyToBeAdded -= currencyAddedPerUpdate;
            //decrement currency change preview
            currencyChangeText.text = (int.Parse(currencyChangeText.text.ToString()) - currencyAddedPerUpdate).ToString();
            //prevent overflow
            if (currencyToBeAdded < 0) currencyToBeAdded = 0;
        } else if(currencyToBeAdded < 0)
        {
            //handle subtracting currency (decrement count)
            currencyText.text = (int.Parse(currencyText.text.ToString()) - currencyAddedPerUpdate).ToString();
            currencyToBeAdded += currencyAddedPerUpdate;
            //increment currency change preview
            currencyChangeText.text = (int.Parse(currencyChangeText.text.ToString()) + currencyAddedPerUpdate).ToString();
            //prevent overflow
            if (currencyToBeAdded > 0) currencyToBeAdded = 0;
        }

        //handle tray idle timer and flags for animation up and down
        if (cursorInsideTray || forceTrayOn)
        {
            //move tray up if it is not already up
            if (!trayUp)
            {
                moveTrayAmt = 2;
            }
            else
            {
                trayIdleTimer = 0;
            }
            //reset when done adding currency
            if (trayUp && !cursorInsideTray && forceTrayOn && currencyToBeAdded == 0)
            {
                //moveTrayAmt = -2;
                forceTrayOn = false;
                //trayIdleTimer = 0;
                //reset currency change preview as well
                Instance.StartCoroutine(UIUtils.fadeObject(currencyChangeObject, false, .5f));
            }
        }
        else if (trayUp)
        {
            //increment timer if tray is already up
            trayIdleTimer += Time.deltaTime;
            if (trayIdleTimer >= trayIdleThreshold)
            {
                //move tray down
                moveTrayAmt = -2;
            }


        }
        //handle tray animation up and down
        if (moveTrayAmt != 0)
        {
            iconTray.transform.position = iconTray.transform.position += new Vector3(0, moveTrayAmt, 0) * Time.deltaTime * 100;
            totalMovement += moveTrayAmt * Time.deltaTime * 100;
            if (Mathf.Abs(totalMovement) >= trayOffset)
            {
                
                moveTrayAmt = 0;
                if (totalMovement > 0)
                {
                    trayUp = true;
                    //snap tray back down
                    iconTray.transform.position -= new Vector3(0, totalMovement - trayOffset, 0);
                }
                else
                {
                    trayUp = false;
                    //snap tray back up
                    iconTray.transform.position -= new Vector3(0, trayOffset+totalMovement, 0);
                }
                totalMovement = 0;
                trayIdleTimer = 0;
            }
        }
        

    }

    public static void setEffect(RawImage img, bool left, float speedmult)
    {
        if (left)
        {
            leftEffectImage = img;
            leftEffectImage.uvRect = new Rect(new Vector2(-1, 0), leftEffectImage.uvRect.size);
            leftEffectImage.transform.localPosition = Vector3.zero;
            playLeftEffect = true;
            leftEffectMask.color = new Color(0, 0, 0, .01f);
            leftSpeedMult = speedmult;
        } else
        {
            rightEffectImage = img;
            rightEffectImage.uvRect = new Rect(new Vector2(-1, 0), rightEffectImage.uvRect.size);
            rightEffectImage.transform.localPosition = Vector3.zero;
            playRightEffect = true;
            rightEffectMask.color = new Color(0, 0, 0, .01f);
            rightSpeedMult = speedmult;
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

    public static void addCurrency(int amt)
    {
        //when adding currency, move the tray up so the user can see the increase
        forceTrayOn = true;
        currencyToBeAdded += amt;
        //display number gained
        currencyChangeObject.GetComponent<TextMeshProUGUI>().text = amt.ToString();
        Instance.StartCoroutine(UIUtils.fadeObject(currencyChangeObject, true, .5f));
    }

    private IEnumerator closeTray()
    {
        yield return new WaitForSeconds(1);
        cursorInsideTray = false;
    }
    private void OnMousePositionChanged(Vector2 newPosition)
    {
        Debug.Log($"Global mouse position changed: {newPosition}");
    }
}
