using System;
using System.Collections.Generic;
using UnityEngine;


public class RenderGrid : MonoBehaviour
{
    private DungeonGrid[] grids;
    string currside;
    [SerializeField] TextAsset[] bottomGridFiles;
    [SerializeField] TextAsset[] northGridFiles;
    [SerializeField] TextAsset[] southGridFiles;
    [SerializeField] TextAsset[] eastGridFiles;
    [SerializeField] TextAsset[] westGridFiles;
    [SerializeField] TextAsset[] topGridFiles;
    [SerializeField] bool startInTutorial;
    [SerializeField] TextAsset[] tutorialGridFiles;
    [SerializeField] GameObject RenderedGrids;
    string[] indoorTilesetTypes = new string[] { "StairsUp" };
    private void Start()
    {
        gameObject.transform.position = Vector3.zero;
        GridDicts.init();
        //for each zone/side
        loadZone(eastGridFiles, "east");
        GridUtils.eastGrids = grids;
        loadZone(northGridFiles, "north");
        GridUtils.northGrids = grids;
        loadZone(southGridFiles, "south");
        GridUtils.southGrids = grids;
        loadZone(westGridFiles, "west");
        GridUtils.westGrids = grids;
        loadZone(bottomGridFiles, "bottom");
        GridUtils.bottomGrids = grids;
        if (startInTutorial)
        {
            loadZone(tutorialGridFiles, "tutorial");

        }
        //determine current active grid
        //asign current active grid to utils
        GridUtils.grids = grids;
        
        //prompt map update
        UIUtils.updateMap();

        //spawn enemies
        if (!startInTutorial)
        {
            EnemyManager.zoneSwitch("bottom");
        }
        


    }

    private void loadZone(TextAsset[] gridFiles, string side)
    {
        //assign grid being loaded currently
        grids = new DungeonGrid[gridFiles.Length];
        GameObject currZone = new GameObject();
        currside = side;
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
            case "tutorial":
                break;
            case "north":
                currZone.transform.eulerAngles = new Vector3(-90, 0, 0);
                //currZone.transform.position += new Vector3(0, 0.5f, 18);
                break;
            case "east":
                currZone.transform.eulerAngles = new Vector3(0, 0, 90);
                //currZone.transform.position += new Vector3(18, 0.5f, 0);
                break;
            case "south":
                currZone.transform.eulerAngles = new Vector3(90, 0, 0);
                //currZone.transform.position += new Vector3(0, 14.5f, -4);
                break;
            case "west":
                currZone.transform.eulerAngles = new Vector3(0, 0, -90);
                //currZone.transform.position += new Vector3(-4, 14.5f, 0);
                break;
        }
        currZone.transform.position += GridUtils.getZoneOffset(side.ToLower());
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
        //add cellframe to be toggled on for displaying enemy cell-based attacks
        //GameObject cellframe = Instantiate(Resources.Load<GameObject>("Prefabs/cellframe"));
        //cellframe.SetActive(false);
        //cellframe.transform.SetParent(cellObject.transform);
        string[] walls = cell.getWalls();

        cell.layer = grid.layer;

        //assign floor
        string floorURL = "";
        //use different floors for indoors tiles/ tiles on buildings (layer > 0? maybe should check if floor below has a ceiling instead)
        //if (cell.hasCeiling || cell.layer > 0)
        //{
        //    floorURL = "Prefabs/" + cell.getFloorToAssign() + "Indoors";
        //} else
        //{
        //    floorURL = "Prefabs/" + cell.getFloorToAssign();
        //}
        if (!cell.hasCeiling && cell.type != "StairsDown" && cell.type != "StairsUp" && cell.useCeilingAsFloor && cell.layer > 0)
        {
            floorURL = grids[cell.layer - 1].getCell(cell.gridX, cell.gridY).tilesetPath + "/Ceiling";
            
        } else
        {
            floorURL = cell.tilesetPath + "/Floor";
        }
        //for certain tiles we want to change what tiles is placed on what tiles are bordering it
        GameObject floor;
        if (cell.getFloorToAssign() == "grass1" || cell.getFloorToAssign() == "lawn" || cell.getFloorToAssign() == "water")
        {
            floor = getBorderedTile(cell, cell.getFloorToAssign());
            //override to make water tiles untraversible
            if (cell.getFloorToAssign() == "water") cell.traversible = false;

        } else
        {
            //handle dual border tiles - tiles that change appearance based on whether they are bordering a tile of a specific type
            //dual border tiles - bricks1water (changes bricks1 floor tiles depending on how many water tiles are bordering it
            if((cell.getFloorToAssign() == "bricks1") && !cell.hasCeiling && grids[cell.layer].getNighborsMatchingFilters(cell, "", "water").Count > 0)
            {
                floor = getBorderedTile(cell, cell.getFloorToAssign(), "water");
            }
            else
            {
                try
                {
                    floor = Instantiate(Resources.Load<GameObject>(floorURL));
                }
                catch (Exception e)
                {
                    Debug.Log("unable to set ceiling below as floor at " + cell.getPos() + "; defaulting to floor tile");
                    Debug.LogException(e);
                    floorURL = cell.tilesetPath + "/Floor";
                    floor = Instantiate(Resources.Load<GameObject>(floorURL));
                }
            }


        }


        floor.transform.parent = cellObject.transform;
        //types that want data but dont want rendering
        if (cell.type == "StairsDown")
        {
            floor.SetActive(false);
        }

        //assign walls

        //assign doorways (between ceiling and non ceiling tiles where there are no walls between
        List<String> doorways = new List<String>();
        List<String> railings = new List<String>();
        //don't put doorways between stairup tiles and indoor tiles
        if (cell.hasCeiling)
        {
            DungeonCell N = grid.getCellInDirection(cell, "N");
            if (N != null && !N.hasCeiling && !N.hasWall("S") && !cell.hasWall("N") && N.type != "StairsUp") doorways.Add("N");
            DungeonCell E = grid.getCellInDirection(cell, "E");
            if (E != null && !E.hasCeiling && !E.hasWall("W") && !cell.hasWall("E") && E.type != "StairsUp") doorways.Add("E");
            DungeonCell S = grid.getCellInDirection(cell, "S");
            if (S != null && !S.hasCeiling && !S.hasWall("N") && !cell.hasWall("S") && S.type != "StairsUp") doorways.Add("S");
            DungeonCell W = grid.getCellInDirection(cell, "W");
            if (W != null && !W.hasCeiling && !W.hasWall("E") && !cell.hasWall("W") && W.type != "StairsUp") doorways.Add("W");

        }
        else if(!cell.hasCeiling && cell.layer > 0)//assign railings between non-ground level non-ceiling and empty tiles where there are no walls
        {
            DungeonCell N = grid.getCellInDirection(cell, "N");
            if (N != null && N.type == "Empty" && !N.hasWall("S") && !cell.hasWall("N")) railings.Add("N");
            DungeonCell E = grid.getCellInDirection(cell, "E");
            if (E != null && E.type == "Empty" && !E.hasWall("W") && !cell.hasWall("E")) railings.Add("E");
            DungeonCell S = grid.getCellInDirection(cell, "S");
            if (S != null && S.type == "Empty" && !S.hasWall("N") && !cell.hasWall("S")) railings.Add("S");
            DungeonCell W = grid.getCellInDirection(cell, "W");
            if (W != null && W.type == "Empty" && !W.hasWall("E") && !cell.hasWall("W")) railings.Add("W");
        }
        //limit windows to 1 per tile
        int windowcount = 0;
        int maxwindows = 4;
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
            //Debug.Log(cell.tilesetPath);
            GameObject wall;
       
            if (walls[i] == cell.breakableWallDirection)
            {
                wall = Instantiate(Resources.Load<GameObject>(cell.tilesetPath + "/WallBreakable"));
            }
            else if (windowcount < maxwindows && (cell.hasCeiling && (cell.layer > 0 || cell.tilesetPath == "Prefabs/gridTilesets/Indoor/Office")) && grid.getCellInDirection(cell, walls[i]) != null && grid.getCellInDirection(cell, walls[i]).type == "Empty")
            {
                wall = Instantiate(Resources.Load<GameObject>(cell.tilesetPath + "/Window"));
                //wall = Instantiate(Resources.Load<GameObject>("Prefabs/gridTilesets/Indoor/Office/Window"));
                windowcount++;
            }
            else 
            {
                wall = Instantiate(Resources.Load<GameObject>(cell.tilesetPath + "/Wall"));
            }

            wall.transform.position = floor.transform.position;
            wall.transform.SetParent(cellObject.transform);


            //offset walls to the edge of the tile
            //add or subtract offset of 1/8th of the tile width (1/2 of the post radius)
            float offsetAmt = (float)-cell.getWidth() / 16; ; //indoor wall offset
            if(!cell.hasCeiling && Array.IndexOf(indoorTilesetTypes, cell.type) < 0) offsetAmt = (float)cell.getWidth() / 8;
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

            //GameObject wall = Instantiate(Resources.Load<GameObject>("Prefabs/BuildingDoorway1"));
            GameObject wall = Instantiate(Resources.Load<GameObject>(cell.tilesetPath + "/Doorway"));
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

            //GameObject wall = Instantiate(Resources.Load<GameObject>("Prefabs/Railing1"));
            if (cell.type == "StairsDown") continue;
            //get cell below because it is guaranteed to be an indoors cell if this one has a railing on it
            GameObject wall = Resources.Load<GameObject>(grids[cell.layer - 1].getCell(cell.gridX, cell.gridY).tilesetPath + "/Railing");
            if (wall == null) 
            {
                Debug.LogError("Tried to place a railing that wasn't found in the tileset " + grids[cell.layer - 1].getCell(cell.gridX, cell.gridY).tilesetPath + " at (" + cell.gridX + ", " + cell.gridY + ")");
                continue;
            }
            wall = Instantiate(wall);
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
            GameObject ceil = Instantiate(Resources.Load<GameObject>(cell.tilesetPath + "/Ceiling"));
            ceil.transform.SetParent(cellObject.transform);
            ceil.transform.position = new Vector3(0, 1f, 0);
        }
        //models/entities
        //we have cell type and entity facing position, assign subclasses and open
        if (cell.type != "None" && cell.type != "Empty" && cell.type != "Prop")
        {
            if (cell.type == "Empty") cell.traversible = false;
            initializeEntity(cell, cellObject);
        }

        //handle models for types (set when saving?
        string modelURL = "Prefabs/" + cell.getModelToAssign();
        if(cell.getModelToAssign() != "None")
        {
            GameObject model = Instantiate(Resources.Load<GameObject>(modelURL));
            model.transform.position = floor.transform.position;
            model.transform.SetParent(cellObject.transform);
        }

        //place cell prop and position it
        string propURL = "Prefabs/propScenes/" + cell.propToAssign;
        
        if (cell.propToAssign != null && cell.propToAssign != "")
        {
            try
            {
                GameObject prop = Instantiate(Resources.Load<GameObject>(propURL));
                prop.transform.position = floor.transform.position;
                //move prop according to its orientation
                //using 'bottom' zone because zones are rendered on the bottom before rotating to match zone orientation
                prop.transform.localPosition = GridUtils.DirStringToWorldOffset(cell.propPlacementOrientation, "bottom");
                GridUtils.RotatePropToDirection(cell.propPlacementOrientation, "bottom", prop);
                prop.transform.SetParent(cellObject.transform);
            }
            catch (Exception e)
            {
                Debug.Log("Unable to place cell prop with name " + cell.propToAssign + " exception: " + e);
            }

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
            //overrides for tileset exclusive models (transition everything to tileset eventually?)
            
            GameObject model;
            if(cell.type == "StairsUp")
            {
                model = Instantiate(Resources.Load<GameObject>(cell.tilesetPath + "/Stairs"));
            } else if(cell.type == "ClosedDoor" && cell.tilesetPath == "Prefabs/gridTilesets/Indoor/Office")
            {
                model = Instantiate(Resources.Load<GameObject>(cell.tilesetPath + "/Door"));
            }
            else
            {
                model = Instantiate(GridDicts.typeToModel[cell.type]);
            }

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
                cell.traversible = true;
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
                cell.traversible = false;
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
                EnemyManager.addEnemy(cell.gridX, cell.gridY, cell.layer, cell.entity.dataString, currside);
                cell.entity.interactable = false;
                cell.traversible = true;
                break;
            case "Item":
                cell.traversible = false;
                break;

        }
        cell.entity.layer = cell.layer;
    }

    GameObject getBorderedTile(DungeonCell cell, string type, string type2 = "") //where type is 'water', 'lawn', ect; type2 is used for hybrid tiles
    {
        
        
        string floorURL = "";
        //for grass tiles, we want to place a border wherever the neighboring tile is not a grass tile.

        //figure out which neighbors match
        List<string> neighbors;
        if (type2 != "") //use type2 as neighbor search type if given
        {
            neighbors = grids[cell.layer].getNighborsMatchingFilters(cell, "", type2);
        } else
        {
            neighbors = grids[cell.layer].getNighborsMatchingFilters(cell, "", type);
        }

        //backwards compatability cause lazy
        if (type == "grass1") type = "lawn";

        //grab the corresponding tile from resources
        switch (neighbors.Count)
        {
            case 0:
                floorURL = "Prefabs/floorVariants/" + type + type2 + "Variants/" + type + type2 + "FullBorder";
                break;
            case 1:
                floorURL = "Prefabs/floorVariants/" + type + type2 + "Variants/" + type + type2 + "ThreeBorder";
                break;
            case 2:
                //determine if corner or parallel
                //if first direction is opposite to the other direction then we want a parallel border
                if (neighbors[0] == GridUtils.getOppositeDirection(neighbors[1]))
                {
                    floorURL = "Prefabs/floorVariants/" + type + type2 + "Variants/" + type + type2 + "ParallelBorder";
                }
                else
                {
                    floorURL = "Prefabs/floorVariants/" + type + type2 + "Variants/" + type + type2 + "CornerBorder";
                }
                break;
            case 3:
                floorURL = "Prefabs/floorVariants/" + type + type2 + "Variants/" + type + type2 + "OneBorder";
                break;
            case 4:
                floorURL = "Prefabs/floorVariants/" + type + type2 + "Variants/" + type + type2 + "Default";
                break;

        }
        //test for water only wants no border
        if(type == "water") floorURL = "Prefabs/floorVariants/" + type + "Variants/" + type + "Default";
        //rotate it to match physical appearance
        GameObject tile = Instantiate(Resources.Load<GameObject>(floorURL));
        if (type != "water")tile.transform.Rotate(0, GridUtils.getBorderRotation(neighbors), 0);

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


