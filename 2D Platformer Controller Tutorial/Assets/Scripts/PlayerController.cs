using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private float movementInputDirection; // states whether the player is pushing either 'a' or 'd'

    private bool isFacingRight = true; // states whether the character is facing right or not
    private bool isWalking; // states whether the character is walking or not

    private Rigidbody2D rb; // reference to the Rigidbody2D component of the player
    private Animator anim; // reference to the Animator component of the player

    public float movementSpeed = 10.0f; // the default horizontal movement speed of the character
    public float jumpForce = 16.0f; // the default vertical movement speed of the character when jumping

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>(); // gets the reference to the Rigidbody2D component of the player
        anim = GetComponent<Animator>(); // gets the reference to the Animator component of the player
    }

    // Update is called once per frame
    void Update()
    {
        CheckInput();
        CheckMovementDirection();
        UpdateAnimations();
    }

    private void FixedUpdate()
    {
        ApplyMovement();
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

    private void UpdateAnimations()
    {
        anim.SetBool("isWalking", isWalking); // sets the player's animator component's parameter "isWalking" to the property "isWalking"
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
        rb.velocity = new Vector2(rb.velocity.x, jumpForce); // changes the character's 'y' velocity only
    }

    private void ApplyMovement()
    {
        rb.velocity = new Vector2(movementSpeed * movementInputDirection, rb.velocity.y); // makes the character move horizontally
    }

    private void Flip()
    {
        isFacingRight = !isFacingRight;
        transform.Rotate(0.0f, 180.0f, 0.0f); // rotates the character's sprite on 'y' axis only
    }
}
