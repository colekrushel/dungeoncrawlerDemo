using UnityEngine;

//data class for info about an enemy's part. oarts are components of their associated scene models, assigned in the enemy prefab.
public class EnemyPart : MonoBehaviour
{
    [SerializeField] public float currentHP;
    [SerializeField] public float maxHP;
    [SerializeField] public string partName;
    [SerializeField] public GameObject partModel;
    public bool isBroken = false;
}
