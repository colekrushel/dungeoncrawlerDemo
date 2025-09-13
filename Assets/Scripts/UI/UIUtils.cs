using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements.Experimental;

public static class UIUtils 
{
    //'pops' in the window by toggling it and performing a little animation
    public static void popIn(GameObject window)
    {
        fadeObject(window, true, 0.2f);
        window.SetActive(true);
    }

    public static void popOut(GameObject window)
    {
        window.SetActive(false);
    }

    public static void updateMap(GameObject mapGrid, DungeonGrid grid)
    {
        //get cells around player and update the map with their info and walls
        //default is 2 cell radius around player (map grid is 5x5 with player's pos at center)
        //0,0 is bottom left of rendered map
        int playerX = (int)Player.getPos().x;
        int playerY = (int)Player.getPos().y;
        string[] walls = new string[4];
        walls[0] = "N"; walls[1] = "S"; walls[2] = "E"; walls[3] = "W";
        for (int y = playerY-2; y < playerY + 3; y++)
        {
            for(int x = playerX-2; x < playerX + 3; x++)
            {
                //get associated cell on the map
                int cellX = (x - playerX) + 2; //[0 - 4]
                int cellY = (y - playerY) + 2; //[0 - 4]
                cellY = Mathf.Abs(cellY - 4); //first object in mapgrid is in the topleft corner so inverse y to get the right cell from the grid data ; now the map is populated starting from the bottom left cell
                GameObject mapCell = mapGrid.transform.GetChild(cellY * 5 + cellX).gameObject;

                //check if cell is out of bounds first; draw empty cell if so
                if (grid.cellOutOfBounds(new Vector2Int(x, y)))
                {
                    //empty cell - background transparent, icon nullsprite
                    mapCell.transform.Find("Background").gameObject.GetComponent<Image>().color = new Color(0, 0, 0, 0);
                    mapCell.transform.Find("Icon").gameObject.GetComponent<Image>().sprite = GridDicts.typeToSprite["None"];
                    //clear walls to prevent holdovers
                    foreach (string wall in walls)
                    {
                        mapCell.transform.Find(wall).gameObject.GetComponent<Image>().color = new Color(0, 0, 0, 0); //set to transparent if wall is not part of cell
                    }
                    continue;
                } else
                {
                    DungeonCell realCell = grid.getCell(x, y);
                    //cell on map - assign sprite from typeToSprite dict and background from floorToColor dict
                    mapCell.transform.Find("Icon").gameObject.GetComponent<Image>().sprite = GridDicts.typeToSprite[realCell.type];
                    mapCell.transform.Find("Background").gameObject.GetComponent<Image>().color = GridDicts.floorToColor[realCell.floorToAssign];
                    //assign walls - overlap should be fine visually?
                    foreach (string wall in walls)
                    {
                        if (!realCell.walls.Contains(wall))
                        {
                            mapCell.transform.Find(wall).gameObject.GetComponent<Image>().color = new Color(0, 0, 0, 0); //set to transparent if wall is not part of cell
                        } else
                        {
                            mapCell.transform.Find(wall).gameObject.GetComponent<Image>().color = Color.black;
                        }
                            
                    }
                }              
            }
        }
        //asign player sprite, along with rotation
        GameObject playerCell = mapGrid.transform.GetChild(2 * 5 + 2).gameObject;
        playerCell.transform.Find("Icon").gameObject.GetComponent<Image>().sprite = GridDicts.typeToSprite["Player"];
        //set rotation with switch statement because player's right is icon's left
        switch (Player.facing)
        {
            case "N":
                playerCell.transform.Find("Icon").transform.rotation = Quaternion.Euler(0, 0, Player.playerObject.transform.rotation.eulerAngles.y);
                break;
            case "E":
                playerCell.transform.Find("Icon").transform.rotation = Quaternion.Euler(0, 0, 270);
                break;
            case "S":
                playerCell.transform.Find("Icon").transform.rotation = Quaternion.Euler(0, 0, Player.playerObject.transform.rotation.eulerAngles.y);
                break;
            case "W":
                playerCell.transform.Find("Icon").transform.rotation = Quaternion.Euler(0, 0, 90);
                break;
        }
        
    }

    //from https://stackoverflow.com/questions/44933517/fading-in-out-gameobject
    public static IEnumerator fadeObject(GameObject objectToFade, bool fadeIn, float duration)
    {
        float counter = 0f;

        //Set Values depending on if fadeIn or fadeOut
        float a, b;
        if (fadeIn)
        {
            a = 0;
            b = 1;
        }
        else
        {
            a = 1;
            b = 0;
        }

        int mode = 0;
        Color currentColor = Color.clear;

        SpriteRenderer tempSPRenderer = objectToFade.GetComponent<SpriteRenderer>();
        Image tempImage = objectToFade.GetComponent<Image>();
        RawImage tempRawImage = objectToFade.GetComponent<RawImage>();
        MeshRenderer tempRenderer = objectToFade.GetComponent<MeshRenderer>();
        Text tempText = objectToFade.GetComponent<Text>();
        CanvasGroup tempGroup = objectToFade.GetComponent<CanvasGroup>();

        //Check if this is a Sprite
        if (tempSPRenderer != null)
        {
            currentColor = tempSPRenderer.color;
            mode = 0;
        }
        //Check if Image
        else if (tempImage != null)
        {
            currentColor = tempImage.color;
            mode = 1;
        }
        //Check if RawImage
        else if (tempRawImage != null)
        {
            currentColor = tempRawImage.color;
            mode = 2;
        }
        //Check if Text 
        else if (tempText != null)
        {
            currentColor = tempText.color;
            mode = 3;
        }

        //Check if 3D Object
        else if (tempRenderer != null)
        {
            currentColor = tempRenderer.material.color;
            mode = 4;

            //ENABLE FADE Mode on the material if not done already
            tempRenderer.material.SetFloat("_Mode", 2);
            tempRenderer.material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            tempRenderer.material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            tempRenderer.material.SetInt("_ZWrite", 0);
            tempRenderer.material.DisableKeyword("_ALPHATEST_ON");
            tempRenderer.material.EnableKeyword("_ALPHABLEND_ON");
            tempRenderer.material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            tempRenderer.material.renderQueue = 3000;
        }
        //check if CanvasGroup
        else if(tempGroup != null)
        {
            mode = 5;
        }
        else
        {
            yield break;
        }

        while (counter < duration)
        {
            counter += Time.deltaTime;
            float alpha = Mathf.Lerp(a, b, counter / duration);

            switch (mode)
            {
                case 0:
                    tempSPRenderer.color = new Color(currentColor.r, currentColor.g, currentColor.b, alpha);
                    break;
                case 1:
                    tempImage.color = new Color(currentColor.r, currentColor.g, currentColor.b, alpha);
                    break;
                case 2:
                    tempRawImage.color = new Color(currentColor.r, currentColor.g, currentColor.b, alpha);
                    break;
                case 3:
                    tempText.color = new Color(currentColor.r, currentColor.g, currentColor.b, alpha);
                    break;
                case 4:
                    tempRenderer.material.color = new Color(currentColor.r, currentColor.g, currentColor.b, alpha);
                    break;
                case 5:
                    tempGroup.alpha = alpha;
                    break;
            }
            yield return null;
        }
        //when finished hiding canvas group, disable it
        if (mode == 5 && !fadeIn) objectToFade.GetComponent<Canvas>().enabled = false;
    }

}
