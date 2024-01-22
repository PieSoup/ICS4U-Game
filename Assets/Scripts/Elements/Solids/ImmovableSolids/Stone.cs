using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stone : ImmovableSolid
{
    // Constructor to set base values
    public Stone(int x, int y) : base(x, y) {

    }

    public override void Step(ElementMatrix matrix) {}

    protected override bool ActOnElementNeighbour(Element neighbour, int modifiedX, int modifiedY, ElementMatrix matrix, bool isFinal, bool isFirst, Vector3 lastValidLocation, int depth)
    {
        return true;
    }
}
