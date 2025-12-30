using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


public class HandleEquipment : MonoBehaviour
{
    public static GameObject EquipmentBox;
    public static List<GameObject> EquipmentPanels;
    public static MonoBehaviour Instance { get; private set; }
    private static bool upgraded = false; //whether or not the app has been upgraded to dual-wielding capacity yet.
    public void Awake()
    {
        Instance = this;
        EquipmentBox = transform.Find("Items").gameObject;
    }

    public static void onUpgradeObtain()
    {
        upgraded = true;
        //just load the autoequipped right weapon because i dont want to handle null equipped weapons
        updateEquipped(false);

        //change icon and text to imply equippability
        //GameObject parent = Instance.transform.parent.transform.Find("RightEquip").gameObject;
        //parent.transform.Find("Box").Find("Icon").gameObject.GetComponent<Image>().sprite = GridDicts.typeToSprite["None"];
        //parent.transform.Find("Box").Find("Logo").gameObject.GetComponent<Image>().enabled = false;
        //parent.transform.Find("Stats").Find("Text").gameObject.GetComponent<TextMeshProUGUI>().text = "AWAITING SELECTION";
    }

    public static bool getUpgradeStatus()
    {
        return upgraded;
    }

    public static void displayEquips()
    {
        //fill the grid with equipment from the player's inventory
        if(EquipmentBox == null) EquipmentBox = Instance.transform.Find("Items").gameObject;
        //add a box for every equipment item
        //kill the boxes
        for (int i = EquipmentBox.transform.childCount - 1; i >= 0; i--)
        {
            Destroy(EquipmentBox.transform.GetChild(i).gameObject);
        }
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
        if (upgraded)
        {
            //only handle right if upgraded
            updateEquipped(false);
        }
        
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
            returnstr += "BASEDMG: " + item.baseDamage + "\n" + "RNG: " + item.range + "\n" + "ATKDLY: " + item.cooldown + "\n" + "TYPE: " + item.equipType.ToString().ToUpper();
        }
        return returnstr;
    }


    public static void selectEquipment(BaseEventData eventData, GameObject box, EquipmentItem item)
    {
        AudioManager.playUISelect();
        //interpet event data and check if left or right
        bool left = true;
        PointerEventData e = eventData as PointerEventData;
        if (e.button == PointerEventData.InputButton.Right) left = false;
        if (!left && !upgraded) return; //if unupgraded and right click then cancel behavior
        //triggered when clicking on an item in the grid;
        Player.equipItem(item, left);

        //fill the appropriate detailbox with the selected equipment's icon and stats
        updateEquipped(left);

        //go through grid and remove the marker from the previous selected icon if necessary

        //put the associated marker on the icon in the grid to signify it as selected


    }

}
