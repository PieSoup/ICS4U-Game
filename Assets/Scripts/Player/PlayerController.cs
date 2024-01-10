using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
// This class manipulates the player using user inputs
public class PlayerController : MonoBehaviour
{
    // Initialize fields
    [Header("Components")]
    private int playerNumber;
    ElementMatrix matrix;
    Player player => matrix.GetPlayer(playerNumber);

    [Header("Movement Variables")]
    [SerializeField] private float moveSpeed;
    private Vector2 horizontalMovement;
    private bool facingRight = true;

    [Header("Jump Variables")]
    [SerializeField] private float jumpForce;
    [SerializeField] private float hangTime = 0.1f;
    [SerializeField] private float jumpBuffer = 0.1f;
    private bool canJump => jumpBufferCounter > 0f && hangTimeCounter > 0f;
    private float hangTimeCounter;
    private float jumpBufferCounter;

    private Vector2 aimCursor;
    [SerializeField] CreationController cursor;
    private PlayerControls playerControls;
    int cycleCount = 0;

    private void Awake() {
        // Set initial values and get other object instances
        playerNumber = JoinManager.currentPlayerIndex;
        cursor.cam = FindObjectsOfType<Camera>()[playerNumber];
        matrix = FindObjectOfType<ElementMatrix>();

        playerControls = new PlayerControls();

        if(player.playerType == PlayerType.FIRE) {
            cursor.selectedElementType = ElementType.LAVA;
        }
        else if(player.playerType == PlayerType.WATER) {
            cursor.selectedElementType = ElementType.WATER;
        }
    }

    private void OnEnable() {
        playerControls.Enable();
    }

    private void OnDisable() {
        playerControls.Disable();
    }

    void Update() {
        // Set the cursor position to the aimCursor input variable
        if(Mathf.Abs(aimCursor.x) + Mathf.Abs(aimCursor.y) > 0.1f) {
            cursor.pos = aimCursor;
            
        }
        // Determine if the player is looking left or right and flip the sprite depending on this
        if((horizontalMovement.x < 0f && facingRight) || (horizontalMovement.x > 0f && !facingRight)){
            facingRight = !facingRight;
            GetComponent<SpriteRenderer>().flipX = !facingRight;

        }
        // Subtract time from the jump buffer
        jumpBufferCounter -= Time.deltaTime;
        // Check if the player is dead
        if(player.health <= 0) {
            Destroy(gameObject);
        } 
        // Set the transform to the actual player's position in the world
        transform.position = new Vector2((float) player.segments[player.sizeX/2 + 1,player.sizeY/2 + 3].matrixX / 16f, (float) player.segments[player.sizeX/2 + 1,player.sizeY/2 + 3].matrixY / 16f);

        
    }

    private void FixedUpdate() {
        Move(); // Move the player 
        // Decrease the hang time variable to check if the player can jump
        if(player.isGrounded) {
            hangTimeCounter = hangTime;
        }
        else {
            hangTimeCounter -= Time.fixedDeltaTime;
        }
    }
    // Run various methods depending on certain event callbacks from the user
    public void OnAim(InputAction.CallbackContext ctx) => aimCursor = ctx.ReadValue<Vector2>();
    public void OnMove(InputAction.CallbackContext ctx) => horizontalMovement = ctx.ReadValue<Vector2>();
    public void OnMine(InputAction.CallbackContext ctx) {
        if(ctx.phase == InputActionPhase.Performed) {
            cursor.isMiningButton = !cursor.isMiningButton; // Set the mining flag
        }
        
    }

    public void OnJump(InputAction.CallbackContext ctx) => Jump();
    public void OnCycle(InputAction.CallbackContext ctx) {
        // Cycle the current element
        if(cycleCount == 2) {
            ElementType selectedElement = cursor.selectedElementType;
            if(player.playerType == PlayerType.FIRE) {
                if(selectedElement == ElementType.LAVA) {
                    cursor.selectedElementType = ElementType.SAND;
                }
                else if(selectedElement == ElementType.SAND) {
                    cursor.selectedElementType = ElementType.STONE;
                }
                else {
                    cursor.selectedElementType = ElementType.LAVA;
                }
            }
            else if(player.playerType == PlayerType.WATER) {
            if(selectedElement == ElementType.WATER) {
                    cursor.selectedElementType = ElementType.SAND;
                }
                else if(selectedElement == ElementType.SAND) {
                    cursor.selectedElementType = ElementType.STONE;
                }
                else {
                    cursor.selectedElementType = ElementType.WATER;
                }
            }
            cycleCount = 0;
        }
        cycleCount += 1;
    }
    public void OnPlace(InputAction.CallbackContext ctx) {
        if(ctx.phase == InputActionPhase.Performed) {
            cursor.isPlacingButton = !cursor.isPlacingButton; // Set the placing flag
        }
    }

    private void Move() {
        player.velocity.x = horizontalMovement.x * moveSpeed; // Set the player's velocity depending on the user input Vector2
    }

    private void Jump() {
        // When the user wants to jump, if they can, add y velocity
        jumpBufferCounter = jumpBuffer;
        if(canJump) {
            player.velocity.y = jumpForce;
        }
    }
}
