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
    public Player player => matrix.GetPlayer(playerNumber);
    [SerializeField] SpriteRenderer sprite;

    [Header("Movement Variables")]
    [SerializeField] private float moveSpeed;
    private Vector2 horizontalMovement;
    private bool facingRight = true;
    private bool canMove = false;

    [Header("Jump Variables")]
    [SerializeField] private float jumpForce;
    [SerializeField] private float jumpBuffer;
    private bool canJump => jumpBufferCounter > 0f;
    private float jumpBufferCounter;
    public List<Sprite> jumpSprites;

    private Vector2 aimCursor;
    [SerializeField] CreationController cursor;
    private PlayerControls playerControls;
    int cycleCount = 1;
    ElementType classElementType;

    [SerializeField] Animator anim;
    [SerializeField] RuntimeAnimatorController[] animControllers;
    string animTypeName;

    private void Awake() {
        // Set initial values and get other object instances
        playerNumber = JoinManager.currentPlayerIndex;

        matrix = FindObjectOfType<ElementMatrix>();

        playerControls = new PlayerControls();

        if(player.playerType == PlayerClass.FIRE) {
            classElementType = ElementType.LAVA;
        }
        else if(player.playerType == PlayerClass.WATER) {
            classElementType = ElementType.WATER;
        }
        else if(player.playerType == PlayerClass.EARTH) {
            classElementType = ElementType.SAND;
        }
        cursor.selectedElementType = classElementType;
        string typeName = player.playerType.ToString();
        animTypeName = $"{typeName[0].ToString().ToUpper()}{typeName.Substring(1).ToLower()}" + "Bender";
        foreach(RuntimeAnimatorController animController in animControllers) {
            if(animController.name == animTypeName) {
                anim.runtimeAnimatorController = animController;
                break;
            }
        }
    }

    private void OnEnable() {
        playerControls.Enable();
    }

    private void OnDisable() {
        playerControls.Disable();
    }

    void Update() {

        canMove = JoinManager.currentPlayerIndex < matrix.maxPlayers - 1 ? false : true;
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
        if(player.isGrounded) {
            jumpBufferCounter = jumpBuffer;
        }
        else {
            jumpBufferCounter -= Time.deltaTime;
        }

        if(player.health <= 0) {
            canMove = false;
            player.health = 0;
            sprite.color = new Color(1f, 1f, 1f, 0f);
        }
        if(player.iTimeCounter > 0) {
            Flicker(player.iTimeCounter / 5);
        }
        anim.SetFloat("Speed", Mathf.Abs(player.velocity.x));
        if(player.isGrounded) {
            anim.SetBool("isGrounded", true);
        }
        else {
            anim.SetBool("isGrounded", false);
        }
        // Set the transform to the actual player's position in the world
        transform.position = new Vector2((float) player.segments[player.sizeX/2 + 1,player.sizeY/2 + 3].matrixX / 16f, (float) player.segments[player.sizeX/2 + 1,player.sizeY/2 + 3].matrixY / 16f);

        
    }

    private void FixedUpdate() {
        if(canMove) {
            Move(); // Move the player 
        }
        if(!player.isGrounded) {
            GetAirSprite();
        }
        else {
            anim.speed = 1f;
        }
    }
    // Run various methods depending on certain event callbacks from the user
    public void OnAim(InputAction.CallbackContext ctx) => aimCursor = ctx.ReadValue<Vector2>();
    public void OnMove(InputAction.CallbackContext ctx) => horizontalMovement = ctx.ReadValue<Vector2>();
    public void OnMine(InputAction.CallbackContext ctx) {
        if(ctx.phase == InputActionPhase.Performed && canMove) {
            cursor.isMiningButton = !cursor.isMiningButton; // Set the mining flag
            if(cursor.isMiningButton) {
                FindObjectOfType<AudioManager>().Play("Mine");
            }
            else {
                FindObjectOfType<AudioManager>().Stop("Mine");
            }
        }
    }

    public void OnJump(InputAction.CallbackContext ctx) => Jump();
    public void OnCycle(InputAction.CallbackContext ctx) {
        // Cycle the current element
        if(cycleCount >= 2) {
            ElementType selectedElement = cursor.selectedElementType;
            FindObjectOfType<AudioManager>().Stop(Enum.GetName(typeof(ElementType), selectedElement));
            if(selectedElement == classElementType) {
                cursor.selectedElementType = ElementType.STONE;
            }
            else {
                cursor.selectedElementType = classElementType;
            }
            
            if(cursor.isPlacingButton) {
                FindObjectOfType<AudioManager>().Play(Enum.GetName(typeof(ElementType), cursor.selectedElementType));
            }
            cycleCount = 0;
        }
        cycleCount += 1;
    }
    public void OnPlace(InputAction.CallbackContext ctx) {
        if(ctx.phase == InputActionPhase.Performed && canMove) {
            cursor.isPlacingButton = !cursor.isPlacingButton; // Set the placing flag
            if(cursor.isPlacingButton) {
                FindObjectOfType<AudioManager>().Play(Enum.GetName(typeof(ElementType), cursor.selectedElementType));
            }
            else {
                FindObjectOfType<AudioManager>().Stop(Enum.GetName(typeof(ElementType), cursor.selectedElementType));
            }
        }
    }

    private void Move() {
        player.velocity.x = horizontalMovement.x * moveSpeed; // Set the player's velocity depending on the user input Vector2
    }

    private void Jump() {
        // When the user wants to jump, if they can, add y velocity
        if(canJump && canMove) {
            player.velocity.y = jumpForce;
        }
        jumpBufferCounter = 0f;
    }

    private void GetAirSprite() {

        int airIndex = (int) Mathf.Clamp(Helpers.Map(player.velocity.y, -jumpForce, jumpForce, 0, jumpSprites.Count),
        0,
        jumpSprites.Count - 1

        );

        anim.speed = 0f;
        anim.Play(animTypeName + "_Jump", 0, (float) airIndex / jumpSprites.Count);

    }

    private IEnumerator Flicker(float delay) {
        sprite.color = new Color(1f, 1f, 1f, 0.5f);
        yield return new WaitForSeconds(delay);
        sprite.color = new Color(1f, 1f, 1f, 1f);
        yield return new WaitForSeconds(delay);
    }
}
