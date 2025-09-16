using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;
using UnityEngine.UI;
using static UnityEngine.EventSystems.EventTrigger;

public class EditorHandle : MonoBehaviour
{
    //options
    [SerializeField] int gridSize;
    [SerializeField] int wallSize;

    //globals
    [SerializeField] GameObject tileGrid;
    [SerializeField] public GameObject currSelected;
    GameObject tileGridParent;
    Tile blankTile;
    int s;
    float tileSize;
    bool hasCeiling = false;
    [SerializeField] Sprite nullsprite;
    [SerializeField] Sprite restrictedsprite;
    [SerializeField] Sprite downstairssprite;
    int currLayer = 1;

    

    //handles the ui for the grid editor.
    //layers above layer 1 require a ceiling on the corresponding spot in the previous layer to exist and be elligible for editing. blank floors on higher layer will use the lower tile's ceiling as its floor.

    void Start()
    {
        //initialize params and layers
        float h = tileGrid.GetComponent<RectTransform>().rect.height;
        float w = tileGrid.GetComponent<RectTransform>().rect.width;
        blankTile = Resources.Load<Tile>("Tiles/blankTile");
        tileSize = w / gridSize - wallSize * 2;
        tileGridParent = tileGrid.transform.parent.gameObject;
        foreach (Transform child in tileGridParent.transform)
        {
            GameObject cgrid = child.gameObject;
            cgrid.GetComponent<GridLayoutGroup>().cellSize = new Vector2(tileSize, tileSize);
            cgrid.GetComponent<GridLayoutGroup>().spacing = new Vector2(wallSize * 2, wallSize * 2);
        }
        GridDicts.init();

        drawGrid();

    }

    void drawGrid()
    {
        int index = 1;
        //initialize each layer
        foreach (Transform child in tileGridParent.transform)
        {
            GameObject cgrid = child.gameObject;
            for (int i = 0; i < gridSize; i++)
            {
                for (int j = 0; j < gridSize; j++)
                {
                    //all tiles in layers above 1 will be restricted by default.
                    GameObject panel = Instantiate(Resources.Load<GameObject>("Prefabs/editorTile"), new Vector3(0, 0, 0), Quaternion.identity);
                    panel.transform.SetParent(cgrid.transform, false);
                    panel.transform.Find("Icon").GetComponent<RectTransform>().sizeDelta = new Vector2(tileSize, tileSize);
                    if (index > 1)
                    {
                        panel.transform.Find("Icon").GetComponent<Image>().sprite = restrictedsprite;
                    } else
                    {
                        panel.transform.Find("Icon").GetComponent<Image>().sprite = nullsprite;
                    }

                    panel.transform.Find("N").GetComponent<Button>().onClick.AddListener(tileGridWallPress);
                    panel.transform.Find("N").GetComponent<RectTransform>().sizeDelta = new Vector2(tileSize, wallSize);
                    panel.transform.Find("E").GetComponent<Button>().onClick.AddListener(tileGridWallPress);
                    panel.transform.Find("E").GetComponent<RectTransform>().sizeDelta = new Vector2(wallSize, tileSize);
                    panel.transform.Find("S").GetComponent<Button>().onClick.AddListener(tileGridWallPress);
                    panel.transform.Find("S").GetComponent<RectTransform>().sizeDelta = new Vector2(tileSize, wallSize);
                    panel.transform.Find("W").GetComponent<Button>().onClick.AddListener(tileGridWallPress);
                    panel.transform.Find("W").GetComponent<RectTransform>().sizeDelta = new Vector2(wallSize, tileSize);
                    panel.GetComponent<Button>().onClick.AddListener(tileGridPress);
                    //assign right click listening functionality to grid items (why isnt this default...)
                    EventTrigger trigger = panel.AddComponent<EventTrigger>(); 
                    var pointerDown = new EventTrigger.Entry();
                    pointerDown.eventID = EventTriggerType.PointerDown;
                    pointerDown.callback.AddListener((e) => rightClickTile(e));
                    trigger.triggers.Add(pointerDown);
                }
            }
            //set inactive if index isnt currlayer
            if (index != currLayer) cgrid.SetActive(false);
            index++;
        }

    }

    public void rightClickTile(BaseEventData eventData)
    {
        PointerEventData e = eventData as PointerEventData; //cast it to pointer event data even though it doesn't have automatic conversion
        GameObject selectedObject = e.pointerEnter.transform.parent.gameObject; //event is intercepted by its child (ceiling obj)... bandage fix it
        if (e.button == PointerEventData.InputButton.Right && selectedObject.transform.Find("Icon").gameObject.GetComponent<Image>().sprite != nullsprite)
        {
            Debug.Log("Right click");
            //toggle this item's entitymenu


            GameObject menu = selectedObject.transform.GetChild(selectedObject.transform.childCount - 1).gameObject;
            menu.transform.position = new Vector3(600, 350, 0);
            menu.SetActive(true);
        }
            
    }

    public void tileGridPress()
    {
        GameObject selectedObject = EventSystem.current.currentSelectedGameObject;
        //set clicked on tile's icon to match currently selected icon
        //check if tile is restricted; cannot edit restricted tiles.
        if (selectedObject.transform.Find("Icon").GetComponent<Image>().sprite == restrictedsprite)
        {
            //do nothing
        } else
        {
            Image tileImg = selectedObject.transform.Find("Icon").GetComponent<Image>();
            tileImg.sprite = currSelected.transform.Find("Icon").GetComponent<Image>().sprite;
            Image tileBckg = selectedObject.transform.Find("Background").GetComponent<Image>();
            tileBckg.color = currSelected.transform.Find("Background").GetComponent<Image>().color;
            Image tileCeil = selectedObject.transform.Find("Ceiling").GetComponent<Image>();
            tileCeil.color = currSelected.transform.Find("Ceiling").GetComponent<Image>().color;
            if (hasCeiling && currLayer != 3)
            {
                //if a ceiling is placed, then unrestrict the above tile.
                int index = selectedObject.transform.GetSiblingIndex();
                GameObject aboveGrid = tileGridParent.transform.GetChild(currLayer).gameObject;
                GameObject aboveTile = aboveGrid.transform.GetChild(index).gameObject;
                aboveTile.transform.Find("Icon").GetComponent<Image>().sprite = nullsprite;
            } else if (!hasCeiling && currLayer != 3)
            {
                //if a ceiling is removed, make sure the above tile is restricted.
                int index = selectedObject.transform.GetSiblingIndex();
                GameObject aboveGrid = tileGridParent.transform.GetChild(currLayer).gameObject;
                GameObject aboveTile = aboveGrid.transform.GetChild(index).gameObject;
                aboveTile.transform.Find("Icon").GetComponent<Image>().sprite = restrictedsprite;
                //Debug.Log(tileImg.sprite.name);
                //if stairsup is selected, then add a stairsdown to the above tile. if not possible then prevent stairs from being places
                if (tileImg.sprite.name == "iconsWIPTransparentNoBorder3_10")
                {
                    Debug.Log("setting above");
                    aboveTile.transform.Find("Icon").GetComponent<Image>().sprite = downstairssprite;
                }
            }
        }


    }

    public void tileGridWallPress()
    {
        //toggle the wall's color
        GameObject selectedObject = EventSystem.current.currentSelectedGameObject;
        if (selectedObject.GetComponent<Image>().color == Color.black)
        {
            selectedObject.GetComponent<Image>().color = Color.white;
        }
        else
        {
            selectedObject.GetComponent<Image>().color = Color.black;
        }


    }

    public void iconSelect()
    {
        //set currently selected icon to the clicked on icon
        GameObject selectedObject = EventSystem.current.currentSelectedGameObject;
        Sprite selectedSprite = selectedObject.GetComponent<Image>().sprite;
        currSelected.transform.Find("Icon").GetComponent<Image>().sprite = selectedSprite;
    }

    public void floorSelect()
    {
        //floor type is dictated by color
        GameObject selectedObject = EventSystem.current.currentSelectedGameObject;
        Color selectedColor = selectedObject.GetComponent<Image>().color;
        currSelected.transform.Find("Background").GetComponent<Image>().color = selectedColor;
    }

    public void clearSelected()
    {
        currSelected.transform.Find("Background").GetComponent<Image>().color = Color.white;
        currSelected.transform.Find("Icon").GetComponent<Image>().sprite = nullsprite;
    }

    public void toggleCeiling()
    {
        if (hasCeiling)
        {
            hasCeiling = false;
            currSelected.transform.Find("Ceiling").GetComponent<Image>().color = Color.white;
        } else
        {
            hasCeiling = true;
            currSelected.transform.Find("Ceiling").GetComponent<Image>().color = Color.black;
        }
    }

    public void switchLayer(int layer)
    {
        //set previous layer to inactive and button's layer to active
        tileGridParent.transform.GetChild(currLayer - 1).gameObject.SetActive(false);
        tileGridParent.transform.GetChild(layer - 1).gameObject.SetActive(true);
        currLayer = layer;
    }

    public void importGrid()
    {
        GameObject selectedObject = EventSystem.current.currentSelectedGameObject;
        string fileName = selectedObject.transform.Find("importName").GetComponent<TMP_InputField>().text;
        //for each layer
        for(int i = 0; i < 3; i++)
        {
            string path = Application.dataPath + "/Scripts/Rendering/" + fileName + (i+1) + ".txt";
            //fetch file
            if (File.Exists(path))
            {
                var sr = File.OpenText(path);
                string json = sr.ReadLine();
                //read json
                DungeonGrid grid = JsonUtility.FromJson<DungeonGrid>(json);
                //iterate through each cell in the grid and change editor to match
                for (int j = 0; j < grid.cells.Length; j++)
                {
                    DungeonCell cell = grid.cells[j];
                    CellEntity entity = cell.entity;
                    if (entity == null) entity = new CellEntity();
                    GameObject editorCell = tileGridParent.transform.GetChild(i).GetChild(j).gameObject;
                    //handle walls N, E, S, W
                    //reset walls to be blank
                    for(int w = 0; w < 4; w++)
                    {
                        editorCell.transform.GetChild(w).gameObject.GetComponent<Image>().color = Color.white;
                    }
                    //then fill with walls from data
                    foreach (string wall in cell.walls){
                        editorCell.transform.Find(wall).gameObject.GetComponent<Image>().color = Color.black;
                    }
                    //handle background/floor
                    Color color = GridDicts.floorToColor[cell.floorToAssign];
                    editorCell.transform.Find("Background").gameObject.GetComponent<Image>().color = color;

                    //handle icon

                    Sprite s = GridDicts.typeToSprite[cell.type];
                    editorCell.transform.Find("Icon").gameObject.GetComponent<Image>().sprite = s;

                    //handle ceiling
                    if(cell.hasCeiling) editorCell.transform.Find("Ceiling").gameObject.GetComponent<Image>().color = Color.black;

                    //populate entity info if not empty
                    if(cell.type != "Empty" && cell.type != "None")
                    {
                        GameObject menu = editorCell.transform.Find("entityMenu").gameObject;
                        menu.transform.Find("linkX").gameObject.GetComponent<TMP_InputField>().text = entity.targetx.ToString();
                        menu.transform.Find("linkY").gameObject.GetComponent<TMP_InputField>().text = entity.targety.ToString();//invert?
                        menu.transform.Find("Facing").gameObject.GetComponent<TMP_InputField>().text = entity.facing;
                    }
                }

            } else
            {
                Debug.Log(path + " doesnt exist");
                return;
            }

        }
    }

    public void saveGrid() // 3(gridSize^2 * 6) objects iterated through
    {
        GameObject selectedObject = EventSystem.current.currentSelectedGameObject;
        string fileName = selectedObject.transform.Find("gridName").GetComponent<TMP_InputField>().text;
        int clayer = 1;

        foreach (Transform child in tileGridParent.transform)
        {
            GameObject cgrid = child.gameObject;
            DungeonGrid grid = new DungeonGrid();
            grid.width = gridSize;
            grid.height = gridSize;
            //loop through each cell on the grid and translate its data into a cell object
            int cellindex = 0;
            List<DungeonCell> cellList = new List<DungeonCell>();
            foreach (Transform tile in cgrid.transform)
            {
                DungeonCell cell = new DungeonCell();
                cell.gridX = cellindex % gridSize;
                cell.gridY = cellindex / gridSize;
                //inverse y pos because rendering has (0,0) corner at bottom left but editor has (0,0) corner at top left
                cell.gridY = Mathf.Abs(cell.gridY - (gridSize - 1));
                //current gameobject is the parent of the tile object; data is stored in children, so we must check those too
                int i = 0;
                List<string> walls = new List<string>();

                CellEntity entity;
                foreach (Transform param in tile.transform)
                {
                    GameObject p = param.gameObject;
                    //N, E, S, W, Background, Icon, Ceiling
                    //add wall data

                    if (i < 4)
                    {
                        if (p.GetComponent<Image>().color == Color.black)
                        {
                            walls.Add(p.name);
                        }
                    }
                    else if (i == 4)//background (floor) data
                    {
                        cell.floorType = p.gameObject.GetComponent<Image>().color.ToString();
                        cell.floorToAssign = GridDicts.colorToFloor[p.gameObject.GetComponent<Image>().color];

                    }
                    else if (i == 5)//icon (type) data
                    {
                        cell.type = GridDicts.spriteToType[p.GetComponent<Image>().sprite];
                        if (cell.type == "None" || cell.type == "Empty")
                        {
                            cell.traversible = true;
                        }
                    } else if (i == 6)//ceiling 
                    {
                        if (p.GetComponent<Image>().color == Color.black) cell.hasCeiling = true;
                    } else if ((i == 7)) //entity info
                    {
                        //switch based on type
                        if (cell.type != "None" && cell.type != "Empty")
                        {
                            //TEST HOW SUBCLASSES GET EXPORTED
                            entity = new CellEntity();
                            //get info from menu
                            GameObject menu = p;
                            //menu.SetActive(true);
                            entity.targetx = int.Parse(menu.transform.Find("linkX").gameObject.GetComponent<TMP_InputField>().text);
                            entity.targety = int.Parse(menu.transform.Find("linkY").gameObject.GetComponent<TMP_InputField>().text); //invert?
                            entity.xpos = cell.gridX;
                            entity.ypos = cell.gridY;
                            entity.facing = menu.transform.Find("Facing").gameObject.GetComponent<TMP_InputField>().text;
                            cell.entity = entity;
                            entity.interactable = true;
                            //menu.SetActive(false);

                        } 

                    }

                    i++;
                }
                cell.walls = walls.ToArray();
                //export cellentity data

                
                cellList.Add(cell);
                cellindex++;
            }
            grid.cells = cellList.ToArray();
            //export json
            string json = JsonUtility.ToJson(grid);
            Debug.Log(json);
            string cfileName = fileName + clayer + ".txt";
            string path = Application.dataPath + "/Scripts/Rendering/" + cfileName;

            if (File.Exists(path))
            {
                Debug.Log(cfileName + " already exists.");
                return;
            }
            var sr = File.CreateText(path);
            sr.WriteLine(json);
            sr.Close();
            clayer++;
            AssetDatabase.Refresh();
        }
    }


}
