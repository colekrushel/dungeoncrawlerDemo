using UnityEngine;

public class BreakablePart : MonoBehaviour, IHittable
{
    [SerializeField] public float HP;
    [SerializeField] public float MaxHP;
    [SerializeField] public BreakableConstruct.breakType breakType;
    [SerializeField] public EquipmentItem.type breakableBy;
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
                Destroy(this.gameObject);
            });
            
        } else
        {
            MovementManager.shakeObject(this.gameObject, .04f, 1f, .2f, this.gameObject.transform.position);
        }
        return 1;

    }

    private void onBreak()
    {
        //tell its parent construct that a part has been broken
        this.gameObject.GetComponentInParent<BreakableConstruct>().partBreak(this);
    }
}
