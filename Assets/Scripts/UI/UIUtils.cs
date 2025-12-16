using System.Collections;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using static UnityEditor.Progress;


public class UIUtils : MonoBehaviour 
{
    //static vars so only have to be loaded once
    static public GameObject logWindow;
    static public GameObject mapGrid;
    static public GameObject entry;
    static public GameObject attackContainer;
    static public GameObject firewall;
    public static MonoBehaviour Instance { get; private set; }
    public static bool updatingLock = false;

    

    private void Awake()
    {
        logWindow = GameObject.Find("LogContent").gameObject;
        mapGrid = GameObject.Find("MapGrid").gameObject;
        entry = Resources.Load<GameObject>("Prefabs/UI/Entry");
        attackContainer = GameObject.Find("AttackContainer").gameObject;
       
        if (!Instance)
        {
            //Instance = new GameObject("UIUtils").AddComponent<AnimUtils>();
            Instance = this;
        }

        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
    }



    //'pops' in the window by toggling it and performing a little animation
    public static void popIn(GameObject window)
    {
        fadeObject(window, true, 0.2f); //doesnt work?
        window.SetActive(true);
    }

    public static void popOut(GameObject window)
    {
        window.SetActive(false);
    }


    //map vars
    public static int selectedLayer = 0;

    public static void selectLayer(int layer)
    {
        Debug.Log("selected layer " + layer);
    }

    //update a single cell on the map given a grid pos, mainly used to update enemy pos on map
    public static void updateSingleMapCell(int gridx, int gridy, Sprite spriteToPlace, Enemy associatedEnem = null)
    {
        //assuming item is on current layer
        //get corresponding index on map 
        int cellX = (gridx - (int)Player.getPos().x) + 2; //[0 - 4]
        int cellY = (gridy - (int)Player.getPos().y) + 2; //[0 - 4]
        cellY = Mathf.Abs(cellY - 4); //first object in mapgrid is in the topleft corner so inverse y to get the right cell from the grid data 
        //if either item is not between 0 - 4 then dont update map
        if (cellX < 0 || cellY < 0 || cellX > 4 || cellY > 4){
            return;
        } else
        {
            //update map with given sprite
            GameObject mapCell = mapGrid.transform.GetChild(cellY * 5 + cellX).gameObject;
            mapCell.transform.Find("Icon").gameObject.GetComponent<Image>().sprite = spriteToPlace;
            //if an enemy sprite is being placed then rotate it to match the enemy's current rotation
            if(associatedEnem != null)
            {
                //get rotation dir
                string dir = GridUtils.getDirectionOfObjectFacing(associatedEnem.positionObject, Player.orientation);
                switch (dir)
                {
                    case "N":
                        mapCell.transform.Find("Icon").transform.rotation = Quaternion.Euler(0, 0, 180);
                        break;
                    case "E":
                        mapCell.transform.Find("Icon").transform.rotation = Quaternion.Euler(0, 0, 90);
                        break;
                    case "S":
                        mapCell.transform.Find("Icon").transform.rotation = Quaternion.Euler(0, 0, 0);
                        break;
                    case "W":
                        mapCell.transform.Find("Icon").transform.rotation = Quaternion.Euler(0, 0, 270);
                        break;
                }
            } else
            {
                //else reset rotation
                mapCell.transform.Find("Icon").transform.rotation = Quaternion.Euler(0, 0, 180);
            }
        }
    }
    
    //overload for when we dont know what sprite to put on the spot but we want to update it because something changed
    //public static void updateSingleMapCell(int gridx, int gridy)
    //{
    //    DungeonGrid grid = GridUtils.grids[Player.currentLayer];
    //    DungeonCell realCell = grid.getCell(gridx, gridy);
    //    Sprite typesprite = GridDicts.typeToSprite[realCell.type];
    //    //updateSingleMapCell(gridx, gridy, typesprite);
    //}
    public static void updateMap()
    {
        if(updatingLock) return;
        updatingLock = true;
        DungeonGrid grid = GridUtils.grids[Player.currentLayer];
        //get cells around player and update the map with their info and walls
        //default is 2 cell radius around player (map grid is 5x5 with player's pos at center)
        //0,0 is bottom left of rendered map
        int playerX = (int)Player.getPos().x;
        int playerY = (int)Player.getPos().y;
        string[] walls = new string[4];
        walls[0] = "N"; walls[1] = "S"; walls[2] = "E"; walls[3] = "W";
        for (int y = playerY-2; y < playerY + 3; y++)
        {
            for(int x = playerX-2; x < playerX + 3; x++)
            {
                //get associated cell on the map
                int cellX = (x - playerX) + 2; //[0 - 4]
                int cellY = (y - playerY) + 2; //[0 - 4]
                cellY = Mathf.Abs(cellY - 4); //first object in mapgrid is in the topleft corner so inverse y to get the right cell from the grid data ; now the map is populated starting from the bottom left cell
                GameObject mapCell = mapGrid.transform.GetChild(cellY * 5 + cellX).gameObject;

                //check if cell is out of bounds first; draw empty cell if so
                if (grid.cellOutOfBounds(new Vector2Int(x, y)))
                {
                    //empty cell - background transparent, icon nullsprite
                    mapCell.transform.Find("Background").gameObject.GetComponent<Image>().color = new Color(0, 0, 0, 0);
                    mapCell.transform.Find("Icon").gameObject.GetComponent<Image>().sprite = GridDicts.typeToSprite["None"];
                    //clear walls to prevent holdovers
                    foreach (string wall in walls)
                    {
                        mapCell.transform.Find(wall).gameObject.GetComponent<Image>().color = new Color(0, 0, 0, 0); //set to transparent if wall is not part of cell
                    }
                    continue;
                } else
                {
                    DungeonCell realCell = grid.getCell(x, y);
                    //cell on map - assign sprite from typeToSprite dict and background from floorToColor dict
                    //ignore some entities used for instantiation
                    if (realCell.type == "Enemy" || realCell.type == "Prop")
                    {
                        mapCell.transform.Find("Icon").gameObject.GetComponent<Image>().sprite = GridDicts.typeToSprite["None"];
                    } else
                    {
                        mapCell.transform.Find("Icon").gameObject.GetComponent<Image>().sprite = GridDicts.typeToSprite[realCell.type];
                    }
                        
                    //mapCell.transform.Find("Background").gameObject.GetComponent<Image>().color = GridDicts.floorToColor[realCell.floorToAssign];
                    mapCell.transform.Find("Background").gameObject.GetComponent<Image>().color = new Color(0, 0, 0, .1f);
                    //assign walls - overlap should be fine visually?
                    foreach (string wall in walls)
                    {
                        if (!realCell.walls.Contains(wall))
                        {
                            mapCell.transform.Find(wall).gameObject.GetComponent<Image>().color = new Color(0, 0, 0, 0); //set to transparent if wall is not part of cell
                        } else
                        {
                            mapCell.transform.Find(wall).gameObject.GetComponent<Image>().color = Color.black;
                        }
                            
                    }
                    //for impassable props (props with no modifiers) we want to draw the map cell as being surrounded by walls to signify that it is not traversible.
                    if(realCell.type == "Prop" && !realCell.isTraversible() && realCell.getPos() != Player.getPos()) //and ignore player's position because player's tile is always considered untraversible
                    {
                        mapCell.transform.Find("N").gameObject.GetComponent<Image>().color = Color.black;
                        mapCell.transform.Find("E").gameObject.GetComponent<Image>().color = Color.black;
                        mapCell.transform.Find("S").gameObject.GetComponent<Image>().color = Color.black;
                        mapCell.transform.Find("W").gameObject.GetComponent<Image>().color = Color.black;
                    }

                }
                //reset rotation; necessary because some types change their rotation
                mapCell.transform.Find("Icon").transform.rotation = Quaternion.Euler(0, 0, 0);
            }
            //also call enemy updates
            EnemyManager.updateMapWithEnemyInfo(grid.layer);
        }
        //asign player sprite, along with rotation
        GameObject playerCell = mapGrid.transform.GetChild(2 * 5 + 2).gameObject;
        playerCell.transform.Find("Icon").gameObject.GetComponent<Image>().sprite = GridDicts.typeToSprite["Player"];
        //set rotation with switch statement because player's right is icon's left
        switch (Player.facing)
        {
            case "N":
                playerCell.transform.Find("Icon").transform.rotation = Quaternion.Euler(0, 0, Player.playerObject.transform.rotation.eulerAngles.y);
                break;
            case "E":
                playerCell.transform.Find("Icon").transform.rotation = Quaternion.Euler(0, 0, 270);
                break;
            case "S":
                playerCell.transform.Find("Icon").transform.rotation = Quaternion.Euler(0, 0, Player.playerObject.transform.rotation.eulerAngles.y);
                break;
            case "W":
                playerCell.transform.Find("Icon").transform.rotation = Quaternion.Euler(0, 0, 90);
                break;
        }
        updatingLock = false;

    }

    //public static void handleEnemyMoveUpdate(Vector2 prevPos, Vector2 newPos)
    //{
    //    if (!updatingLock)
    //    {
    //        //if not currently updating map then update the results of this enemy's movement
    //        updateSingleMapCell((int)newPos.x, (int)newPos.y, GridDicts.typeToSprite["Enemy"]);
    //        updateSingleMapCell((int)prevPos.x, (int)prevPos.y);
    //    }
    //}

    //from https://stackoverflow.com/questions/44933517/fading-in-out-gameobject
    public static IEnumerator fadeObject(GameObject objectToFade, bool fadeIn, float duration, bool destroyWhenFinished = false)
    {
        float counter = 0f;

        //Set Values depending on if fadeIn or fadeOut
        float a, b;
        if (fadeIn)
        {
            a = 0;
            b = 1;
        }
        else
        {
            a = 1;
            b = 0;
        }

        int mode = 0;
        Color currentColor = Color.clear;

        SpriteRenderer tempSPRenderer = objectToFade.GetComponent<SpriteRenderer>();
        Image tempImage = objectToFade.GetComponent<Image>();
        RawImage tempRawImage = objectToFade.GetComponent<RawImage>();
        MeshRenderer tempRenderer = objectToFade.GetComponent<MeshRenderer>();
        TextMeshProUGUI tempText = objectToFade.GetComponent<TextMeshProUGUI>();
        CanvasGroup tempGroup = objectToFade.GetComponent<CanvasGroup>();

        //Check if this is a Sprite
        if (tempSPRenderer != null)
        {
            currentColor = tempSPRenderer.color;
            mode = 0;
        }
        //Check if Image
        else if (tempImage != null)
        {
            currentColor = tempImage.color;
            mode = 1;
        }
        //Check if RawImage
        else if (tempRawImage != null)
        {
            currentColor = tempRawImage.color;
            mode = 2;
        }
        //Check if Text 
        else if (tempText != null)
        {
            currentColor = tempText.color;
            mode = 3;
        }

        //Check if 3D Object
        else if (tempRenderer != null)
        {
            currentColor = tempRenderer.material.color;
            mode = 4;

            //ENABLE FADE Mode on the material if not done already
            tempRenderer.material.SetFloat("_Mode", 2);
            tempRenderer.material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            tempRenderer.material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            tempRenderer.material.SetInt("_ZWrite", 0);
            tempRenderer.material.DisableKeyword("_ALPHATEST_ON");
            tempRenderer.material.EnableKeyword("_ALPHABLEND_ON");
            tempRenderer.material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            tempRenderer.material.renderQueue = 3000;
        }
        //check if CanvasGroup
        else if(tempGroup != null)
        {
            mode = 5;
        }
        else
        {
            yield break;
        }

        while (counter < duration)
        {
            counter += Time.deltaTime;
            float alpha = Mathf.Lerp(a, b, counter / duration);

            switch (mode)
            {
                case 0:
                    tempSPRenderer.color = new Color(currentColor.r, currentColor.g, currentColor.b, alpha);
                    break;
                case 1:
                    tempImage.color = new Color(currentColor.r, currentColor.g, currentColor.b, alpha);
                    break;
                case 2:
                    tempRawImage.color = new Color(currentColor.r, currentColor.g, currentColor.b, alpha);
                    break;
                case 3:
                    tempText.color = new Color(currentColor.r, currentColor.g, currentColor.b, alpha);
                    break;
                case 4:
                    tempRenderer.material.color = new Color(currentColor.r, currentColor.g, currentColor.b, alpha);
                    break;
                case 5:
                    tempGroup.alpha = alpha;
                    break;
            }
            yield return null;
        }
        //when finished hiding canvas group, disable it
        if (mode == 5 && !fadeIn) objectToFade.GetComponent<Canvas>().enabled = false;
        //if set, destroy object when done
        if (destroyWhenFinished)
        {
            Destroy(objectToFade);
        }
    }

    public static void addMessageToLog(string message, Color color)
    {


        GameObject newEntry = Instantiate(entry);
        newEntry.name = "newmessage";
        //newEntry.GetComponent<TextMeshProUGUI>().text = message;
        newEntry.GetComponent<TextMeshProUGUI>().color = color;
        newEntry.transform.SetParent(logWindow.transform, false);
        //adjust content height by adding height of new message entry to window content height
        logWindow.GetComponent<RectTransform>().sizeDelta = new Vector2(800, logWindow.GetComponent<RectTransform>().sizeDelta.y + newEntry.GetComponent<RectTransform>().sizeDelta.y);
        //scroll to bottom
        logWindow.transform.parent.gameObject.GetComponent<ScrollRect>().normalizedPosition = new Vector2(0, 0);
        Instance.StartCoroutine(animateMessageToLog(message, newEntry.GetComponent<TextMeshProUGUI>()));
    }

    static IEnumerator animateMessageToLog(string message, TextMeshProUGUI textComponent)
    {

        for (int i = 0; i < message.Length; i++)
        {
            string currentText = message.Substring(0, i + 1);
            textComponent.text = currentText;
            yield return new WaitForSeconds(0.03f);
        }
    }

    public static void drawAttack(Vector2 startPos, Vector2 endPos, float range, Texture effectImg, bool left, float speedmult)
    {
        GameObject mask;
        if (left) mask = attackContainer.transform.Find("LeftAttackMask").gameObject;
        else mask = attackContainer.transform.Find("RightAttackMask").gameObject;
        GameObject RawImage = mask.transform.Find("RawImage").gameObject;


        //set effect
        RawImage img = RawImage.GetComponent<RawImage>();
        img.texture = effectImg;
        //adjust size based on range * 10
        RawImage.GetComponent<RectTransform>().sizeDelta = new Vector2(range * 10, RawImage.GetComponent<RectTransform>().sizeDelta.y);
        mask.GetComponent<RectTransform>().sizeDelta = new Vector2(range * 10, RawImage.GetComponent<RectTransform>().sizeDelta.y);
        //move vfx window to attack location 
        attackContainer.transform.position = startPos;
        Vector2 diff = startPos - endPos;
        mask.transform.localPosition = new Vector2(range * -5, 0);
        //calc angle
        float angle = Mathf.Atan2(diff.y, diff.x) * Mathf.Rad2Deg;
        attackContainer.transform.localEulerAngles = new Vector3(0, 0, angle);
        AnimateUI.setEffect(img, left, speedmult);
    }

    public static void playAttackHitEffect(Vector3 worldPos, EquipmentItem item, float effectiveness) //where effectiveness is a damage mult based on the part of the enemy hit/enemy state (eg stunned enemies would be 2)
    {
        //to play the effect we want to instantiate a gameobject with the item's particle system at the hit location, play it, and then destroy the object when done
        GameObject particleEffect = Instantiate(item.hitParticles, worldPos, Quaternion.identity);
        particleEffect.transform.LookAt(Player.playerObject.transform.position);
        ParticleSystem particleSystem = particleEffect.GetComponent<ParticleSystem>();
        //modify particle system based on effectiveness
        var main = particleSystem.main;
        main.startSpeed = effectiveness;
        main.startSize = effectiveness * .05f;
        //set colors
        if(effectiveness > 1)
        {
            //positive effectiveness
            var col = particleSystem.colorOverLifetime; col.enabled = true; Gradient grad = new Gradient();
            grad.SetKeys(new GradientColorKey[] { new GradientColorKey(Color.red, 0.0f), new GradientColorKey(Color.black, 1.0f) }, new GradientAlphaKey[] { new GradientAlphaKey(1.0f, 0.0f), new GradientAlphaKey(0.0f, 1.0f) });
            col.color = grad;
        }
        //set emissions
        if(effectiveness > 1)
        {
            var em = particleSystem.emission; em.enabled = true;
            em.SetBursts(
                new ParticleSystem.Burst[]{
                new ParticleSystem.Burst(0.0f, 24, 1, 0.020f),
                new ParticleSystem.Burst(0.5f, 12, 1, 0.010f)
                });
        }



        particleSystem.Play();
        //destroy when finished playing
        Destroy(particleEffect, particleSystem.main.duration); 
    }

    public static void playPartBreakEffect(Vector3 worldPos, EnemyPart part, GameObject particles)
    {
        //to play the effect we want to instantiate a gameobject with the item's particle system at the hit location, play it, and then destroy the object when done
        GameObject particleEffect = Instantiate(particles, worldPos, Quaternion.identity);
        particleEffect.transform.LookAt(Player.playerObject.transform.position);
        ParticleSystem particleSystem = particleEffect.GetComponent<ParticleSystem>();
        //modify particle system based on effectiveness
        particleSystem.Play();
        //destroy when finished playing
        Destroy(particleEffect, particleSystem.main.duration);
    }

    public static void addCurrency(int amt)
    {
        //animate adding of currency
    }

}
