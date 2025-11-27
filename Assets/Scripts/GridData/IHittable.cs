using UnityEngine;

public interface IHittable
{
    float hitByPlayer(float damage, EquipmentItem.type type);
}
