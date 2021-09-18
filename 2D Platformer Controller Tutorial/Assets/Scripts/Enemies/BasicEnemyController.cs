using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicEnemyController : MonoBehaviour
{
    private enum State // used as the state machine for the enemy
    {
        Moving,
        Knockback,
        Dead
    }

    private State currentState; // points to the current state of the enemy

    [SerializeField]
    private float
        groundCheckDistance, // value used to detect if there is a ground in front of the enemy
        wallCheckDistance, // value used to detect if there is a wall in front of the enemy
        movementSpeed, // 
        maxHealth,
        knockbackDuration;

    [SerializeField]
    private Transform
        groundCheck,
        wallCheck;
    
    [SerializeField]
    private LayerMask whatIsGround;
    
    [SerializeField]
    private Vector2 knockbackSpeed;
    
    [SerializeField]
    private GameObject
        hitParticle,
        deathChunkParticle,
        deathBloodParticle;

    private float 
        currentHealth,
        knockbackStartTime;

    private int
        facingDirection,
        damageDirection;

    private Vector2 movement;

    private bool
        groundDetected,
        wallDetected;

    private GameObject alive; // reference to the enemy's alive version's game object
    private Rigidbody2D aliveRb; // reference to the enemy's alive version's rigid body component
    private Animator aliveAnim; // reference to the enemy's alive version's animator component

    private void Start()
    {
        alive = transform.Find("Alive").gameObject; // gets a reference to the child game object named "Alive"
        aliveRb = alive.GetComponent<Rigidbody2D>(); // gets a reference to the rigid body component of the alive game object
        aliveAnim = alive.GetComponent<Animator>(); // gets a reference to the animator component of the alive game object

        currentHealth = maxHealth; // sets the current health of the enemy
        facingDirection = 1; // sets the default facing of the enemy
    }

    private void Update()
    {
        // updates the enemy according to its current state
        switch (currentState)
        {
            case State.Moving:
                UpdateMovingState();
                break;
            case State.Knockback:
                UpdateKnockbackState();
                break;
            case State.Dead:
                UpdateDeadState();
                break;
        }
    }

    // Moving State -------------------------------------------

    private void EnterMovingState()
    {

    }

    private void UpdateMovingState()
    {
        groundDetected = Physics2D.Raycast(groundCheck.position, Vector2.down, groundCheckDistance, whatIsGround); // casts a ray from child "groundCheck" position down to check for ground
        wallDetected = Physics2D.Raycast(wallCheck.position, transform.right, wallCheckDistance, whatIsGround); // casts a ray from child "wallCheck" position to the front of it to check for wall

        if (!groundDetected || wallDetected) // if no ground was detected or a wall was detected
        {
            Flip();
        }
        else // just continue moving on the current facing of the enemy
        {
            movement.Set(movementSpeed * facingDirection, aliveRb.velocity.y);
            aliveRb.velocity = movement;
        }
    }

    private void ExitMovingState()
    {

    }

    // Knockback State -------------------------------------------

    private void EnterKnockbackState()
    {
        knockbackStartTime = Time.time; // gets the current time

        // pushes the enemy away from the attacking character
        movement.Set(knockbackSpeed.x * damageDirection, knockbackSpeed.y);
        aliveRb.velocity = movement;

        aliveAnim.SetBool("knockback", true); // sets the enemy's current animation to knockback
    }

    private void UpdateKnockbackState()
    {
        // if the knockback duration expires
        if (Time.time >= knockbackStartTime + knockbackDuration)
        {
            SwitchState(State.Moving);
        }
    }

    private void ExitKnockbackState()
    {
        aliveAnim.SetBool("knockback", false); // sets the parameter "knockback" to false so that the animator component will switch to another specific animation
    }

    // Dead State -------------------------------------------

    private void EnterDeadState()
    {
        // spawn chunks and blood using the prefabs
        Instantiate(deathChunkParticle, alive.transform.position, deathChunkParticle.transform.rotation);
        Instantiate(deathBloodParticle, alive.transform.position, deathBloodParticle.transform.rotation);
        
        Destroy(gameObject); // removes the enemy from the scene
    }

    private void UpdateDeadState()
    {

    }

    private void ExitDeadState()
    {

    }

    // Other methods -------------------------------------------

    private void Damage(float[] attackDetails)
    {
        currentHealth -= attackDetails[0]; // reduce the enemy's current health

        Instantiate(hitParticle, alive.transform.position, Quaternion.Euler(0.0f, 0.0f, Random.Range(0.0f, 360.0f))); // create hit particles from the prefab

        if (attackDetails[1] > alive.transform.position.x) // gets the source direction of the attack
        {
            damageDirection = -1;
        }
        else
        {
            damageDirection = 1;
        }

        if (currentHealth > 0.0f) // if the enemy is still alive, set it to its knockback state
        {
            SwitchState(State.Knockback);
        }
        else if (currentHealth <= 0.0f) // if the enemy is still alive, set it to its dead state
        {
            SwitchState(State.Dead);
        }
    }

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
            case State.Moving:
                ExitMovingState();
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
            case State.Moving:
                EnterMovingState();
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

    private void OnDrawGizmos()
    {
        Gizmos.DrawLine(groundCheck.position, new Vector2(groundCheck.position.x, groundCheck.position.y - groundCheckDistance));
        Gizmos.DrawLine(wallCheck.position, new Vector2(wallCheck.position.x + wallCheckDistance, wallCheck.position.y));
    }
}
