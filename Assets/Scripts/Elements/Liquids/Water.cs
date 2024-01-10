using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

public class Water : Liquid
{
    // Constructor to set base values
    public Water(int x, int y, Vector3 velocity) : base(x, y) {

        elementColor = Color.blue;
        
        dispersionRate = 3;
        frictionFactor = 1f;
        this.velocity = velocity;
    }
}
