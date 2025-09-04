using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class GridData
{
    //class to export grid data into from the editor; should resemble json files used to load grid in the renderer
    public List<Cell> cells;
    public int width;
    public int height;

    public GridData(int w, int h)
    {
        width = w; height = h;
        cells = new List<Cell>();
    }

    public void addCell()
    {

    }

}
