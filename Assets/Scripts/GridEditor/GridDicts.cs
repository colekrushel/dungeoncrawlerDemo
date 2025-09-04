using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class GridDicts
{
    //static string name = "WA";
    public static Dictionary<string, string> colorToFloor = new Dictionary<string, string>()
    {
        { "RGBA(1.000, 1.000, 1.000, 1.000)", "bricks1" },
        { "RGBA(0.041, 0.464, 0.062, 1.000)", "grass1" }

    };
    public static Dictionary<string, string> spriteToType = new Dictionary<string, string>()
    {
        { "None", "None2" },
        { "iconsWIPTransparentNoBorder_0", "Rest" },
        { "iconsWIPTransparentNoBorder_1", "Item" },
        { "iconsWIPTransparentNoBorder_2", "OpenDoor" },
        { "iconsWIPTransparentNoBorder_3", "ClosedDoor" },
        { "iconsWIPTransparentNoBorder_4", "OneWay" },
        { "iconsWIPTransparentNoBorder_5", "Entrance" },
        { "iconsWIPTransparentNoBorder_7", "None" }, //transparent icon
        { "restricted", "Empty" }
    };
    public static Dictionary<string, string> floorToColor = colorToFloor.ToDictionary(x => x.Value, x => x.Key);
    public static Dictionary<string, string> typeToSprite = spriteToType.ToDictionary(x => x.Value, x => x.Key);

}
