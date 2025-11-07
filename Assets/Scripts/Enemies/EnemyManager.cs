using NUnit.Framework;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    public static MonoBehaviour Instance { get; private set; }
    //enemeis that are active in the current zone
    private static List<Enemy> activeEnemies;
    //uninitialized enemy data
    private static List<EnemySpawn> enemySpawns = new List<EnemySpawn>();
    //lock to prevent enemies from being modified during switch
    public static bool switchingLock = false;
    void Awake()
    {
        Instance = this;
        activeEnemies = new List<Enemy>();
        EnemyDicts.init();
    }

    public static void zoneSwitch(string zone)
    {
        //reset active enemies to be only the ones in the new zone
        //activeEnemies = new List<Enemy>();
        switchingLock = true;
        foreach(Enemy enemy in activeEnemies)
        {
            despawnEnemy(enemy);
        }
        activeEnemies.Clear();
        foreach (EnemySpawn e in enemySpawns) {
            if (e.zone == zone)
            {
                spawnEnemy(e);
            }
        }
        switchingLock = false;
    }
    public static void spawnEnemy(EnemySpawn enemy)
    {
        //return;
        //insantiate object; object already has enemy class on it so we just need to instantiate and move it
        //create a parent object because the animations have local coordinates pre-set

        GameObject enemyParent = new GameObject();
        enemyParent.transform.SetParent(Instance.transform, false);
        GameObject enemyObj = Instantiate(EnemyDicts.enemyToModel[enemy.type]);
        enemyObj.transform.SetParent(enemyParent.transform, false);
        //fetch enemy instance and add to list
        Enemy e = enemyObj.GetComponent<Enemy>();
        e.positionObject = enemyParent;
        e.initialize(enemy.layer, enemy.zone);
        e.snap(new Vector2(enemy.x, enemy.y), enemy.layer);
        activeEnemies.Add(e);
    }

    public static void addEnemy(int x, int y, int layer, string type, string zone)
    {
        //insantiate object; object already has enemy class on it so we just need to instantiate and move it
        //create a parent object because the animations have local coordinates pre-set
        EnemySpawn newEnemy = new EnemySpawn(x, y, type, layer, zone);
        enemySpawns.Add(newEnemy);
    }

    public static void despawnEnemy(Enemy enemy)
    {
        //activeEnemies.Remove(enemy);
        Destroy(enemy.positionObject);
        //handle movement manager entry?
    }

    public static void killEnemy(Enemy enemy, int dropAmt)
    {
        activeEnemies.Remove(enemy);
        Destroy(enemy.positionObject);
        //handle on death fx and drops
        Player.addCurrency(dropAmt);
    }

    public static bool enemyOnPos(Vector2 pos, int layer)
    {
        foreach (Enemy enemy in activeEnemies)
        {
            if(enemy.getPos() == pos && enemy.getLayer() == layer) return true;
        }
        return false;
    }

    public static void updateMapWithEnemyInfo(int layer)
    {
        //pass each enemy in the list into the map's update single cell method; to avoid looping through the enemy list multiple times in updatemap
        if(activeEnemies.Count == 0) return;
        foreach (Enemy enemy in activeEnemies)
        {
            //if(enemy.getLayer() == layer)UIUtils.updateSingleMapCell((int) enemy.getPos().x, (int) enemy.getPos().y, GridDicts.typeToSprite["Enemy"]);
        }
    }
}
