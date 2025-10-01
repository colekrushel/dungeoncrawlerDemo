using System;
using UnityEngine;

[Serializable]
public class PlayerInventory
{
    public EquipmentItem[] equipmentItems = new EquipmentItem[0];


    public void addItem(string name, string type)
    {
        if(type.ToLower() == "breacher")
        {
            EquipmentItem[] newarr = new EquipmentItem[equipmentItems.Length+1];
            for(int i = 0; i < equipmentItems.Length; i++)
            {
                newarr[i] = equipmentItems[i];
            }
            newarr[equipmentItems.Length] = Resources.Load<EquipmentItem>("Equipment/" + name + "");
            equipmentItems = newarr;
        }
        return;
    }
}
