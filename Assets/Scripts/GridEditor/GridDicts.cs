using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class GridDicts
{



    //static string name = "WA";
    public static Dictionary<Color, string> colorToFloor = new Dictionary<Color, string>()
    {
        { Color.white, "bricks1" },
        { new Color32(0, 123, 0, 255), "grass1" },
        { new Color32(255, 0, 0, 255), "red" },
        { new Color32(0, 0, 0, 255), "black" },

    };
    public static Dictionary<Sprite, string> spriteToType = new Dictionary<Sprite, string>();

    public static Dictionary<string, Color> floorToColor = colorToFloor.ToDictionary(x => x.Value, x => x.Key);
    public static Dictionary<string, Sprite> typeToSprite = new Dictionary<string, Sprite>();

    public static Dictionary<string, GameObject> typeToModel = new Dictionary<string, GameObject>();

    public static void init()
    {
        //add spritesheet icons to dict
        Sprite[] spritesheet = Resources.LoadAll<Sprite>("Tiles/iconsWIPTransparentNoBorder");

        foreach (var s in spritesheet)
        {
            if (s.name == "iconsWIPTransparentNoBorder_0") spriteToType.Add(s, "Rest");
            if (s.name == "iconsWIPTransparentNoBorder_1") spriteToType.Add(s, "Item");
            if (s.name == "iconsWIPTransparentNoBorder_2") spriteToType.Add(s, "OpenDoor");
            if (s.name == "iconsWIPTransparentNoBorder_3") spriteToType.Add(s, "ClosedDoor");
            if (s.name == "iconsWIPTransparentNoBorder_4") spriteToType.Add(s, "OneWay");
            if (s.name == "iconsWIPTransparentNoBorder_5") spriteToType.Add(s, "Entrance");
            if (s.name == "iconsWIPTransparentNoBorder_7") spriteToType.Add(s, "None");

        }
        spriteToType.Add(Resources.Load<Sprite>("Tiles/restricted"), "Empty");
        typeToSprite = spriteToType.ToDictionary(x => x.Value, x => x.Key);

        //entity models
        typeToModel.Add("OpenDoor", Resources.Load<GameObject>("Prefabs/Door1"));
        typeToModel.Add("ClosedDoor", Resources.Load<GameObject>("Prefabs/Door1"));
        typeToModel.Add("Entrance", Resources.Load<GameObject>("Prefabs/Door1"));
        typeToModel.Add("Item", Resources.Load<GameObject>("Prefabs/Item1"));
        typeToModel.Add("Rest", Resources.Load<GameObject>("Prefabs/Rest1"));
    }

}
