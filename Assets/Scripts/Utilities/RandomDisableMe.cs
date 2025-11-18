using UnityEngine;

public class RandomDisableMe : MonoBehaviour
{
    //on instantiating attached object, chance to deactivate it
    //used to make certain props appear only sometimes (for variety/performance)

    [SerializeField] private float chanceToDisable; //number 0-100 that represents the % chance to be disabled

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        float randFloat = Random.value * 100;
        if(randFloat < chanceToDisable)
        {
            this.gameObject.SetActive(false);
        }
    }


}
