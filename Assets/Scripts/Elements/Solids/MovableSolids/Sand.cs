using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sand : MovableSolid
{
    // Constructor to set base values
    public Sand(int x, int y, Vector3 velocity) : base(x, y) {

        elementColor = Color.yellow;
        
        this.velocity = velocity;
        frictionFactor = 0.9f;
        inertialResistance = 0.1f;
    }
}
