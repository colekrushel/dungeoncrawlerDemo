using System.Collections;
using UnityEngine;

public class BreakablePart : MonoBehaviour, IHittable
{
    [SerializeField] public float HP;
    [SerializeField] public float MaxHP;
    [SerializeField] public BreakableConstruct.breakType breakType;
    [SerializeField] public EquipmentItem.type breakableBy;
    [SerializeField] public int breakValue; //for fielditems
    //[SerializeField] GameObject 

    public float hitByPlayer(float damage, EquipmentItem.type type)
    {
        //play fx and resolve logic
        //check if equipment type matches breakable by type
        if (breakableBy != EquipmentItem.type.None && type != breakableBy)
        {
            return 0;
        }
        HP -= damage;
        if (HP <= 0)
        {
            //if object is broken
            MovementManager.shakeObject(this.gameObject, .04f, 1f, .2f, this.gameObject.transform.position, () =>
            {
                onBreak();
                
            });
            
        } else
        {
            MovementManager.shakeObject(this.gameObject, .04f, 1f, .2f, this.gameObject.transform.position);
        }
        return 1;

    }

    private void onBreak()
    {
        if (breakType == BreakableConstruct.breakType.None)
        {
            //if none then only destroy object
            return;
        }
        if (breakType == BreakableConstruct.breakType.FieldItem)
        {
            //lone breakable parts that have their own rewards
            Player.addCurrency(breakValue);
            return;
        }
        //if treebark then check if all of the cores in the scene have been broken; if one is not broken, then regenerate this object (coroutine?)
        if (breakType == BreakableConstruct.breakType.TreeBark)
        {
            if (EnemyManager.barkregen)
            {
                StartCoroutine(regenerate());
            }
            else
            {
                //don't destroy it because we want to bring it back later for phase 2
                this.gameObject.SetActive(false);
            }
        }
        else if (breakType == BreakableConstruct.breakType.PowerCore)
        {
            EnemyManager.onCoreBreak();
            //don't destroy it because we want to bring it back later for phase 2
            this.gameObject.SetActive(false);
        }
        else {
            //tell its parent construct that a part has been broken
            this.gameObject.GetComponentInParent<BreakableConstruct>().partBreak(this);
            Destroy(this.gameObject);
        }


    }

    IEnumerator regenerate()
    {
        //set scale from 0.01 to .2 to kinda animate regeneration
        this.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);
        yield return new WaitForSeconds(0.5f);
        this.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
        //partial regen
        yield return new WaitForSeconds(0.5f);
        this.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
        //finish regen

    }
}
