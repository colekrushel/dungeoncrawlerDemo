using Mono.Cecil;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "EquipmentItem", menuName = "Scriptable Objects/EquipmentItem")]
public class EquipmentItem : ScriptableObject
{
    [SerializeField] public enum type { Strike, Slash, Pierce, Shield}
    [SerializeField] public type equipType; //type of equipment
    [SerializeField] public float baseDamage; // base damage of weapon;
    [SerializeField] public float shieldHealth; // base health of shield type equipments
    [SerializeField] public float shieldDecay; // rate of shield health decay while held (hp per second)
    [SerializeField] public float shieldRegen; // rate of shield health recovery while not held (hp per second)
    [SerializeField] public float range; //length of slash; radius of strike/pierce
    [SerializeField] public float recoil; //non-actionable time between attacks
    [SerializeField] public float cooldown; //actionable time between attacks
    [SerializeField] public Sprite icon;
    [SerializeField] public Sprite logo;
    [SerializeField] public Texture effect;
}
