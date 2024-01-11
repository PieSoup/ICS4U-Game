using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

// This script is used for managing the creation of elements
// Two modes: player creation for the actual game, and creator creation for testing purposes
public class CreationController : MonoBehaviour
{
    // Initialize field variables
    private bool touchedLastFrame = false;
    private Vector3 lastTouchPos = new Vector3();

    public Vector2 pos;
    public bool isMiningButton;
    public bool isPlacingButton;
    [SerializeField] private Camera cam;
    [SerializeField] private float shootForce;
    [SerializeField] private Transform spawnPos;

    public ElementType selectedElementType;
    private BRUSHTYPE brushType = BRUSHTYPE.CIRCLE;
    private CREATIONMODE creationMode = CREATIONMODE.PLAYER;
    private int brushSize = 5;
    private readonly int maxBrushSize = 50;
    private readonly int minBrushSize = 3;

    [SerializeField] private ElementMatrix matrix;

    private void Awake() {
        // Get the matrix
        matrix = FindObjectOfType<ElementMatrix>();
        
    }

    private void Update() {
        if(pos != null) {
            // In player mode, use trigonometry to determine the angle depending on either the mouse or joystick position
            if(creationMode == CREATIONMODE.PLAYER) {
                Vector2 rotation;
                if(GetComponentInParent<PlayerInput>().currentControlScheme == "Keyboard+Mouse") {
                    rotation = cam.ScreenToWorldPoint(pos) - transform.position;
                }
                else {
                    rotation = pos;
                }
                float rotZ = MathF.Atan2(rotation.y, rotation.x) * Mathf.Rad2Deg;
                transform.rotation = Quaternion.Euler(0, 0, rotZ-90);
                // Detect if the player is either mining or placing, set the brush size, and spawn the elements accordingly
                if(isMiningButton) {
                    brushSize = 32;
                    SpawnElementByInput(spawnPos.position, new Vector3(rotation.x, rotation.y, 0).normalized * shootForce, ElementType.EMPTYCELL);
                }
                else if(isPlacingButton) {
                    brushSize = 7;
                    SpawnElementByInput(spawnPos.position, new Vector3(rotation.x, rotation.y, 0).normalized * shootForce);
                }
                else {
                    touchedLastFrame = false; // Variable used for traversing between frames to ensure there are no gaps in placements
                }

            }
            else {
                // In creator mode, simply just place the elements whereever the cursor is.
                if(isMiningButton) {
                    selectedElementType = ElementType.EMPTYCELL;
                    SpawnElementByInput(pos, new Vector3(0, 0, 0));
                }
                else {
                    touchedLastFrame = false;
                }
                // The user also has the ability to change the brush size in creator mode
                SetBrushSize(Input.mouseScrollDelta);
            }
        }
    
    }
    // Method for setting the brush size and clamping it between a min and max value
    private void SetBrushSize(Vector2 brushDelta) {
        brushSize += (int) brushDelta.y;
        if(brushSize > maxBrushSize) {
            brushSize = maxBrushSize;
        }
        else if(brushSize < minBrushSize) {
            brushSize = minBrushSize;
        }
    }
    // Method to spawn elements based on the user input
    private void SpawnElementByInput(Vector3 touchPos, Vector3 velocity) {
        // If the user is holding the button down, spawn the elements between two points
        // This ensures that if the player is moving the cursor fast, it will not leave gaps in between frames
        if(touchedLastFrame) {
            matrix.SpawnElementBetweenTwoPoints(lastTouchPos, touchPos, velocity, selectedElementType, brushSize, brushType);
        }
        else {
            // Otherwise, just spawn them regularily
            matrix.SpawnElementByPixelWithBrush(touchPos.x, touchPos.y, velocity, selectedElementType, brushSize, brushType);
        }
        // Set the last touch position and the flag
        lastTouchPos = touchPos;
        touchedLastFrame = true;
    }
    // Same method if there is a specified element type
    private void SpawnElementByInput(Vector3 touchPos, Vector3 velocity, ElementType elementType) {

        if(touchedLastFrame) {
            matrix.SpawnElementBetweenTwoPoints(lastTouchPos, touchPos, velocity, elementType, brushSize, brushType);
        }
        else {
            matrix.SpawnElementByPixelWithBrush(touchPos.x, touchPos.y, velocity, elementType, brushSize, brushType);
        }

        lastTouchPos = touchPos;
        touchedLastFrame = true;
    }
    // Enums for the states of the brush and the creation mode
    public enum BRUSHTYPE {
        CIRCLE,
        SQUARE,
        RECTANGLE
    }

    public enum CREATIONMODE {
        CREATION,
        PLAYER
    }
}
