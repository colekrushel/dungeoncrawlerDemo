using System;
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
    GameObject lastSelectedEditorTile;
    string globalPropName = "";
    string globalPropOrientation = "";

    

    //handles the ui for the grid editor.
    //layers above layer 1 require a ceiling on the corresponding spot in the previous layer to exist and be elligible for editing. blank floors on higher layer will use the lower tile's ceiling as its floor.

    void Start()
    {
        //initialize params and layers
        float h = tileGrid.GetComponent<RectTransform>().rect.height;
        float w = tileGrid.GetComponent<RectTransform>().rect.width;
        blankTile = Resources.Load<Tile>("Tiles/blankTile");
        tileSize = w / gridSize - wallSize * 2;
        //tileSize = 50;
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

    private void Update()
    {
        
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
                    GameObject panel = Instantiate(Resources.Load<GameObject>("Prefabs/UI/editorTile"), new Vector3(0, 0, 0), Quaternion.identity);
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
                    panel.transform.Find("N").GetComponent<RectTransform>().localPosition = new Vector3(0, wallSize/2, 0);
                    panel.transform.Find("E").GetComponent<Button>().onClick.AddListener(tileGridWallPress);
                    panel.transform.Find("E").GetComponent<RectTransform>().sizeDelta = new Vector2(wallSize, tileSize);
                    panel.transform.Find("E").GetComponent<RectTransform>().localPosition = new Vector3(wallSize/2, 0, 0);
                    panel.transform.Find("S").GetComponent<Button>().onClick.AddListener(tileGridWallPress);
                    panel.transform.Find("S").GetComponent<RectTransform>().sizeDelta = new Vector2(tileSize, wallSize);
                    panel.transform.Find("S").GetComponent<RectTransform>().localPosition = new Vector3(0, -wallSize / 2, 0);
                    panel.transform.Find("W").GetComponent<Button>().onClick.AddListener(tileGridWallPress);
                    panel.transform.Find("W").GetComponent<RectTransform>().sizeDelta = new Vector2(wallSize, tileSize);
                    panel.transform.Find("W").GetComponent<RectTransform>().localPosition = new Vector3(-wallSize / 2, 0, 0);
                    panel.GetComponent<Button>().onClick.AddListener(tileGridPress);
                    //assign right click listening functionality to grid items (why isnt this default...)
                    EventTrigger trigger = panel.AddComponent<EventTrigger>(); 
                    var pointerDown = new EventTrigger.Entry();
                    pointerDown.eventID = EventTriggerType.PointerDown;
                    pointerDown.callback.AddListener((e) => rightClickTile(e));
                    trigger.triggers.Add(pointerDown);
                    //detect if pointer is held when entering element (allow user to hold down leftclick to fill tiles)
                    var pointerEnter = new EventTrigger.Entry();
                    pointerEnter.eventID = EventTriggerType.PointerEnter;
                    pointerEnter.callback.AddListener((e) => tileGridEnter(e));
                    trigger.triggers.Add(pointerEnter);
                }
            }
            //set inactive if index isnt currlayer
            if (index != currLayer) cgrid.SetActive(false);
            index++;
        }

    }

    public void tileGridEnter(BaseEventData eventData)
    {
        PointerEventData e = eventData as PointerEventData; //cast it to pointer event data even though it doesn't have automatic conversion
        GameObject selectedObject = e.pointerEnter.transform.parent.gameObject; //event is intercepted by its child (ceiling obj)... bandage fix it
        if (e.button == PointerEventData.InputButton.Left && Input.GetMouseButton(0))
        {
            tileGridPress(selectedObject);
        }
    }

    public void rightClickTile(BaseEventData eventData)
    {
        PointerEventData e = eventData as PointerEventData; //cast it to pointer event data even though it doesn't have automatic conversion
        GameObject selectedObject = e.pointerEnter.transform.parent.gameObject; //event is intercepted by its child (ceiling obj)... bandage fix it
        if(lastSelectedEditorTile != null)onPropStringChange();//check if the prev tile's prop string changed
        lastSelectedEditorTile = selectedObject;
        if (e.button == PointerEventData.InputButton.Right)
        {
            //toggle this item's entitymenu
            GameObject menu = selectedObject.transform.GetChild(selectedObject.transform.childCount - 1).gameObject; //get last child
            menu.transform.position = new Vector3(600, 350, 0);
            menu.SetActive(true);
        }
            
    }

    //move actions into separate func so method has the object of taking the selected object from the event system or being passed in one
    public void handleTileGridPressBehavior(GameObject selectedObject)
    {
        Image tileImg = selectedObject.transform.Find("Icon").GetComponent<Image>();
        tileImg.sprite = currSelected.transform.Find("Icon").GetComponent<Image>().sprite;
        Image tileBckg = selectedObject.transform.Find("Background").GetComponent<Image>();
        tileBckg.color = currSelected.transform.Find("Background").GetComponent<Image>().color;
        Image tileCeil = selectedObject.transform.Find("Ceiling").GetComponent<Image>();
        tileCeil.color = currSelected.transform.Find("Ceiling").GetComponent<Image>().color;
        //prop placement handlingGameObject menu = selectedObject.transform.Find("entityMenu").gameObject;
        GameObject menu = selectedObject.transform.Find("entityMenu").gameObject;
        if (tileImg.sprite == GridDicts.typeToSprite["Prop"])
        {
            menu.transform.Find("PropString").gameObject.GetComponent<TMP_InputField>().text = globalPropName;
            menu.transform.Find("PropPos").gameObject.GetComponent<TMP_InputField>().text = globalPropOrientation;
        }
        if (hasCeiling && currLayer != 3)
        {
            //if a ceiling is placed, then unrestrict the above tile.
            int index = selectedObject.transform.GetSiblingIndex();
            GameObject aboveGrid = tileGridParent.transform.GetChild(currLayer).gameObject;
            GameObject aboveTile = aboveGrid.transform.GetChild(index).gameObject;
            aboveTile.transform.Find("Icon").GetComponent<Image>().sprite = nullsprite;
        }
        else if (!hasCeiling && currLayer != 3)
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

    public void tileGridPress()
    {
        GameObject selectedObject = EventSystem.current.currentSelectedGameObject;
        handleTileGridPressBehavior(selectedObject);
    }

    public void tileGridPress(GameObject selectedObject)
    {
        handleTileGridPressBehavior(selectedObject);
    }

    public void tileGridWallPress()
    {
        //toggle the wall's color
        GameObject selectedObject = EventSystem.current.currentSelectedGameObject;
        Color wallColor = GameObject.Find("walltype").transform.Find("color").GetComponent<Image>().color;
        if (selectedObject.GetComponent<Image>().color == Color.black)
        {
            selectedObject.GetComponent<Image>().color = Color.white;
        }
        else
        {
            selectedObject.GetComponent<Image>().color = wallColor;
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

    public void cycleWall()
    {
        //wall type is determined by the color of the indicator;
        //black is default wall
        //green is a breakable wall
        GameObject selectedObject = EventSystem.current.currentSelectedGameObject;
        Color selectedColor = selectedObject.transform.parent.Find("color").GetComponent<Image>().color;
        Color nextColor = Color.black;
        if (selectedColor == Color.black) nextColor = Color.green;
        else if (selectedColor == Color.green) nextColor = Color.black;
        selectedObject.transform.parent.Find("color").gameObject.GetComponent<Image>().color = nextColor;
    }

    public void switchLayer(int layer)
    {
        //set previous layer to inactive and button's layer to active
        tileGridParent.transform.GetChild(currLayer - 1).gameObject.SetActive(false);
        tileGridParent.transform.GetChild(layer - 1).gameObject.SetActive(true);
        currLayer = layer;
    }

    public void scrollGrid(bool scrollUp)
    {
        //'scroll' the grid by adjusting its top position to see tiles that cant be seen when the grid is too big to display every tile at once

        //scroll all of them at once
        foreach (Transform child in tileGridParent.transform)
        {
            GameObject cgrid = child.gameObject;
            //set top param of each recttransform to be +- 200 depending on scroll direction
            RectTransform rect = child.GetComponent<RectTransform>();
            int offset = 200;
            if (scrollUp) offset = -200;
            rect.offsetMax = new Vector2(rect.offsetMax.x, rect.offsetMax.y + offset);
        }
    }

    public void onPropStringChange()
    {
        //used to place a prop sprite on the cell where the prop was placed if there is not already an entity sprite there
        GameObject selectedObject = lastSelectedEditorTile;
        if (lastSelectedEditorTile == null || selectedObject == null)
        {
            return;
        }
        GameObject menu = selectedObject.transform.Find("entityMenu").gameObject;
        Image tileImg = selectedObject.transform.Find("Icon").GetComponent<Image>();
        string pstring = menu.transform.Find("PropString").gameObject.GetComponent<TMP_InputField>().text;
        if (pstring != null && pstring != "" && tileImg.sprite == nullsprite)
        {
            tileImg.sprite = GridDicts.typeToSprite["Prop"];
        } 
        //if no prop then make sure propsprite is removed
        if((pstring == null || pstring == "") && tileImg.sprite == GridDicts.typeToSprite["Prop"])
        {
            tileImg.sprite = nullsprite;
        }
        
    }

    public void onGlobalPropStringChange()
    {
        globalPropName = this.transform.Find("propPreset").Find("propString").gameObject.GetComponent<TMP_InputField>().text;
    }

    public void onGlobalPropOrientChange()
    {
        globalPropOrientation = this.transform.Find("propPreset").Find("propOrientation").gameObject.GetComponent<TMP_InputField>().text;
    }

    public void importGrid()
    {
        GameObject selectedObject = EventSystem.current.currentSelectedGameObject;
        string fileName = selectedObject.transform.Find("importName").GetComponent<TMP_InputField>().text;
        //for each layer
        for(int i = 0; i < 3; i++)
        {
            string path = Application.dataPath + "/Resources/Grids/" + fileName + "-" + (i+1) + ".txt";
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
                    if(cell.breakableWallDirection != "")editorCell.transform.Find(cell.breakableWallDirection).gameObject.GetComponent<Image>().color = Color.green;
                    //handle background/floor
                    Color color = GridDicts.floorToColor[cell.floorToAssign];
                    editorCell.transform.Find("Background").gameObject.GetComponent<Image>().color = color;

                    //handle icon

                    Sprite s = GridDicts.typeToSprite[cell.type];
                    editorCell.transform.Find("Icon").gameObject.GetComponent<Image>().sprite = s;

                    //handle ceiling
                    if(cell.hasCeiling) editorCell.transform.Find("Ceiling").gameObject.GetComponent<Image>().color = Color.black;

                    //populate entity info if not empty
                    GameObject menu = editorCell.transform.Find("entityMenu").gameObject;
                    if (cell.type != "Empty" && cell.type != "None")
                    {
                        
                        menu.transform.Find("linkX").gameObject.GetComponent<TMP_InputField>().text = entity.targetx.ToString();
                        menu.transform.Find("linkY").gameObject.GetComponent<TMP_InputField>().text = entity.targety.ToString();//invert?
                        menu.transform.Find("Facing").gameObject.GetComponent<TMP_InputField>().text = entity.facing;
                        menu.transform.Find("DataString").gameObject.GetComponent<TMP_InputField>().text = entity.dataString;
                    }

                    //populate prop info
                    menu.transform.Find("PropString").gameObject.GetComponent<TMP_InputField>().text = cell.propToAssign;
                    menu.transform.Find("PropPos").gameObject.GetComponent<TMP_InputField>().text = cell.propPlacementOrientation;
                    if(cell.propToAssign != null && cell.propToAssign != "" && cell.type == "None")
                    {
                        editorCell.transform.Find("Icon").gameObject.GetComponent<Image>().sprite = GridDicts.typeToSprite["Prop"];
                    }
                    GameObject.Find("useCeilingAsFloor").transform.Find("Toggle").gameObject.GetComponent<Toggle>().isOn = cell.useCeilingAsFloor;

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
                        else if(p.GetComponent<Image>().color == Color.green)
                        {
                            walls.Add(p.name);
                            cell.breakableWallDirection = p.name;
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
                        if (cell.type == "None" )
                        {
                            cell.traversible = true;
                        }
                    } else if (i == 6)//ceiling 
                    {
                        if (p.GetComponent<Image>().color == Color.black) cell.hasCeiling = true;
                    } else if ((i == 7)) //entity info
                    {
                        GameObject menu = p;
                        //switch based on type
                        if (cell.type != "None" && cell.type != "Empty" && cell.type != "Prop")
                        {
                            //fill entity info from menu
                            entity = new CellEntity();
                            entity.targetx = int.Parse(menu.transform.Find("linkX").gameObject.GetComponent<TMP_InputField>().text);
                            entity.targety = int.Parse(menu.transform.Find("linkY").gameObject.GetComponent<TMP_InputField>().text); //invert?
                            entity.xpos = cell.gridX;
                            entity.ypos = cell.gridY;
                            entity.facing = menu.transform.Find("Facing").gameObject.GetComponent<TMP_InputField>().text;
                            entity.dataString = menu.transform.Find("DataString").gameObject.GetComponent<TMP_InputField>().text;
                            cell.entity = entity;
                            entity.interactable = true;
                        }
                        //prop info
                        cell.propToAssign = menu.transform.Find("PropString").gameObject.GetComponent<TMP_InputField>().text;
                        cell.propPlacementOrientation = menu.transform.Find("PropPos").gameObject.GetComponent<TMP_InputField>().text;
                        if(cell.propPlacementOrientation != null && cell.propPlacementOrientation != "") cell.traversible = true;
                    }

                    i++;
                }
                cell.walls = walls.ToArray();
                //export cellentity data

                //tileset data
                if (cell.hasCeiling)
                {
                    string tset = GameObject.Find("indoorTileset").transform.Find("Dropdown").Find("Label").GetComponent<TextMeshProUGUI>().text;
                    cell.tilesetPath = "Prefabs/gridTilesets/Indoor/" + tset;
                }
                else
                {
                    string tset = GameObject.Find("outdoorTileset").transform.Find("Dropdown").Find("Label").GetComponent<TextMeshProUGUI>().text;
                    cell.tilesetPath = "Prefabs/gridTilesets/Outdoor/" + tset;
                }
                //tileset override for tiles that might not have a ceiling but still want indoor tilesets (stairs)
                if(cell.type == "StairsUp") cell.tilesetPath = "Prefabs/gridTilesets/Indoor/" + GameObject.Find("indoorTileset").transform.Find("Dropdown").Find("Label").GetComponent<TextMeshProUGUI>().text;

                //ceiling as floor
                bool ceilingAsFloor = GameObject.Find("useCeilingAsFloor").transform.Find("Toggle").gameObject.GetComponent<Toggle>().isOn;
                cell.useCeilingAsFloor = ceilingAsFloor;
                cellList.Add(cell);
                cellindex++;
            }
            grid.cells = cellList.ToArray();
            //export json
            string json = JsonUtility.ToJson(grid);
            Debug.Log(json);
            string cfileName = fileName + "-" + clayer + ".txt";
            string path = Application.dataPath + "/Resources/Grids/" + cfileName;

            //overwrite existing?
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
