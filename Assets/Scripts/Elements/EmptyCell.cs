using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Class for the empty cell element type
public class EmptyCell : Element
{  
    // Create a singleton instance as emptycells do not need individual behaviour since they do nothing
    private static Element element;
    //Constructor
    public EmptyCell(int x, int y) : base(x, y) {
        elementColor = Color.clear; // Set the color
    }

    public static Element GetInstance() {
        if(element == null) {
            element = new EmptyCell(-1, -1);
        }
        return element;
    }
    // Override the abstract methods and simply return out of them
    public override void Step(ElementMatrix matrix)
    {
        return;
    }

    protected override bool ActOnElementNeighbour(Element neighbour, int modifiedX, int modifiedY, ElementMatrix matrix, bool isFinal, bool isFirst, Vector3 lastValidLocation, int depth) {
        return true;
    }
}
