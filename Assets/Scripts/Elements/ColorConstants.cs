using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ColorConstants
{
    public static readonly Dictionary<ElementType, List<Color>> elementColorDict = new Dictionary<ElementType, List<Color>>();
    private static readonly Dictionary<string, MaterialMap> materialsMap = new Dictionary<string, MaterialMap>();

    private static readonly System.Random random = new System.Random();

    // Immovable Solids
    private static readonly Color STONE = new Color(150/255f, 150/255f, 150/255f);

    private static readonly Color PLAYER_SEGMENT = Color.clear;

    // Movable Solids
    private static readonly Color SAND_1 = new Color(217/255f, 170/255f, 85/255f);
    private static readonly Color SAND_2 = new Color(217/255f, 184/255f, 143/255f);
    private static readonly Color SAND_3 = new Color(217/255f, 189/255f, 173/255f);
    private static readonly Color SAND_4 = new Color(242/255f, 213/255f, 187/255f);

    // Liquids
    private static readonly Color WATER_1 = new Color(3/255f, 88/255f, 140/255f, 0.7f);
    private static readonly Color WATER_2 = new Color(242/255f, 242/255f, 242/255f, 0.7f);

    //private static readonly Color LAVA_1 = new Color(64/255f, 1/255f, 1/255f);
    //private static readonly Color LAVA_2 = new Color(140/255f, 3/255f, 3/255f);
    //private static readonly Color LAVA_3 = new Color(217/255f, 61/255f, 4/255f);
    //private static readonly Color LAVA_4 = new Color(242/255f, 116/255f, 5/255f);
    private static readonly Color LAVA = new Color(242/255f, 195/255f, 53/255f);

    // Other
    private static readonly Color EMPTY_CELL = Color.clear;

    static ColorConstants() {
        foreach(ElementType type in Enum.GetValues(typeof(ElementType))) {
            elementColorDict[type] = new List<Color>();
        }

        // Color dictionary initialization
        elementColorDict[ElementType.STONE].Add(STONE);

        elementColorDict[ElementType.PLAYERSEGMENT].Add(PLAYER_SEGMENT);

        elementColorDict[ElementType.SAND].Add(SAND_1);
        elementColorDict[ElementType.SAND].Add(SAND_2);
        elementColorDict[ElementType.SAND].Add(SAND_3);
        elementColorDict[ElementType.SAND].Add(SAND_4);

        elementColorDict[ElementType.WATER].Add(WATER_1);
        elementColorDict[ElementType.WATER].Add(WATER_2);

        elementColorDict[ElementType.LAVA].Add(LAVA);
        

        elementColorDict[ElementType.EMPTYCELL].Add(EMPTY_CELL);

        // Put maps in dict
        materialsMap.Add("STONE", new MaterialMap(Resources.Load<Texture2D>("StoneTexture")));
    }

    public static Color GetColorForElementType(ElementType elementType) {
        List<Color> colorList = elementColorDict[elementType];
        return elementColorDict[elementType][random.Next(colorList.Count)];
    }
    public static Color GetColorForElementType(ElementType elementType, int x, int y) {
        if(materialsMap.ContainsKey(Enum.GetName(typeof(ElementType), elementType))) {
            Color color = materialsMap[Enum.GetName(typeof(ElementType), elementType)].GetColor(x, y);
            return color;
        }
        else {
            return GetColorForElementType(elementType);
        }
    }
}
