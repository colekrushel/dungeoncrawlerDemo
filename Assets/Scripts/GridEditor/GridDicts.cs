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
        Sprite[] spritesheet2 = Resources.LoadAll<Sprite>("Tiles/iconsWIPTransparentNoBorder2");
        Sprite[] spritesheet3 = Resources.LoadAll<Sprite>("Tiles/iconsWIPTransparentNoBorder3");

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

        foreach (var s in spritesheet2)
        {
            if (s.name == "iconsWIPTransparentNoBorder2_9") spriteToType.Add(s, "Stairs");
        }

        foreach (var s in spritesheet3)
        {
            if (s.name == "iconsWIPTransparentNoBorder3_10") spriteToType.Add(s, "StairsUp");
            if (s.name == "iconsWIPTransparentNoBorder3_11") spriteToType.Add(s, "StairsDown");
        }

        spriteToType.Add(Resources.Load<Sprite>("Tiles/restricted"), "Empty");
        spriteToType.Add(Resources.Load<Sprite>("Tiles/playerIcon"), "Player");
        spriteToType.Add(Resources.Load<Sprite>("Tiles/enemyIcon"), "Enemy");
        typeToSprite = spriteToType.ToDictionary(x => x.Value, x => x.Key);

        //entity models
        typeToModel.Add("OpenDoor", Resources.Load<GameObject>("Prefabs/Door1"));
        typeToModel.Add("ClosedDoor", Resources.Load<GameObject>("Prefabs/Door1"));
        typeToModel.Add("Entrance", Resources.Load<GameObject>("Prefabs/Door1"));
        typeToModel.Add("Item", Resources.Load<GameObject>("Prefabs/Item1"));
        typeToModel.Add("Rest", Resources.Load<GameObject>("Prefabs/Rest1"));
        typeToModel.Add("StairsUp", Resources.Load<GameObject>("Prefabs/Stairs1"));
    }

}
