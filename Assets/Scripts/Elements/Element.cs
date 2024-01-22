using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

// This is the main class for all of the elements to inherit from, storing the main variables that every element needs
// Also has overridable methods that all elements should use
public abstract class Element
{
    // Initialize field variables
    public int matrixX {get; set;}
    public int matrixY {get; set;}
    public BitArray stepped = new BitArray(1);
    public bool isFreeFalling = true;
    public float inertialResistance;

    public ElementType elementType {get; private set;}
    public Color elementColor {get; set;}

    public Vector3 velocity;
    public float xThreshold = 0;
    public float yThreshold = 0;
    public float frictionFactor;

    public string soundName;
    // Constructor
    public Element(int x, int y) {
        SetCoordinatesByMatrix(x, y);  // Set position
        elementType = GetElementEnumType(); // Set the type
        elementColor = ColorConstants.GetColorForElementType(elementType, x, y);
        soundName = Enum.GetName(typeof(ElementType), elementType);

        stepped.Set(0, ElementMatrix.stepped.Get(0)); // Flag for if the element has stepped this frame already
    }
    // Set the field coordinate variables based on the parameters
    public void SetCoordinatesByMatrix(int x, int y) {
        matrixX = x;
        matrixY = y;
    }
    // Get the enum type based on the class name
    public ElementType GetElementEnumType() {
        return (ElementType)Enum.Parse(typeof(ElementType), GetType().Name.ToUpper());
    }
    // Method to swap positions with a given element and positions
    public void SwapPositions(ElementMatrix matrix, Element toSwap, int toSwapX, int toSwapY) {
        if(matrixX == toSwapX && matrixY == toSwapY) {
            return;
        }
        // Set current and neighbour elements to each other
        matrix.SetElementAtIndex(matrixX, matrixY, toSwap);

        matrix.SetElementAtIndex(toSwapX, toSwapY, this);
    }
    // Method to move the element to a specified location
    public void MoveToLastValid(ElementMatrix matrix, Vector3 moveToLocation) {
        if((int) (moveToLocation.x) == matrixX && (int) (moveToLocation.y) == matrixY) return;
        // Get the element at the location that it is moving to, and swap with it
        Element toSwap = matrix.matrix[(int) moveToLocation.x, (int) moveToLocation.y];
        SwapPositions(matrix, toSwap, (int) moveToLocation.x, (int) moveToLocation.y);
    }
    // Method with similar functionality as the previous, but more checks to ensure it moves to the correct place
    public void MoveToLastValidAndSwap(ElementMatrix matrix, Element toSwap, int toSwapX, int toSwapY, Vector3 moveToPosition) {
        int movePosX = (int) moveToPosition.x;
        int movePosY = (int) moveToPosition.y;

        Element neighbour = matrix.matrix[movePosX, movePosY];

        if(this == neighbour || toSwap == neighbour) {
            SwapPositions(matrix, toSwap, toSwapX, toSwapY);
            return;
        }

        if(this == toSwap) {
            SwapPositions(matrix, neighbour, movePosX, movePosY);
            return;
        }

        matrix.SetElementAtIndex(matrixX, matrixY, neighbour);
        matrix.SetElementAtIndex(toSwapX, toSwapY, this);
        matrix.SetElementAtIndex(movePosX, movePosY, toSwap);
    }
    // Method to delete an element if it "dies"
    protected void Die(ElementMatrix matrix) {
        Die(matrix, ElementType.EMPTYCELL);
    }
    // Same as above method, but if an element type is specified switch it to that instead
    protected void Die(ElementMatrix matrix, ElementType type) {
        Element newElement = type.CreateElementByMatrix(matrixX, matrixY, Vector3.zero);
        matrix.SetElementAtIndex(matrixX, matrixY, newElement);
    }
    // Inheritable method that runs the die method by default
    public virtual void DieAndReplace(ElementMatrix matrix, ElementType type) {
        Die(matrix, type);
    }
    // Abstract method to step the element
    public abstract void Step(ElementMatrix matrix);
    // Abstract element to determine behaviour depending on where it is trying to move
    protected abstract bool ActOnElementNeighbour(Element neighbour, int modifiedX, int modifiedY, ElementMatrix matrix, bool isFinal, bool isFirst, Vector3 lastValidLocation, int depth);
}
