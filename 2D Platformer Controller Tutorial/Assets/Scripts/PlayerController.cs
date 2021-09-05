using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private float movementInputDirection; // states whether the player is pushing either 'a' or 'd'

    private int amountOfJumpsLeft; // available number of jumps the character can do

    private bool isFacingRight = true; // states whether the character is facing right or not
    private bool isWalking; // states whether the character is walking or not
    private bool isGrounded; // states whether the character is on ground or not
    private bool isTouchingWall; // states whether the character is on touching a wall or not
    private bool isWallSliding; // states whether the character is sliding on a wall or not
    private bool canJump; // states whether the character can jump or not

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

    public Transform groundCheck; // object used to check for ground
    public Transform wallCheck; // object used to check for wall

    public LayerMask whatIsGround; // specifies what is considered ground

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>(); // gets the reference to the Rigidbody2D component of the player
        anim = GetComponent<Animator>(); // gets the reference to the Animator component of the player
        amountOfJumpsLeft = amountOfJumps; // sets the jumps available the character can do
    }

    // Update is called once per frame
    void Update()
    {
        CheckInput();
        CheckMovementDirection();
        UpdateAnimations();
        CheckIfCanJump();
        CheckIfWallSliding();
    }

    private void FixedUpdate()
    {
        ApplyMovement();
        CheckSurroundings();
    }

    private void CheckIfWallSliding()
    {
        if (isTouchingWall && !isGrounded && rb.velocity.y < 0) // if the character is touching a wall, not grounded, and moving down
        {
            isWallSliding = true;
        }
        else
        {
            isWallSliding = false;
        }
    }

    private void CheckSurroundings()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, whatIsGround); // detects if what is considered ground is colliding with the circle on the groundCheck's position
        isTouchingWall = Physics2D.Raycast(wallCheck.position, transform.right, wallCheckDistance, whatIsGround); // detects if what is considered ground/wall is colliding with the raycast from the wallCheck's position
    }

    private void CheckIfCanJump()
    {
        if (isGrounded && rb.velocity.y < 0.01f)
        {
            amountOfJumpsLeft = amountOfJumps; // resets the available jumps
        }

        if (amountOfJumpsLeft <= 0)
        {
            canJump = false;
        }
        else
        {
            canJump = true;
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

        if (Input.GetButtonDown("Jump"))
        {
            Jump();
        }
    }

    private void Jump()
    {
        if (canJump)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce); // changes the character's 'y' velocity only
            amountOfJumpsLeft--; // decreases the jumps available
        }
    }

    private void ApplyMovement()
    {
        if (isGrounded) // if the character is on ground
        {
            rb.velocity = new Vector2(movementSpeed * movementInputDirection, rb.velocity.y); // makes the character move horizontally normally
        }
        else if (!isGrounded && !isWallSliding && movementInputDirection != 0) // if the character is on air and the player is pressing a movement button
        {
            Vector2 forceToAdd = new Vector2(movementForceInAir * movementInputDirection, 0);
            rb.AddForce(forceToAdd); // applies the force as the horizontal movement of the character in air

            if (Mathf.Abs(rb.velocity.x) > movementSpeed) // if because of continuously adding force to the 'x' velocity of the character the 'x' velocity becomes greater than the intended horizontal movement speed
            {
                rb.velocity = new Vector2(movementSpeed * movementInputDirection, rb.velocity.y); // makes the character's horizontal speed as if it's moving on ground
            }
        }
        else if (!isGrounded && !isWallSliding && movementInputDirection == 0) // if the player stops pressing the movement buttons when the character is on air
        {
            rb.velocity = new Vector2(rb.velocity.x * airDragMultiplier, rb.velocity.y); // gradually stops the horizontal movement of the character on air
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
        if (!isWallSliding)
        {
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
