using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private float movementInputDirection; // states whether the player is pushing either 'a' or 'd'
    private float jumpTimer; // used to temporarily freeze the character on a state where the player can do inputs that will affect the character's jumping mechanism
    private float turnTimer; // used to temporarily freeze the character on a state where the player can do inputs that will affect the character's turning mechanism
    private float wallJumpTimer; // used to temporarily freeze the character on a state where the player can do inputs that will affect the character's wall jumping mechanism

    private int amountOfJumpsLeft; // available number of jumps the character can do
    private int facingDirection = 1; // stores the current facing direction of the character, -1 is left and 1 is right
    private int lastWallJumpDirection; // character's facing direction when it wall jumped

    private bool isFacingRight = true; // states whether the character is facing right or not
    private bool isWalking; // states whether the character is walking or not
    private bool isGrounded; // states whether the character is on ground or not
    private bool isTouchingWall; // states whether the character is on touching a wall or not
    private bool isWallSliding; // states whether the character is sliding on a wall or not
    private bool canNormalJump; // states whether the character can do normal jump
    private bool canWallJump; // states whether the character can do wall jump
    private bool isAttemptingToJump; // states whether the character is attempting to jump
    private bool checkJumpMultiplier; // states whether the character can stop moving upward while jumping when the player stops pressing the jump button
    private bool canMove; // states whether the character can move horizontally
    private bool canFlip; // states whether the character can flip its direction
    private bool hasWallJumped; // states whether the character has already wall jumped or not
    private bool isTouchingLedge;
    private bool canClimbLedge = false;
    private bool ledgeDetected;

    private Vector2 ledgePosBot;
    private Vector2 ledgePos1;
    private Vector2 ledgePos2;

    private Rigidbody2D rb; // reference to the Rigidbody2D component of the player
    private Animator anim; // reference to the Animator component of the player

    public int amountOfJumps = 1; // max jumps the character can do

    public float movementSpeed = 10.0f; // the default horizontal movement speed of the character
    public float jumpForce = 16.0f; // the default vertical movement speed of the character when jumping
    public float groundCheckRadius; // radius used to detect ground
    public float wallCheckDistance; // the distance used to detect walls
    public float wallSlideSpeed; // states the default speed when the character is sliding on a wall
    public float movementForceInAir; // states the force to add to character's velocity when moving in air
    public float airDragMultiplier = 0.95f; // acts like a friction in air that gradually stops the character from moving in air when the player stops pressing the movement button
    public float variableJumpHeightMultiplier = 0.5f; // used to make the character's upward velocity slower when the player stops pressing the jump button
    public float wallHopForce = 10.0f; // used to calculate the force to apply when the character hops down from sliding on a wall
    public float wallJumpForce = 20.0f; // used to calculate the force to apply when the character jumps from sliding on a wall
    public float jumpTimerSet = 0.15f; // used to set the jump timer
    public float turnTimerSet = 0.1f; // used to set the turn timer
    public float wallJumpTimerSet = 0.5f; // used to set the wall jump timer

    public float ledgeClimbXOffset1 = 0f;
    public float ledgeClimbYOffset1 = 0f;
    public float ledgeClimbXOffset2 = 0f;
    public float ledgeClimbYOffset2 = 0f;

    public Vector2 wallHopDirection; // contains the directions used to calculate the force to apply when the character hops down from sliding on a wall
    public Vector2 wallJumpDirection; // contains the directions used to calculate the force to apply when the character jumps from sliding on a wall

    public Transform groundCheck; // will be used as the reference to the object used to check for ground
    public Transform wallCheck; // will be used as the reference to the object used to check for wall
    public Transform ledgeCheck; // will be used as the reference to the ledge detecting object

    public LayerMask whatIsGround; // specifies what is considered ground

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>(); // gets the reference to the Rigidbody2D component of the player
        anim = GetComponent<Animator>(); // gets the reference to the Animator component of the player
        amountOfJumpsLeft = amountOfJumps; // sets the jumps available the character can do
        wallHopDirection.Normalize();
        wallJumpDirection.Normalize();
    }

    // Update is called once per frame
    void Update()
    {
        CheckInput();
        CheckMovementDirection();
        UpdateAnimations();
        CheckIfCanJump();
        CheckIfWallSliding();
        CheckJump();
        CheckLedgeClimb();
    }

    private void FixedUpdate()
    {
        ApplyMovement();
        CheckSurroundings();
    }

    private void CheckIfWallSliding()
    {
        if (isTouchingWall && movementInputDirection == facingDirection && rb.velocity.y < 0 && !canClimbLedge) // if the character is touching a wall, the player is currently pressing a movement button that is the same with the current facing of the character,
        // ,the character is falling down, and the character is not in the state where it can climb a ledge
        {
            isWallSliding = true; // sets the character's state to be wall sliding
        }
        else
        {
            isWallSliding = false; // sets the character to be no longer wall sliding
        }
    }

    private void CheckLedgeClimb()
    {
        if (ledgeDetected && !canClimbLedge)
        {
            canClimbLedge = true;

            if (isFacingRight)
            {
                ledgePos1 = new Vector2(Mathf.Floor(ledgePosBot.x + wallCheckDistance) - ledgeClimbXOffset1, Mathf.Floor(ledgePosBot.y) + ledgeClimbYOffset1);
                ledgePos2 = new Vector2(Mathf.Floor(ledgePosBot.x + wallCheckDistance) + ledgeClimbXOffset2, Mathf.Floor(ledgePosBot.y) + ledgeClimbYOffset2);
            }
            else
            {
                ledgePos1 = new Vector2(Mathf.Ceil(ledgePosBot.x - wallCheckDistance) + ledgeClimbXOffset1, Mathf.Floor(ledgePosBot.y) + ledgeClimbYOffset1);
                ledgePos2 = new Vector2(Mathf.Ceil(ledgePosBot.x - wallCheckDistance) - ledgeClimbXOffset2, Mathf.Floor(ledgePosBot.y) + ledgeClimbYOffset2);
            }

            canMove = false;
            canFlip = false;

            anim.SetBool("canClimbLedge", canClimbLedge);
        }

        if (canClimbLedge)
        {
            transform.position = ledgePos1;
        }
    }

    public void FinishLedgeClimb()
    {
        canClimbLedge = false;
        transform.position = ledgePos2;
        canMove = true;
        canFlip = true;
        ledgeDetected = false;
        anim.SetBool("canClimbLedge", canClimbLedge);
    }

    private void CheckSurroundings()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, whatIsGround); // detects if what is considered ground is colliding with the circle on the groundCheck's position
        isTouchingWall = Physics2D.Raycast(wallCheck.position, transform.right, wallCheckDistance, whatIsGround); // detects if what is considered ground/wall is colliding with the raycast from the wallCheck's position
        isTouchingLedge = Physics2D.Raycast(ledgeCheck.position, transform.right, wallCheckDistance, whatIsGround);

        if (isTouchingWall && !isTouchingLedge && !ledgeDetected) // this means that the topmost part of the ledge is in between 
        {
            ledgeDetected = true;
            ledgePosBot = wallCheck.position;
        }
    }

    private void CheckIfCanJump()
    {
        if (isGrounded && rb.velocity.y < 0.01f) // if the character is already on ground
        {
            amountOfJumpsLeft = amountOfJumps; // resets the available jumps
        }

        if (isTouchingWall) // if the character is touching a wall
        {
            canWallJump = true; // sets the character to be able to wall jump
        }

        if (amountOfJumpsLeft <= 0) // if the amount of jumps available exhausted
        {
            canNormalJump = false; // sets the character unable to normal jump
        }
        else
        {
            canNormalJump = true; // sets the character able to normal jump
        }
    }

    private void CheckMovementDirection()
    {
        if (isFacingRight && movementInputDirection < 0) // if the character is facing right and the player is pressing 'a'
        {
            Flip();
        }
        else if (!isFacingRight && movementInputDirection > 0) // if the character is not facing right and the player is pressing 'd'
        {
            Flip();
        }

        if (rb.velocity.x != 0) // if the character is not moving horizontally
        {
            isWalking = true;
        }
        else
        {
            isWalking = false;
        }
    }

    // Changes the animation of the character depending on its state
    private void UpdateAnimations()
    {
        anim.SetBool("isWalking", isWalking); // sets the player's animator component's boolean parameter "isWalking" to the property "isWalking"
        anim.SetBool("isGrounded", isGrounded); // sets the player's animator component's boolean parameter "isGrounded" to the property "isGrounded"
        anim.SetFloat("yVelocity", rb.velocity.y); // sets the player's animator component's float parameter "yVelocity" to the 'y' velocity of character's rigid body component
        anim.SetBool("isWallSliding", isWallSliding); // sets the player's animator component's boolean parameter "isWallSliding" to the property "isWallSliding"
    }

    private void CheckInput()
    {
        movementInputDirection = Input.GetAxisRaw("Horizontal"); // by default, returns -1 when 'a' is pressed, and 1 when 'd' is pressed

        if (Input.GetButtonDown("Jump")) // if the player pressed the jump button, which is the spacebar by default
        {
            if (isGrounded || (amountOfJumpsLeft > 0 && !isTouchingWall))
            {
                NormalJump();
            }
            else
            {
                jumpTimer = jumpTimerSet;
                isAttemptingToJump = true;
            }
        }

        if (Input.GetButtonDown("Horizontal") && isTouchingWall) // if the player pressed a movement button while the characters is touching a wall 
        {
            if (!isGrounded && movementInputDirection != facingDirection) // if the character is on air and the movement button pressed is opposite to the current facin of the character
            {
                canMove = false; // makes the character unable to move
                canFlip = false; // makes the character unable to flip
                turnTimer = turnTimerSet; // start turn timer
            }
        }

        if (turnTimer >= 0)
        {
            turnTimer -= Time.deltaTime; // decrease the timer for turning

            if (turnTimer <= 0) // if the timer already finishes
            {
                canMove = true; // makes the character able to move
                canFlip = true; // makes the character able to flip
            }
        }

        if (checkJumpMultiplier && !Input.GetButton("Jump")) // if the jump multiplier is true and the player is not pushing the jump button, which is the spacebar
        {
            checkJumpMultiplier = false;
            rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * variableJumpHeightMultiplier); // makes the upward velocity of the character slower
        }
    }

    private void CheckJump()
    {
        if (jumpTimer > 0) // if jump timer is already running
        {
            if (!isGrounded && isTouchingWall && movementInputDirection != 0 && movementInputDirection != facingDirection) // if the char is on air, touching a wall, and the player is pressing a movement button opposite to the facing of the character
            {
                WallJump(); // makes the char wall jump
            }
            else if (isGrounded) // if the char is on ground
            {
                NormalJump(); // makes the char do normal jump
            }
        }
        
        if (isAttemptingToJump)
        {
            jumpTimer -= Time.deltaTime; // decrease the jump timer
        }

        if (wallJumpTimer > 0) // if timer of wall jump is running
        {
            if (hasWallJumped && movementInputDirection == -lastWallJumpDirection) // if the char has already wall jumped and the player is pressing a movement button opposite to the char's last facing direction when it wall jumped
            {
                rb.velocity = new Vector2(rb.velocity.x, 0.0f);
                hasWallJumped = false;
            }
            else if (wallJumpTimer <= 0) // if the wall jump timer runs out
            {
                hasWallJumped = false;
            }
            else
            {
                wallJumpTimer -= Time.deltaTime; // decrease the wall jump timer
            }
        }
    }

    private void NormalJump()
    {
        if (canNormalJump)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce); // changes the character's 'y' velocity only
            amountOfJumpsLeft--; // decreases the jumps available
            jumpTimer = 0;
            isAttemptingToJump = false;
            checkJumpMultiplier = true;
        }
    }

    private void WallJump()
    {
        if (canWallJump)
        {
            rb.velocity = new Vector2(rb.velocity.x, 0.0f);
            isWallSliding = false;
            amountOfJumpsLeft = amountOfJumps;
            amountOfJumpsLeft--;
            Vector2 forceToAdd = new Vector2(wallJumpForce * wallJumpDirection.x * movementInputDirection, wallJumpForce * wallJumpDirection.y);
            rb.AddForce(forceToAdd, ForceMode2D.Impulse); // makes the character jump in the opposite direction from wall sliding
            jumpTimer = 0;
            isAttemptingToJump = false;
            checkJumpMultiplier = true;
            turnTimer = 0;
            canMove = true;
            canFlip = true;
            hasWallJumped = true;
            wallJumpTimer = wallJumpTimerSet;
            lastWallJumpDirection = -facingDirection;
        }
    }

    private void ApplyMovement()
    {
        if (!isGrounded && !isWallSliding && movementInputDirection == 0) // if the player stops pressing the movement buttons when the character is on air
        {
            rb.velocity = new Vector2(rb.velocity.x * airDragMultiplier, rb.velocity.y); // gradually stops the horizontal movement of the character on air
        }
        else if (canMove)
        {
            rb.velocity = new Vector2(movementSpeed * movementInputDirection, rb.velocity.y); // makes the character move horizontally normally
        }

        if (isWallSliding)
        {
            if (rb.velocity.y < -wallSlideSpeed)
            {
                rb.velocity = new Vector2(rb.velocity.x, -wallSlideSpeed); // makes the downward velocit of the character slower
            }
        }
    }

    private void Flip()
    {
        if (!isWallSliding && canFlip)
        {
            facingDirection *= -1; // flips the facing of the character
            isFacingRight = !isFacingRight;
            transform.Rotate(0.0f, 180.0f, 0.0f); // rotates the character's sprite on 'y' axis only
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius); // draws a gizmo on groundCheck's position 
        Gizmos.DrawLine(wallCheck.position, new Vector3(wallCheck.position.x + wallCheckDistance, wallCheck.position.y, wallCheck.position.z)); // draws a line from a the character towards its front
    }
}
