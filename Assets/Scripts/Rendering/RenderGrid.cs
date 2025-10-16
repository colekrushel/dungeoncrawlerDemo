using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using static UnityEditor.Experimental.GraphView.GraphView;

public class RenderGrid : MonoBehaviour
{
    private DungeonGrid[] grids;
    [SerializeField] TextAsset[] bottomGridFiles;
    [SerializeField] TextAsset[] northGridFiles;
    [SerializeField] TextAsset[] southGridFiles;
    [SerializeField] TextAsset[] eastGridFiles;
    [SerializeField] TextAsset[] westGridFiles;
    [SerializeField] TextAsset[] topGridFiles;
    [SerializeField] GameObject RenderedGrids;
    private void Start()
    {
        gameObject.transform.position = Vector3.zero;
        GridDicts.init();
        //for each zone/side
        loadZone(eastGridFiles, "east");
        loadZone(northGridFiles, "north");
        loadZone(southGridFiles, "south");
        loadZone(westGridFiles, "west");
        loadZone(bottomGridFiles, "bottom");
        //determine current active grid
        //asign current active grid to utils
        GridUtils.grids = grids;
        //prompt map update
        UIUtils.updateMap();


    }

    private void loadZone(TextAsset[] gridFiles, string side)
    {
        //assign grid being loaded currently
        grids = new DungeonGrid[gridFiles.Length];
        GameObject currZone = new GameObject();
        currZone.name = side;
        currZone.transform.SetParent(gameObject.transform);
        //for each layer
        for (int i = 0; i < gridFiles.Length; i++)
        {
            TextAsset file = gridFiles[i];
            //load rooms
            grids[i] = JsonUtility.FromJson<DungeonGrid>(file.text);
            grids[i].fillGridVariable();
            grids[i].layer = i;
            //render cell objects ; cant be rendered in a class function as serializable objects cannot inherit monobehavior to be loaded from json

            GameObject newlayer;
            if (RenderedGrids != null)
            {
                Debug.Log("pre-rendered layer" + i);
                //if a pre-rendered object is attached to this script then use that instead
                //newlayer = Instantiate(RenderedGrids.gameObject.transform.GetChild(i).gameObject);
                newlayer = RenderedGrids.gameObject.transform.GetChild(i).gameObject;
                //newlayer.transform.SetParent(gameObject.transform);
                //newlayer.name = "layer" + (i);
                loadGridData(grids[i], newlayer);
            }
            else
            {
                //Debug.Log("rendering layer " + i);
                newlayer = new GameObject();
                newlayer.transform.SetParent(currZone.transform);
                newlayer.name = "layer" + (i);
                renderCellGrid(grids[i], newlayer);
            }
            newlayer.transform.position = gameObject.transform.position + new Vector3(0, i, 0);
        }
        //rotate zone based on side
        switch (side.ToLower())
        {
            case "bottom":
                break;
            case "north":
                currZone.transform.eulerAngles = new Vector3(-90, 0, 0);
                currZone.transform.position += new Vector3(0, 0, 18);
                break;
            case "east":
                currZone.transform.eulerAngles = new Vector3(0, 0, 90);
                currZone.transform.position += new Vector3(18, 0, 0);
                break;
            case "south":
                currZone.transform.eulerAngles = new Vector3(90, 0, 0);
                currZone.transform.position += new Vector3(0, 14, -4);
                break;
            case "west":
                currZone.transform.eulerAngles = new Vector3(0, 0, -90);
                currZone.transform.position += new Vector3(-4, 14, 0);
                break;
        }
    }

    //called for pre-rendered grids only
    private void loadGridData(DungeonGrid grid, GameObject layer)
    {
        //to associate each dungeoncell with its respective gameobject in the scene, we can loop through every cell object in the scene and assign it to its cell based on its world position
        foreach (Transform child in layer.transform)
        {
            DungeonCell cell = grid.getCell((int)child.position.x, (int)child.position.z);
            cell.createCellObject(child.gameObject);
        }
        List<List<DungeonCell>> cellGrid = grid.getCellGrid();
        for (int i = 0; i < cellGrid.Count; i++)
        {
            for (int j = 0; j < cellGrid[i].Count; j++)
            {
                //dont load cell if type is empty
                if (cellGrid[i][j].type == "Empty") continue;
                Vector2Int cellPos = cellGrid[i][j].getPos();
                loadCellData(cellGrid[i][j], cellPos.x, cellPos.y, grid);
            }
        }
    }

    //called for pre-rendered grids only
    private void loadCellData(DungeonCell cell, int cellX, int cellY, DungeonGrid grid)
    {
        cell.layer = grid.layer;
        //load entity data
        //we have cell type and entity facing position, assign subclasses and open
        if (cell.type != "None" && cell.type != "Empty")
        {
            if (cell.type == "Empty") cell.traversible = false;
            //just assign subclass because model is already present
            assignEntitySubclass(cell);
        }
        //breakable construct is saved from initial render but its onbreak action needs to be reassigned
        BreakableConstruct breakable = cell.getCellObject().GetComponent<BreakableConstruct>();
        if (breakable != null)
        {
            breakable.onBreak = cell.breakObject;
            cell.breakableConstruct = breakable;
        }

        //GameObject cellObject = cell.getCellObject();
        //BreakablePart[] parts = cellObject.GetComponentsInChildren<BreakablePart>();
        //if (parts.Length > 0)
        //{
        //    Debug.Log("parts found at " + cell.gridX + ", " + cell.gridY);
        //    cell.breakableConstruct = cellObject.AddComponent<BreakableConstruct>();
        //    cell.breakableConstruct.setParts(parts);
        //    cell.breakableConstruct.onBreak = cell.breakObject;
        //}
    }

    private void renderCellGrid(DungeonGrid grid, GameObject layer)
    {
        List<List<DungeonCell>> cellGrid = grid.getCellGrid();
        for (int i = 0; i < cellGrid.Count; i++)
        {
            for (int j = 0; j < cellGrid[i].Count; j++)
            {
                //dont render cell if type is empty
                if (cellGrid[i][j].type == "Empty") continue;
                Vector2Int cellPos = cellGrid[i][j].getPos();
                GameObject newCell = renderCell(cellGrid[i][j], cellPos.x, cellPos.y, grid);
                grid.instantiateCellObject(newCell, cellPos.x, cellPos.y);
                newCell.transform.SetParent(layer.transform);
                //cellGridRow.Add(newCell);
            }
        }
    }



    private GameObject renderCell(DungeonCell cell, int cellX, int cellY, DungeonGrid grid)
    {
        //parent container for cell
        GameObject cellObject = new GameObject();
        string[] walls = cell.getWalls();

        cell.layer = grid.layer;

        //assign floor
        string floorURL = "";
        //use different floors for indoors tiles/ tiles on buildings (layer > 0? maybe should check if floor below has a ceiling instead)
        if (cell.hasCeiling || cell.layer > 0)
        {
            floorURL = "Prefabs/" + cell.getFloorToAssign() + "Indoors";
        } else
        {
            floorURL = "Prefabs/" + cell.getFloorToAssign();
        }

        //for certain tiles we want to change what tiles is placed on what tiles are bordering it
        GameObject floor;
        if (cell.getFloorToAssign() == "grass1")
        {
            floor = getBorderedTile(cell);

        } else
        {
            floor = Instantiate(Resources.Load<GameObject>(floorURL));
        }


        floor.transform.parent = cellObject.transform;
        //types that want data but dont want rendering
        if (cell.type == "StairsDown")
        {
            floor.SetActive(false);
        }

        //assign walls

        //assign doorways (between ceiling and non ceiling tiles
        List<String> doorways = new List<String>();
        List<String> railings = new List<String>();
        if (cell.hasCeiling)
        {
            DungeonCell N = grid.getCellInDirection(cell, "N");
            if (N != null && !N.hasCeiling) doorways.Add("N");
            DungeonCell E = grid.getCellInDirection(cell, "E");
            if (E != null && !E.hasCeiling) doorways.Add("E");
            DungeonCell S = grid.getCellInDirection(cell, "S");
            if (S != null && !S.hasCeiling) doorways.Add("S");
            DungeonCell W = grid.getCellInDirection(cell, "W");
            if (W != null && !W.hasCeiling) doorways.Add("W");

        }
        else if(!cell.hasCeiling && cell.layer > 0 && cell.traversible)//assign railings between non-ground level non-ceiling and empty tiles
        {
            DungeonCell N = grid.getCellInDirection(cell, "N");
            if (N == null || N.type == "Empty") railings.Add("N");
            DungeonCell E = grid.getCellInDirection(cell, "E");
            if (E == null || E.type == "Empty") railings.Add("E");
            DungeonCell S = grid.getCellInDirection(cell, "S");
            if (S == null || S.type == "Empty") railings.Add("S");
            DungeonCell W = grid.getCellInDirection(cell, "W");
            if (W == null || W.type == "Empty") railings.Add("W");
        }
        
        for (int i = 0; i < walls.Length; i++)
        {
            //determine wall based on whether cell has a ceiling or not

            //wall types: (in order of priority)
            //breakable walls
            //windows (between indoors non-ground level walkable and empty tile where there is a wall)
            //indoor walls
            //ground level outdoors (fence)


            //model fillers where there are no walls
            //railings (between non-ground-level walkable and empty tile where there is no wall) [NO-WALL FILLER]
            //doorways (between ceiling and non-ceiling tiles where there is no wall) [NO-WALL FILLER]

            GameObject wall;
            if (walls[0] == cell.breakableWallDirection)
            {
                wall = Instantiate(Resources.Load<GameObject>("Prefabs/CellWallBreakable"));
            }
            else if (cell.hasCeiling && cell.layer > 0 && grid.getCellInDirection(cell, walls[0]) != null && grid.getCellInDirection(cell, walls[0]).type == "Empty")
            {
                wall = Instantiate(Resources.Load<GameObject>("Prefabs/BuildingWindow1"));
            }
            else if (cell.hasCeiling)
            {
                wall = Instantiate(Resources.Load<GameObject>("Prefabs/BuildingWall1"));
            }
            else
            {
                wall = Instantiate(Resources.Load<GameObject>("Prefabs/CellWall"));
            }
            wall.transform.position = floor.transform.position;
            wall.transform.SetParent(cellObject.transform);


            //offset walls to the edge of the tile
            //add or subtract offset of 1/8th of the tile width (1/2 of the post radius)
            float offsetAmt = (float)-cell.getWidth() / 16; ; //indoor wall offset
            if(!cell.hasCeiling) offsetAmt = (float)cell.getWidth() / 8;
            switch (walls[i])
            {
                case "N":
                    wall.transform.Rotate(0, 180, 0); //xyz
                    wall.transform.position += new Vector3(0, 0, offsetAmt);
                    if(cell.hasCeiling) doorways.Remove("N");
                    break;
                case "E":
                    wall.transform.Rotate(0, 270, 0); //xyz
                    wall.transform.position += new Vector3(offsetAmt, 0, 0);
                    if (cell.hasCeiling) doorways.Remove("E");
                    break;
                case "S":
                    wall.transform.Rotate(0, 0, 0); //xyz
                    wall.transform.position += new Vector3(0, 0, offsetAmt * -1);
                    if (cell.hasCeiling) doorways.Remove("S");
                    break;
                case "W":
                    wall.transform.Rotate(0, 90, 0); //xyz
                    wall.transform.position += new Vector3(offsetAmt * -1, 0, 0);
                    if (cell.hasCeiling) doorways.Remove("W");
                    break;
                default:
                    Debug.Log("default case?");
                    break;
            }


        }
        //make doorways and/or railings
        for (int i = 0; i < doorways.Count; i++)
        {

            GameObject wall = Instantiate(Resources.Load<GameObject>("Prefabs/BuildingDoorway1"));
            wall.transform.position = floor.transform.position;
            wall.transform.SetParent(cellObject.transform);
            //offset walls to the edge of the tile
            //add or subtract offset of 1/8th of the tile width (1/2 of the post radius)
            float offsetAmt = (float)-cell.getWidth() / 16; ; //indoor wall offset
            switch (doorways[i])
            {
                case "N":
                    wall.transform.Rotate(0, 180, 0); //xyz
                    wall.transform.position += new Vector3(0, 0, offsetAmt);
                    break;
                case "E":
                    wall.transform.Rotate(0, 270, 0); //xyz
                    wall.transform.position += new Vector3(offsetAmt, 0, 0);
                    break;
                case "S":
                    wall.transform.Rotate(0, 0, 0); //xyz
                    wall.transform.position += new Vector3(0, 0, offsetAmt * -1);
                    break;
                case "W":
                    wall.transform.Rotate(0, 90, 0); //xyz
                    wall.transform.position += new Vector3(offsetAmt * -1, 0, 0);
                    break;
            }
        }
        for (int i = 0; i < railings.Count; i++)
        {

            GameObject wall = Instantiate(Resources.Load<GameObject>("Prefabs/Railing1"));
            wall.transform.position = floor.transform.position;
            wall.transform.SetParent(cellObject.transform);
            //offset walls to the edge of the tile
            //add or subtract offset of 1/8th of the tile width (1/2 of the post radius)
            float offsetAmt = (float)-cell.getWidth() / 16; ; //indoor wall offset
            switch (railings[i])
            {
                case "N":
                    wall.transform.Rotate(0, 180, 0); //xyz
                    wall.transform.position += new Vector3(0, 0, offsetAmt);
                    break;
                case "E":
                    wall.transform.Rotate(0, 270, 0); //xyz
                    wall.transform.position += new Vector3(offsetAmt, 0, 0);
                    break;
                case "S":
                    wall.transform.Rotate(0, 0, 0); //xyz
                    wall.transform.position += new Vector3(0, 0, offsetAmt * -1);
                    break;
                case "W":
                    wall.transform.Rotate(0, 90, 0); //xyz
                    wall.transform.position += new Vector3(offsetAmt * -1, 0, 0);
                    break;
            }
        }
        //assign ceiling
        if (cell.hasCeiling)
        {
            GameObject ceil = Instantiate(Resources.Load<GameObject>("Prefabs/Ceiling1"));
            ceil.transform.SetParent(cellObject.transform);
            ceil.transform.position = new Vector3(0, 1, 0);
        }
        //models/entities
        //we have cell type and entity facing position, assign subclasses and open
        if (cell.type != "None" && cell.type != "Empty")
        {
            if (cell.type == "Empty") cell.traversible = false;
            initializeEntity(cell, cellObject);
        }

       
        

        //handle models for types (set when saving?
        string modelURL = "Prefabs/" + cell.getModelToAssign();
        if(cell.getModelToAssign() != "None")
        {
            Debug.Log(modelURL);
            GameObject model = Instantiate(Resources.Load<GameObject>(modelURL));
            model.transform.position = floor.transform.position;
            model.transform.SetParent(cellObject.transform);
        }

        //set breakable object now that all children have been assigned
        BreakablePart[] parts = cellObject.GetComponentsInChildren<BreakablePart>();
        if(parts.Length > 0)
        {
            cell.breakableConstruct = cellObject.AddComponent<BreakableConstruct>();
            cell.breakableConstruct.setParts(parts);
            cell.breakableConstruct.onBreak = cell.breakObject;
        }

        //add rendered object to global array so it doesnt have to be loaded again
        cellObject.transform.position = new Vector3(cellX, 0, cellY);
        return cellObject;
    }

    public void initializeEntity(DungeonCell cell, GameObject cellObject)
    {
        //exclude types that dont have models
        if(cell.type != "StairsDown" && cell.type != "Enemy")
        {
            //get model from dict
            GameObject model = Instantiate(GridDicts.typeToModel[cell.type]);
            model.transform.SetParent(cellObject.transform);
            cell.entity.entityInScene = model;
            //rotate 
            switch (cell.entity.facing)
            {
                case "N":
                    model.transform.Rotate(0, 180, 0); //xyz
                    break;
                case "E":
                    model.transform.Rotate(0, 270, 0); //xyz
                    break;
                case "S":
                    model.transform.Rotate(0, 0, 0); //xyz
                    break;
                case "W":
                    model.transform.Rotate(0, 90, 0); //xyz
                    break;
            }
        }

        //assign subclass
        assignEntitySubclass(cell);

    }

    void assignEntitySubclass(DungeonCell cell)
    {
        switch (cell.type)
        {
            case "OpenDoor":
                Door door;
                if(cell.entity.entityInScene == null)
                {
                    door = new Door(cell.entity, cell.getCellObject().GetComponentInChildren<Animator>());
                }
                else
                {
                    door = new Door(cell.entity);
                }
                door.open = true;
                cell.entity = door;
                break;
            case "ClosedDoor":
                Door cdoor;
                if (cell.entity.entityInScene == null)
                {
                    cdoor = new Door(cell.entity, cell.getCellObject().GetComponentInChildren<Animator>());
                }
                else
                {
                    cdoor = new Door(cell.entity);
                }
                cdoor.open = false;
                cell.entity = cdoor;
                break;
            case "StairsUp":
                cell.entity.interactable = false;
                cell.traversible = true;
                break;
            case "StairsDown":
                cell.entity.interactable = false;
                cell.traversible = true;
                //grab facing direction from the paired stairsup below the tile
                cell.entity.facing = grids[cell.layer - 1].getCell(cell.gridX, cell.gridY).entity.facing;
                break;
            case "Enemy":
                //spawn an enemy here
                //EnemyManager.spawnEnemy(cell.gridX, cell.gridY, cell.layer, cell.entity.dataString);
                cell.entity.interactable = false;
                cell.traversible = true;
                break;
        }
        cell.entity.layer = cell.layer;
    }

    GameObject getBorderedTile(DungeonCell cell)
    {
        string floorURL = "";
        //for grass tiles, we want to place a border wherever the neighboring tile is not a grass tile.

        //figure out which neighbors are grass
        List<string> neighbors = grids[cell.layer].getNighborsMatchingFilters(cell, "", "grass1");
        //grab the corresponding tile from resources
        switch (neighbors.Count)
        {
            case 0:
                floorURL = "Prefabs/lawnVariants/" + cell.floorToAssign + "FullBorder";
                break;
            case 1:
                floorURL = "Prefabs/lawnVariants/" + cell.floorToAssign + "ThreeBorder";
                break;
            case 2:
                //determine if corner or parallel
                //if first direction is opposite to the other direction then we want a parallel border
                if (neighbors[0] == GridUtils.getOppositeDirection(neighbors[1]))
                {
                    floorURL = "Prefabs/lawnVariants/" + cell.floorToAssign + "ParallelBorder";
                }
                else
                {
                    floorURL = "Prefabs/lawnVariants/" + cell.floorToAssign + "CornerBorder";
                }
                break;
            case 3:
                floorURL = "Prefabs/lawnVariants/" + cell.floorToAssign + "OneBorder";
                break;
            case 4:
                floorURL = "Prefabs/lawnVariants/" + cell.floorToAssign + "Default";
                break;

        }
        //rotate it to match physical appearance
        GameObject tile = Instantiate(Resources.Load<GameObject>(floorURL));
        tile.transform.Rotate(0, GridUtils.getBorderRotation(neighbors), 0);
        return tile;
    }

    public DungeonGrid getGrid(int layer)
    {
        Debug.Log("getting layer " + layer);
        return grids[layer];
    }

    public DungeonGrid[] getGrids()
    {
        return grids;
    }

}    


