using UnityEngine;

//data class used to hold info to initialize enemies on load/zone entry
public class EnemySpawn
{
    public int x;
    public int y;
    public string type;
    public int layer;
    public string zone;

    public EnemySpawn(int ix, int iy, string itype, int ilayer, string izone)
    {
        x = ix; 
        y = iy;
        type = itype; 
        layer = ilayer;
        zone = izone;
    }
}
