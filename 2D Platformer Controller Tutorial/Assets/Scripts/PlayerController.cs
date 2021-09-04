using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private float movementInputDirection; // determines whether the player is pushing either 'a' or 'd'

    private Rigidbody2D rb; // reference to the Rigidbody2D component of the player

    public float movementSpeed = 10.0f; // determines the default horizontal movement speed of the character

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>(); // gets the reference to the Rigidbody2D component of the player
    }

    // Update is called once per frame
    void Update()
    {
        CheckInput();
    }

    private void FixedUpdate()
    {
        ApplyMovement();
    }

    private void CheckInput()
    {
        movementInputDirection = Input.GetAxisRaw("Horizontal"); // by default, returns -1 when 'a' is pressed, and 1 when 'd' is pressed
    }

    private void ApplyMovement()
    {
        rb.velocity = new Vector2(movementSpeed * movementInputDirection, rb.velocity.y); // makes the character move horizontally
    }
}
