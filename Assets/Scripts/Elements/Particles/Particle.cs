using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
// This class is currently unused in the code, please ignore
public class Particle : Element
{
    
    public ElementType particleElementType;

    public Particle(int x, int y, Vector3 velocity, ElementType elementType, Color color) : base(x, y) {
        if(elementType == ElementType.PARTICLE) {
            throw new InvalidOperationException("Containing element cannot be particle");
        }

        particleElementType = elementType;
        this.velocity = new Vector3();
        Vector3 localVelocity = velocity == null ? new Vector3(0f, -124f, 0f) : velocity;
        this.velocity.x = localVelocity.x;
        this.velocity.y = localVelocity.y;
        elementColor = color;

    }

    public Particle(ElementMatrix matrix, int x, int y, Vector3 velocity, Element sourceElement) : base(x, y) {
        if(sourceElement.elementType == ElementType.PARTICLE) { 
            throw new InvalidOperationException();
        }

        particleElementType = sourceElement.elementType;
        Vector3 localVelocity = velocity == null ? new Vector3(0f, -124f, 0f) : velocity;
        this.velocity.x = localVelocity.x;
        this.velocity.y = localVelocity.y;
        elementColor = sourceElement.elementColor;
    }

    public override void DieAndReplace(ElementMatrix matrix, ElementType type) {
        ParticleDieAndSpawn(matrix);
    }

    private void ParticleDieAndSpawn(ElementMatrix matrix) {
        Element currentElementLocation = matrix.Get(matrixX, matrixY);
        if(currentElementLocation == this || currentElementLocation is EmptyCell) {
            Die(matrix);
            Element newElement = particleElementType.CreateElementByMatrix(matrixX, matrixY, Vector3.zero);
            newElement.elementColor = elementColor;

            matrix.SetElementAtIndex(matrixX, matrixY, newElement);
        }
        else {
            int yIndex = 0;
            while(true) {
                Element elementAtNewPos = matrix.Get(matrixX, matrixY + yIndex);
                if(elementAtNewPos == null) {
                    break;
                }
                else if(elementAtNewPos is EmptyCell) {
                    Die(matrix);
                    matrix.SetElementAtIndex(matrixX, matrixY + yIndex, particleElementType.CreateElementByMatrix(matrixX, matrixY + yIndex, Vector3.zero));
                    break;
                }
            }
        }
    }

    public override void Step(ElementMatrix matrix)
    {
        Debug.Log("Stepping");
        if(stepped.Get(0) == ElementMatrix.stepped.Get(0)) return;
        stepped.Set(0, !stepped.Get(0));
        
        if(velocity.y > -64f && velocity.y < 32f) {
            velocity.y = -64f;
        }
        velocity += ElementMatrix.gravity;
        if(velocity.y < -500f) {
            velocity.y = -500f;
        }
        else if(velocity.y > 500f) {
            velocity.y = 500f;
        }

        int xModifier = velocity.x < 0 ? -1 : 1;
        int yModifier = velocity.y < 0 ? -1 : 1;

        int velXDelta = (int) (Mathf.Abs(velocity.x) * 1f/60f);
        int velYDelta = (int) (Mathf.Abs(velocity.x) * 1f/60f);

        int velHi = Mathf.Max(Mathf.Abs(velXDelta), Mathf.Abs(velYDelta));
        int velLo = Mathf.Min(Mathf.Abs(velXDelta), Mathf.Abs(velYDelta));
        
        float slope = (velHi == 0 || velLo == 0) ? 0 : ((float) (velLo + 1) / (float) (velHi + 1));
        bool xIsLarger = Mathf.Abs(velXDelta) > Mathf.Abs(velYDelta);
        int smallCount;
        Vector3 lastValidPosition = new Vector3(matrixX, matrixY, 0f);

        for(int i = 1; i <= velHi; i++) {

            smallCount = Mathf.FloorToInt(i * slope);

            int xIncrease, yIncrease;

            if(xIsLarger) {
                xIncrease = i;
                yIncrease = smallCount;
            }
            else {
                xIncrease = smallCount;
                yIncrease = i;
            }

            int modifiedX = matrixX + xIncrease * xModifier;
            int modifiedY = matrixY + yIncrease * yModifier;

            if(matrix.isWithinBounds(modifiedX, modifiedY)) {
                Element neighbour = matrix.Get(modifiedX, modifiedY);
                if(neighbour == this) continue;
                bool shouldStop = ActOnElementNeighbour(neighbour, modifiedX, modifiedY, matrix, i == velHi, i == 1, lastValidPosition, 0);
                if(shouldStop) {
                    break;
                } 
                
                lastValidPosition.x = modifiedX;
                lastValidPosition.y = modifiedY;
            }
            else {
                matrix.SetElementAtIndex(matrixX, matrixY, ElementType.EMPTYCELL.CreateElementByMatrix(matrixX, matrixY, Vector3.zero));
                return;
            }
        }
    }

    protected override bool ActOnElementNeighbour(Element neighbour, int modifiedX, int modifiedY, ElementMatrix matrix, bool isFinal, bool isFirst, Vector3 lastValidLocation, int depth)
    {
        if(neighbour is EmptyCell || neighbour is Particle) {
            if(isFinal) {
                SwapPositions(matrix, neighbour, modifiedX, modifiedY);
            }
            else {
                return false;
            }
        }
        else if(neighbour is Liquid || neighbour is Solid) {
            Debug.Log("hit wall or water");
            MoveToLastValid(matrix, lastValidLocation);
            DieAndReplace(matrix, particleElementType);
            return true;
        }
        return false;
    }
}
