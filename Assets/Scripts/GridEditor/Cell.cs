using System;
using System.Collections.Generic;

[Serializable]
public class Cell
{
    //data type to hold info about individual cells/tiles for saving grid data
    public int gridX = -1;
    public int gridY = -1;
    public string type = "Empty";
    public string modelToAssign = "None";
    public string floorToAssign = "None";
    public string floorType = "Default";
    public bool hasCeiling = false;
    public bool traversible = false;
    public List<string> walls = new List<string>();

}
