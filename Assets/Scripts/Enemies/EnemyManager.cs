using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    public static MonoBehaviour Instance { get; private set; }
    private static List<Enemy> enemies;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Instance = this;
        enemies = new List<Enemy>();
        EnemyDicts.init();
    }

    public static void initializeEnemies() //called after grid is rendered
    {
        spawnEnemy(3, 6, 0, "Drone");
    }

    public static void spawnEnemy(int x, int y, int layer, string type)
    {
        //insantiate object; object already has enemy class on it so we just need to instantiate and move it
        //create a parent object because the animations have local coordinates pre-set
        GameObject enemyParent = new GameObject();
        enemyParent.transform.SetParent(Instance.transform, false);
        GameObject enemy = Instantiate(EnemyDicts.enemyToModel[type]);
        enemy.transform.SetParent(enemyParent.transform, false);
        //fetch enemy instance and add to list
        Enemy e = enemy.GetComponent<Enemy>();
        e.positionObject = enemyParent;
        e.snap(new Vector2(x, y), layer);
        enemies.Add(e);
    }
}
