using System.Collections.Generic;
using UnityEngine;

public static class EnemyDicts
{
    public static Dictionary<string, GameObject> enemyToModel = new Dictionary<string, GameObject>();

    public static void init()
    {
        //load enemy prefabs
        enemyToModel.Add("Drone", Resources.Load<GameObject>("Prefabs/Enemies/Bat"));
        enemyToModel.Add("Bat", Resources.Load<GameObject>("Prefabs/Enemies/Bat"));
        enemyToModel.Add("Bee", Resources.Load<GameObject>("Prefabs/Enemies/BeeClean"));
        enemyToModel.Add("BeeClean", Resources.Load<GameObject>("Prefabs/Enemies/BeeClean"));
        enemyToModel.Add("HiveClean", Resources.Load<GameObject>("Prefabs/Enemies/HiveClean"));
    }
}
