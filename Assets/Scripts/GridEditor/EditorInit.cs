using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;

public class EditorInit : MonoBehaviour
{
    Tilemap tilemap;
    [SerializeField] int width;
    [SerializeField] int height;
    Tile blankTile;
    void Start()
    {
        tilemap = gameObject.GetComponent<Tilemap>();
        blankTile = Resources.Load<Tile>("Tiles/blankTile");
        drawGrid();
        
    }

    void drawGrid()
    {
        for(int i = 0; i < width; i++)
        {
            for(int j = 0; j < height; j++)
            {
                Debug.Log("set");
                tilemap.SetTile(new Vector3Int(j, i, 0), blankTile);
            }
        }
    }

    
}
