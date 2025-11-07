using UnityEngine;

public class BreakablePart : MonoBehaviour, IHittable
{
    [SerializeField] public float HP;
    [SerializeField] public float MaxHP;
    [SerializeField] public BreakableConstruct.breakType breakType;
    //[SerializeField] GameObject 

    public float hitByPlayer(float damage)
    {
        //play fx and resolve logic
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
