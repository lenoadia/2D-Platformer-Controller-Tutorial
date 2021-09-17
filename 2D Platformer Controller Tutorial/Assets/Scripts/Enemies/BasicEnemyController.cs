using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicEnemyController : MonoBehaviour
{
    private enum State
    {
        Walking,
        Knockback,
        Dead
    }

    private State currentState;

    [SerializeField]
    private float
        groundCheckDistance,
        wallCheckDistance,
        movementSpeed;

    [SerializeField]
    private Transform
        groundCheck,
        wallCheck;
    
    [SerializeField]
    private LayerMask whatIsGround;

    private int facingDirection;

    private Vector2 movement;

    private bool
        groundDetected,
        wallDetected;

    private GameObject alive;
    private Rigidbody2D aliveRb;

    private void Start()
    {
        alive = transform.Find("Alive").gameObject;
        aliveRb = alive.GetComponent<Rigidbody2D>();
        facingDirection = 1;
    }

    private void Update()
    {
        switch (currentState)
        {
            case State.Walking:
                UpdateWalkingState();
                break;
            case State.Knockback:
                UpdateKnockbackState();
                break;
            case State.Dead:
                UpdateDeadState();
                break;
        }
    }

    // Walking State -------------------------------------------

    private void EnterWalkingState()
    {

    }

    private void UpdateWalkingState()
    {
        groundDetected = Physics2D.Raycast(groundCheck.position, Vector2.down, groundCheckDistance, whatIsGround);
        wallDetected = Physics2D.Raycast(wallCheck.position, transform.right, wallCheckDistance, whatIsGround);

        if (!groundDetected || wallDetected)
        {
            Flip();
        }
        else
        {
            movement.Set(movementSpeed * facingDirection, aliveRb.velocity.y);
            aliveRb.velocity = movement;
        }
    }

    private void ExitWalkingState()
    {

    }

    // Knockback State -------------------------------------------

    private void EnterKnockbackState()
    {

    }

    private void UpdateKnockbackState()
    {

    }

    private void ExitKnockbackState()
    {

    }

    // Dead State -------------------------------------------

    private void EnterDeadState()
    {

    }

    private void UpdateDeadState()
    {

    }

    private void ExitDeadState()
    {

    }

    // Other methods -------------------------------------------

    private void Flip()
    {
        facingDirection *= -1;
        alive.transform.Rotate(0.0f, 180.0f, 0.0f);
    }

    private void SwitchState(State state)
    {
        // exits from the current state
        switch (currentState)
        {
            case State.Walking:
                ExitWalkingState();
                break;
            case State.Knockback:
                ExitKnockbackState();
                break;
            case State.Dead:
                ExitDeadState();
                break;
        }
        
        // enters to the new state
        switch (state)
        {
            case State.Walking:
                EnterWalkingState();
                break;
            case State.Knockback:
                EnterKnockbackState();
                break;
            case State.Dead:
                EnterDeadState();
                break;
        }

        // sets the current state of the enemy
        currentState = state;
    }
}
