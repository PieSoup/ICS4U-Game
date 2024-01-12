using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Player
{
    // Initialize fields
    public Element[,] segments {get; private set;}
    public Vector3 velocity;
    public int matrixX;
    public int matrixY;
    private float xThreshold;
    private float yThreshold;

    public int sizeX {get; private set;}
    public int sizeY {get; private set;}

    public bool hittingWall {get; set;}
    public bool isGrounded {get; private set;}

    public int health = 20;
    public PlayerType playerType;
    private float iTime = 0.15f;
    private float iTimeCounter;
    public int playerIndex;

    // Constructor
    public Player(int x, int y, int sizeX, int sizeY, int PlayerIndex, PlayerType playerType, ElementMatrix matrix) {
        this.sizeX = sizeX;
        this.sizeY = sizeY;
        playerIndex = PlayerIndex;
        segments = CreatePlayer(x, y, PlayerIndex, matrix, sizeX, sizeY);
        hittingWall = false;
        this.playerType = playerType;
        iTimeCounter = iTime;
    }
    // Step method (not the element step method, as this class does not inherit from it)
    public void Step(ElementMatrix matrix) {
        iTimeCounter -= Time.deltaTime; // Decrease the invinsibility time
        // Matrix traversal is similar to the elements, but instead looping through each player segment
        velocity += ElementMatrix.gravity;

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
        float floatFreq = (velLo == 0 || velHi == 0) ? 0 : ((float) velLo / velHi);
        int freqThreshold = 0;
        float freqCount = 0;
        
        bool xIsLarger = Mathf.Abs(velXDelta) > Mathf.Abs(velYDelta);
        int smallCount = 0;
        Vector3 lastValidPosition = new Vector3(matrixX, matrixY, 0f);
        for(int i = 1; i <= velHi; i++) {
            freqCount += floatFreq;
            bool thresholdPassed = Mathf.Floor(freqCount) > freqThreshold;
            if(floatFreq != 0 && thresholdPassed && velLo >= smallCount) {
                freqThreshold = Mathf.FloorToInt(freqCount);
                smallCount += 1;
            }

            int xIncrease, yIncrease;

            if(xIsLarger) {
                xIncrease = i;
                yIncrease = smallCount;
            }
            else {
                xIncrease = smallCount;
                yIncrease = i;
            }

            int xOffset = xIncrease * xModifier;
            int yOffset = yIncrease * yModifier;
            bool unstopped;

            for(int y = 0; y < segments.GetLength(1); y++) {
                for(int x = 0; x < segments.GetLength(0); x++) {
                    PlayerSegment playerSegment = (PlayerSegment) segments[x,y];
                    unstopped = playerSegment.StepAsPlayer(matrix, xOffset, yOffset);
                    if(!unstopped) {
                        MoveToLastValid(matrix, lastValidPosition);
                        if(y == 0 && matrix.Get(segments[x,y].matrixX, segments[x,y].matrixY - 1) is not EmptyCell) {
                            velocity.y = 0f;
                            isGrounded = true;
                            hittingWall = false;
                        }
                        else if(y == segments.GetLength(1) - 1 && matrix.Get(segments[x,y].matrixX, segments[x,y].matrixY + 1) is not EmptyCell) {
                            velocity.y *= -1f;
                        }
                        
                        if(y == 0 && isGrounded && velocity.x != 0f && (matrix.Get(segments[x,y].matrixX + 1, segments[x,y].matrixY) is not EmptyCell ||
                        matrix.Get(segments[x,y].matrixX - 1, segments[x,y].matrixY) is not EmptyCell)) {
                            velocity.y = 30f;
                        }
                        return;
                    }
                    else {
                        isGrounded = false;
                    }
                }
            }
            lastValidPosition.x = matrixX + xOffset;
            lastValidPosition.y = matrixY + yOffset;
        }
        MoveToLastValid(matrix, lastValidPosition);
    }

    // Method to move the player to the last valid position
    private void MoveToLastValid(ElementMatrix matrix, Vector3 lastValidPosition) {
        if(matrixX == (int) lastValidPosition.x && matrixY == (int) lastValidPosition.y) {
            return;
        }
        foreach(Element segment in segments) {
            matrix.SetElementAtIndex(segment.matrixX, segment.matrixY, ElementType.EMPTYCELL.CreateElementByMatrix(0, 0, Vector3.zero));
        }

        int xOffset = (int) lastValidPosition.x - matrixX;
        int yOffset = (int) lastValidPosition.y - matrixY;

        foreach(Element segment in segments) {
            PlayerSegment playerSegment = (PlayerSegment) segment;
            int neighbourX = playerSegment.matrixX + xOffset;
            int neighbourY = playerSegment.matrixY + yOffset;
            if(matrix.isWithinBounds(neighbourX, neighbourY)) {
                Element neighbour = matrix.Get(neighbourX, neighbourY);
                if(neighbour is EmptyCell) {
                    matrix.SetElementAtIndex(neighbourX, neighbourY, playerSegment);
                }
                else if(neighbour is Liquid || neighbour is MovableSolid) {
                    matrix.SetElementAtIndex(neighbourX, neighbourY, playerSegment);
                    TakeDamage(matrix, neighbour);
                }
                else if(neighbour is PlayerSegment) {
                    playerSegment.Move(matrix, neighbourX, neighbourY);
                }
            }
            else {
                matrix.SetElementAtIndex(playerSegment.matrixX, playerSegment.matrixY, ElementType.EMPTYCELL.CreateElementByMatrix(playerSegment.matrixX, playerSegment.matrixY, Vector3.zero));
            }
        }
        matrixX = (int) lastValidPosition.x;
        matrixY = (int) lastValidPosition.y;
    }
    // Generate the player using a set of player segments, and add them to the world
    private Element[,] CreatePlayer(int worldX, int worldY, int PlayerIndex, ElementMatrix matrix, int sizeX, int sizeY) {
        Element[,] elements = new Element[sizeX,sizeY];
        
        for(int y = 0; y < sizeY; y++) {
            for(int x = 0; x < sizeX; x++) {
                Element segment = ElementType.PLAYERSEGMENT.CreateElementByMatrix(worldX + x, worldY + y, Vector3.zero);
                matrix.SetElementAtIndex(worldX + x, worldY + y, segment);
                ((PlayerSegment) segment).player = this;
                segment.elementColor = Color.clear;
                elements[x,y] = segment;
            }
        }
        return elements;
    }
    // Method that causes the player to take damage depending on the invinsibility time and the player / element types
    public void TakeDamage(ElementMatrix matrix, Element element) {
        if(iTimeCounter <= 0f) {
            switch(playerType) {
                case PlayerType.FIRE:
                    if(element.elementType == ElementType.WATER) {
                        health -= 1;
                        element.DieAndReplace(matrix, ElementType.EMPTYCELL);
                    }
                    break;
                
                case PlayerType.WATER:
                    if(element.elementType == ElementType.LAVA) {
                        health -= 1;
                        element.DieAndReplace(matrix, ElementType.EMPTYCELL);
                    }
                    break;
            }
            iTimeCounter = iTime;
        }
    }
}
// Player class enum
public enum PlayerType {
    FIRE,
    WATER,
    EARTH,
    AIR
}
