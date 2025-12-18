using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//script to attach to enemy gameobject that is called from an animationevent to start a cell attack.



public class StartCellAttack : MonoBehaviour
{
    CellAttack attackObject;
    [SerializeField] Enemy enemy;
    bool startnext = false;

    public void setAttack(CellAttack atk)
    {
        attackObject = atk;
    }
    public void startCellAttack()
    {
        StartCoroutine(doAttack(attackObject));
        
    }

    public IEnumerator doAttack(CellAttack atk)
    {
        if (atk == null) yield break;
        //Debug.Log("starting cell attack");
        //start attack by instantiating an attackobject on each affected cell

        //determine which cell gameobjects will be affected

        List<GameObject> affectedCells = new List<GameObject>();
        int layer = enemy.getLayer();
        DungeonGrid grid = GridUtils.grids[layer];
        //decided based on cellattack type
        switch (atk.type)
        {
            case CellAttack.cellAttackType.Within:
                //get all traversible cells around the enemy
                foreach (DungeonCell item in grid.getCellsAroundPoint(enemy.getPos(), atk.range))
                {
                    affectedCells.Add(item.getCellObject());
                }
                break;
            case CellAttack.cellAttackType.Ring:
                //rings around the enemy at radius
                foreach (DungeonCell item in grid.getCellRingAroundPoint(enemy.getPos(), atk.range))
                {
                    affectedCells.Add(item.getCellObject());
                }
                break;
            case CellAttack.cellAttackType.Random:
                foreach (DungeonCell item in grid.getRandomCellsAroundPoint(enemy.getPos(), atk.range, atk.chance, atk.randMax))
                {
                    affectedCells.Add(item.getCellObject());
                }
                break;
        }
        //start cellattack on the affected cells
        foreach (GameObject cell in affectedCells)
        {
            GameObject attack = Instantiate(atk.attackObject);
            attack.GetComponent<HandleCellAttack>().caller = this;
            attack.transform.position = cell.transform.position;
            attack.transform.rotation = Quaternion.Euler(GridUtils.getZoneRotationEuler(Player.orientation));
            attack.transform.SetParent(cell.transform);
        }
        //start next attack by waiting for the charge time and calling this recursively
        if (attackObject != null && attackObject.nextAttack != null)
        {
            //wait for the attack to finish
            yield return new WaitUntil(cannextattack);
            //Debug.Log("death signal receieved");
            //attackObject = attackObject.nextAttack;
            startnext = false;
            StartCoroutine(doAttack(atk.nextAttack));
        }
    }

    bool cannextattack()
    {
        return startnext;
    }

    public void onattackend()
    {
        startnext = true;
    }

}
