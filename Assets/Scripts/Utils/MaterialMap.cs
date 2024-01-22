using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaterialMap
{
    Texture2D img;
    public int w;
    public int h;

    public MaterialMap(Texture2D img) {
        this.img = img;
        w = img.width;
        h = img.height;
    }

    public Color GetColor(int x, int y) {
        return img.GetPixel(Mathf.Abs(x) % w, Mathf.Abs(y) % h);
    }
}
