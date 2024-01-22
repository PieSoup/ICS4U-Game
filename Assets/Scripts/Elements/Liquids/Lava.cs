using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

public class Lava : Liquid
{
    // Constructor to set base values
    public Lava(int x, int y, Vector3 velocity) : base(x, y) {
        
        dispersionRate = 0;
        frictionFactor = 1f;
        this.velocity = velocity;
    }
}