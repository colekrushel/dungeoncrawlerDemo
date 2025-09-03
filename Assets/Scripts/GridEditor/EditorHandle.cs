using System.Collections;
using Unity.VisualScripting;
using UnityEditor.Build;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class EditorHandle : MonoBehaviour
{
    //options
    [SerializeField] int gridSize;
    [SerializeField] int wallSize;

    //globals
    [SerializeField] GameObject tileGrid;
    [SerializeField] public GameObject currSelected;
    Tile blankTile;
    float s;
    float tileSize;
    bool hasCeiling = false;
    [SerializeField] Sprite nullsprite;



    void Start()
    {
        //initialize params
        float h = tileGrid.GetComponent<RectTransform>().rect.height;
        float w = tileGrid.GetComponent<RectTransform>().rect.width;
        if (h > w) s = w; else s = h;
        Debug.Log(w);
        blankTile = Resources.Load<Tile>("Tiles/blankTile");
        //nullsprite = Resources.Load<Sprite>("Tiles/IconsWIPTransparentNoBorder_7");
        tileSize = w / gridSize - wallSize * 2;
        tileGrid.GetComponent<GridLayoutGroup>().cellSize = new Vector2(tileSize, tileSize);
        tileGrid.GetComponent<GridLayoutGroup>().spacing = new Vector2(wallSize * 2, wallSize * 2);
        drawGrid();

    }

    void drawGrid()
    {
        for (int i = 0; i < gridSize; i++)
        {
            for (int j = 0; j < gridSize; j++)
            {
                //tilemap.SetTile(new Vector3Int(j, i, 0), blankTile);
                //GameObject panel = new GameObject();
                //panel.AddComponent<CanvasRenderer>();
                //panel.AddComponent<RectTransform>();
                //panel.AddComponent<Image>();
                //panel.AddComponent<Button>();
                //panel.AddComponent<GraphicRaycaster>();
                GameObject panel = Instantiate(Resources.Load<GameObject>("Prefabs/editorTile"), new Vector3(0, 0, 0), Quaternion.identity);
                panel.transform.SetParent(tileGrid.transform, false);
                panel.transform.Find("Icon").GetComponent<RectTransform>().sizeDelta = new Vector2(tileSize, tileSize);
                panel.transform.Find("Icon").GetComponent<Image>().sprite = nullsprite;
                panel.transform.Find("N").GetComponent<Button>().onClick.AddListener(tileGridWallPress);
                panel.transform.Find("E").GetComponent<Button>().onClick.AddListener(tileGridWallPress);
                panel.transform.Find("S").GetComponent<Button>().onClick.AddListener(tileGridWallPress);
                panel.transform.Find("W").GetComponent<Button>().onClick.AddListener(tileGridWallPress);
                panel.GetComponent<Button>().onClick.AddListener(tileGridPress);

                //panel.GetComponent<Image>().sprite = null;

                //create panels to act as wall toggles
                //for (int k = 0; k < 4; k++)
                //{
                //    GameObject p = new GameObject();
                //    p.AddComponent<CanvasRenderer>();
                //    p.AddComponent<RectTransform>();
                //    RectTransform r = p.GetComponent<RectTransform>();
                //    p.AddComponent<Image>();
                //    p.AddComponent<Button>();
                //    p.GetComponent<Button>().onClick.AddListener(tileGridWallPress);
                //    switch (k)
                //    {
                //        case 0:
                //            r.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, -wallSize, wallSize);
                //            r.sizeDelta = new Vector2(tileSize, r.sizeDelta.y);
                //            break;
                //        case 1:
                //            r.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Right, -wallSize, wallSize);
                //            r.sizeDelta = new Vector2(r.sizeDelta.x, tileSize);
                //            break;
                //        case 2:
                //            r.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Bottom, -wallSize, wallSize);
                //            r.sizeDelta = new Vector2(tileSize, r.sizeDelta.y);
                //            break;
                //        case 3:
                //            r.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, -wallSize, wallSize);
                //            r.sizeDelta = new Vector2(r.sizeDelta.x, tileSize);
                //            break;
                //    }

                //    p.transform.SetParent(panel.transform, false);
                //}

            }
        }
    }


    public void tileGridPress()
    {
        //set clicked on tile's icon to match currently selected icon
        GameObject selectedObject = EventSystem.current.currentSelectedGameObject;
        Image tileImg = selectedObject.transform.Find("Icon").GetComponent<Image>();
        tileImg.sprite = currSelected.transform.Find("Icon").GetComponent<Image>().sprite;
        Image tileBckg = selectedObject.transform.Find("Background").GetComponent<Image>();
        tileBckg.color = currSelected.transform.Find("Background").GetComponent<Image>().color;
        if (hasCeiling) {

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
            //currSelected.GetComponent<Image>().color = Color.white; 
        } else 
        { 
            hasCeiling = true;
            //currSelected.GetComponent<Image>().color = Color.cyan;
        }
    }


}
