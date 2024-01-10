using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// This class is used for the player segments contained in the player
public class PlayerSegment : ImmovableSolid
{
    public Player player {get; set;}
    // Constructor to set base values
    public PlayerSegment(int x, int y) : base(x, y) {
        frictionFactor = 0.5f;
        inertialResistance = 1.1f;
        velocity = new Vector3(0f, 0f, 0f);
    }

    public override void Step(ElementMatrix matrix) {}


    protected override bool ActOnElementNeighbour(Element neighbour, int modifiedX, int modifiedY, ElementMatrix matrix, bool isFinal, bool isFirst, Vector3 lastValidLocation, int depth)
    {
        return false;
    }
    // New step method that returns a bool to the host player depending on the interactions of each segment with the surrounding elements
    public bool StepAsPlayer(ElementMatrix matrix, int xOffset, int yOffset) {
        player.hittingWall = false;
        if(matrix.isWithinBounds(matrixX + xOffset, matrixY + yOffset)) {
            Element neighbour = matrix.Get(matrixX + xOffset, matrixY + yOffset);
            if(neighbour is EmptyCell || neighbour is Liquid || neighbour is Particle) {
                return true;
            }
            else if(neighbour is MovableSolid) {
                if(neighbour.isFreeFalling) {
                    return true;
                }
            }
            else if(neighbour is PlayerSegment) {
                PlayerSegment otherSegment = (PlayerSegment) neighbour;
                if(otherSegment.player == player) {
                    return true;
                }
                return false;
            }
            else if(neighbour is ImmovableSolid) {
                player.hittingWall = true;
                return false;
            }
        }
        return true;
    }
    // Move the player segment to the given location
    public void Move(ElementMatrix matrix, int x, int y) {
        matrix.SetElementAtIndex(matrixX, matrixY, ElementType.EMPTYCELL.CreateElementByMatrix(matrixX, matrixY, Vector3.zero));
        matrix.SetElementAtIndex(x, y, this);
    }
}
