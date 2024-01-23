using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Solid : Element
{
    // Call super constructor
    public Solid(int x, int y) : base(x, y) {}
}

public abstract class ImmovableSolid : Solid
{
    // Call super constructor and set free falling flag to false as it never moves
    public ImmovableSolid(int x, int y) : base(x, y) {
        isFreeFalling = false;
    }

}

public abstract class MovableSolid : Solid
{
    // Call super constructor
    public MovableSolid(int x, int y) : base(x, y) {}

    // Similar functionality as the liquid step method, except it uses recursion instead of a separate method
    // This is because the liquid required different behaviour for its additional movement
    public override void Step(ElementMatrix matrix) {
        
        if(stepped.Get(0) == ElementMatrix.stepped.Get(0)) return;
        stepped.Set(0, !stepped.Get(0));

        velocity += ElementMatrix.gravity;
        if(isFreeFalling) velocity.x *= 0.9f;

        int yModifier = velocity.y < 0 ? -1 : 1;
        int xModifier = velocity.x < 0 ? -1 : 1;
        float velXFloatDelta = Mathf.Abs(velocity.x) * (1f/60f);
        float velYFloatDelta = Mathf.Abs(velocity.y) * (1f/60f);
        int velXDelta;
        int velYDelta;

        if(velXFloatDelta < 1){
            xThreshold += velXFloatDelta;
            velXDelta = (int) xThreshold;
            if(Mathf.Abs(velXDelta) > 0){
                xThreshold = 0;
            }
        }
        else{
            xThreshold = 0;
            velXDelta = (int) velXFloatDelta;
        }
        if(velYFloatDelta < 1){
            yThreshold += velYFloatDelta;
            velYDelta = (int) yThreshold;
            if(Mathf.Abs(velYDelta) > 0){
                yThreshold = 0;
            }
        }
        else{
            yThreshold = 0;
            velYDelta = (int) velYFloatDelta;
        }

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
    // Similar functionality as the liquid method except tuned to imitate solid behaviour
    protected override bool ActOnElementNeighbour(Element neighbour, int modifiedX, int modifiedY, ElementMatrix matrix, bool isFinal, bool isFirst, Vector3 lastValidLocation, int depth) {

        if(neighbour is EmptyCell || neighbour is Particle) {

            SetAdjacentNeighbourFreeFalling(matrix, depth, lastValidLocation);
            if(isFinal) {
                isFreeFalling = true;

                SwapPositions(matrix, neighbour, modifiedX, modifiedY);
            }
            else {
                return false;
            }
        }
        else if(neighbour is Liquid) {
            if(depth > 0) {
                SetAdjacentNeighbourFreeFalling(matrix, depth, lastValidLocation);
                isFreeFalling = true;
                SwapPositions(matrix, neighbour, modifiedX, modifiedY);
            }
            else {
                isFreeFalling = true;
                MoveToLastValidAndSwap(matrix, neighbour, modifiedX, modifiedY, lastValidLocation);
                return true;
            }
        }
        else if(neighbour is Solid) {
            if(neighbour is PlayerSegment) {
                PlayerSegment playerSegment = (PlayerSegment) neighbour;
                Player player = playerSegment.player;
                player.TakeDamage(matrix, this);
            }
            if(depth > 0) return true;
            if(isFinal) {
                MoveToLastValid(matrix, lastValidLocation);
                return true;
            }
            if(isFreeFalling) {
                float absY = Mathf.Max(Mathf.Abs(velocity.y) / 31, 105);
                if(Mathf.Abs(velocity.x) > 300f) {
                    velocity.x += velocity.x < 0 ? -absY : absY;
                }
                else {
                    velocity.x = velocity.x < 0 ? -absY : absY;
                }
                
            }

            Vector3 normalizedVelocity = velocity.normalized;

            int additionalX = GetAdditional(normalizedVelocity.x);
            int additionalY = GetAdditional(normalizedVelocity.y);

            Element diagonalNeighbour = matrix.Get(matrixX + additionalX, matrixY + additionalY);
            if(isFirst) {
                velocity.y = GetAverageVelOrGravity(velocity.y, neighbour.velocity.y);
            }
            else {
                velocity.y = -124f;
            }

            neighbour.velocity.y = velocity.y;

            velocity.x *= frictionFactor;

            if(diagonalNeighbour != null) {
                bool stoppedDiagonally = ActOnElementNeighbour(diagonalNeighbour, matrixX + additionalX, matrixY + additionalY, matrix, true, false, lastValidLocation, depth + 1);
                if(!stoppedDiagonally) {
                    isFreeFalling = true;
                    return true;
                }
            }

            Element adjacentNeighbour = matrix.Get(matrixX + additionalX, matrixY);

            if(adjacentNeighbour != null && adjacentNeighbour != diagonalNeighbour) {
                bool stoppedAdjacently = ActOnElementNeighbour(adjacentNeighbour, matrixX + additionalX, matrixY, matrix, true, false, lastValidLocation, depth + 1);
                if(stoppedAdjacently){
                    if(Mathf.Abs(velocity.x) < 315f) {
                        velocity.x *= -1f;
                    }
                    else if(adjacentNeighbour is ImmovableSolid) {
                        velocity.x = Mathf.Sign(velocity.x) * -105f;
                    }
                }
                if(!stoppedAdjacently) {
                    isFreeFalling = false;
                    return true;
                }
            }

            isFreeFalling = false;

            MoveToLastValid(matrix, lastValidLocation);
            return true;
        }
        return false;
    }
    // See liquid class
    private int GetAdditional(float val) {
        if(val < -0.1f) {
            return Mathf.FloorToInt(val);
        }
        else if(val > 0.1f) {
            return Mathf.CeilToInt(val);
        }
        else {
            return 0;
        }
    }
    // See liquid class
    private float GetAverageVelOrGravity(float vel, float otherVel) {
        // Check if otherVel is greater than -125f
        if (otherVel > -125f) {
            // If true, return a predefined value -124f
            return -124f;
        }
        // Calculate the average of vel and otherVel
        float avg = (vel + otherVel) / 2;
        // Check if the average is greater than 0
        if (avg > 0) {
            // If true, return the calculated average
            return avg;
        } else {
            // If the average is not greater than 0, return the minimum of the average and -124f
            return Mathf.Min(avg, -124f);
        }
    }
    // See liquid class
    private void SetAdjacentNeighbourFreeFalling(ElementMatrix matrix, int depth, Vector3 lastValidLocation) {
        
        if(depth > 0 ) return;

        Element neighbour1 = matrix.Get((int) lastValidLocation.x + 1, (int) lastValidLocation.y);

        if(neighbour1 is Solid) {
            SetInteria(neighbour1);
        }

        Element neighbour2 = matrix.Get((int) lastValidLocation.x - 1, (int) lastValidLocation.y);

        if(neighbour2 is Solid) {
            SetInteria(neighbour2);
        }
    }
    // See liquid class
    private void SetInteria(Element element) {
        element.isFreeFalling = Random.value > element.inertialResistance;
    }
}