using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

public class Lava : Liquid
{
    // Constructor to set base values
    public Lava(int x, int y, Vector3 velocity) : base(x, y) {

        elementColor = Color.red;
        
        dispersionRate = 1;
        frictionFactor = 0.999f;
        this.velocity = velocity;
    }
}