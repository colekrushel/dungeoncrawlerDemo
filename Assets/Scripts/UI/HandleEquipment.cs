using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


public class HandleEquipment : MonoBehaviour
{
    public static GameObject EquipmentBox;
    public static List<GameObject> EquipmentPanels;
    public static MonoBehaviour Instance { get; private set; }
    public void Awake()
    {
        Instance = this;
        EquipmentBox = transform.Find("Items").gameObject;
    }

    public static void displayEquips()
    {
        //fill the grid with equipment from the player's inventory
        if(EquipmentBox == null) EquipmentBox = Instance.transform.Find("Items").gameObject;
        //add a box for every NEW equipment item
        foreach (EquipmentItem equipment in Player.inventory.equipmentItems)
        {
            //check if item is already present in ui



            GameObject newBox = Instantiate(Resources.Load<GameObject>("Prefabs/UI/itemBox"));
            //newBox.transform.Find("Panel").gameObject.GetComponent<Button>().onClick.AddListener(() => { selectEquipment(newBox, true, equipment); });
            newBox.transform.Find("Icon").gameObject.GetComponent<Image>().sprite = equipment.icon;
            newBox.name = equipment.name;
            newBox.transform.SetParent(EquipmentBox.transform, false);

            //assign pointerdown functionality to box
            EventTrigger trigger = newBox.transform.Find("Icon").gameObject.AddComponent<EventTrigger>();
            var pointerDown = new EventTrigger.Entry();
            pointerDown.eventID = EventTriggerType.PointerDown;
            pointerDown.callback.AddListener((e) => selectEquipment(e, newBox, equipment));
            trigger.triggers.Add(pointerDown);

        }
        updateEquipped(true);
        updateEquipped(false);
    }

    public static void updateEquipped(bool updateLeft)
    {
        //updates the equipped display 
        GameObject parent;
        EquipmentItem equipped;
        if (updateLeft)
        {
            parent = Instance.transform.parent.transform.Find("LeftEquip").gameObject;
            equipped = Player.leftItem;
        } else
        {
            parent = Instance.transform.parent.transform.Find("RightEquip").gameObject;
            equipped = Player.rightItem;
        }

        //update ui
        if (equipped != null) {
            parent.transform.Find("Box").Find("Icon").gameObject.GetComponent<Image>().sprite = equipped.icon;
            parent.transform.Find("Box").Find("Logo").gameObject.GetComponent<Image>().enabled = false;
            parent.transform.Find("Box").Find("Logo").gameObject.GetComponent<Image>().enabled = true;
            parent.transform.Find("Box").Find("Logo").gameObject.GetComponent<Image>().sprite = equipped.logo;
            string stats = getStatString(equipped);

            parent.transform.Find("Stats").Find("Text").gameObject.GetComponent<TextMeshProUGUI>().text = stats;
        } else
        {
            parent.transform.Find("Box").Find("Icon").gameObject.GetComponent<Image>().sprite = Resources.Load<Sprite>("Tiles/restricted");
            parent.transform.Find("Box").Find("Logo").gameObject.GetComponent<Image>().enabled = false;
            parent.transform.Find("Stats").Find("Text").gameObject.GetComponent<TextMeshProUGUI>().text = "";
        }



    }

    private static string getStatString(EquipmentItem item)
    {
        string returnstr = "";
        if (item.equipType == EquipmentItem.type.Shield)
        {
            returnstr += "Health: " + item.shieldHealth + "\n" + "Regen: " + item.shieldRegen + "\n" + "Decay: " + item.shieldDecay + "\n" + "Cooldown: " + item.cooldown;
        } else
        {
            returnstr += "Damage: " + item.baseDamage + "\n" + "Range: " + item.range + "\n" + "Cooldown: " + item.cooldown;
        }
        return returnstr;
    }


    public static void selectEquipment(BaseEventData eventData, GameObject box, EquipmentItem item)
    {
        //interpet event data and check if left or right
        bool left = true;
        PointerEventData e = eventData as PointerEventData;
        if (e.button == PointerEventData.InputButton.Right) left = false;

        //triggered when clicking on an item in the grid;
        if (left) Player.leftItem = item;
        else Player.rightItem = item;

        //assign clicked item to player equipped
        Debug.Log(left);
        Debug.Log("clicked on " + box.name);

        //fill the appropriate detailbox with the selected equipment's icon and stats
        updateEquipped(left);

        //go through grid and remove the marker from the previous selected icon if necessary

        //put the associated marker on the icon in the grid to signify it as selected


    }

}
