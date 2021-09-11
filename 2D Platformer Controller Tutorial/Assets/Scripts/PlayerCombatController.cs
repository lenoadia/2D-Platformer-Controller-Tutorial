using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCombatController : MonoBehaviour
{
    [SerializeField]
    private bool combatEnabled; // states whether the character is enabled to attack or not
    [SerializeField]
    private float inputTimer; // states how long to hold the input for
    [SerializeField]
    private float attack1Radius; // used as an area of effect when detecting objects to be attacked or damaged
    [SerializeField]
    private float attack1Damage; // used as amount of the character's damage
    [SerializeField]
    private Transform attack1HitBoxPos; // child object used to get the position from where to detect objects to be attacked or damaged
    [SerializeField]
    private LayerMask whatIsDamageable; // layer used to make an object damageable

    private bool gotInput; // states whether the player has given an input
    private bool isAttacking; // states whether the character is attacking or not
    private bool isFirstAttack; // used to alternate between two different attack animations

    private float lastInputTime = Mathf.NegativeInfinity; // used to record the time the player has given input

    private Animator anim; // used to refer to the animator component of the game object this script is attached to

    private void Start()
    {
        anim = GetComponent<Animator>(); // gets a reference to the animator component of this class
        anim.SetBool("canAttack", combatEnabled); // sets the parameter "canAttack" to whether the character can attack or not
    }

    private void Update()
    {
        CheckCombatInput();
        CheckAttacks();
    }

    private void CheckCombatInput()
    {
        if (Input.GetMouseButtonDown(0)) // if the player clicks the left mouse button
        {
            if (combatEnabled) // if the character can attack
            {
                gotInput = true; // sets that an input was given by the player
                lastInputTime = Time.time; // store the time when the last input was given
            }
        }
    }

    private void CheckAttacks()
    {
        if (gotInput) // if the player has input
        {
            if (!isAttacking) // if the character currently not attack
            {
                gotInput = false;
                isAttacking = true; // sets the charcter on attacking state
                isFirstAttack = !isFirstAttack; // alternates between the two attack animations

                // sets the animator's parameters so it will play the attack animation
                anim.SetBool("attack1", true);
                anim.SetBool("isAttacking", isAttacking);
                anim.SetBool("firstAttack", isFirstAttack); // sets an animator's parameter so it will play either attack 1 or attack 2
            }
        }

        if (Time.time >= lastInputTime + inputTimer)
        {
            gotInput = false; // wait for new input
        }
    }

    // Detects objects to be damaged
    // Called on 2nd frame of attack animations
    private void CheckAttackHitBox()
    {
        Collider2D[] detectedObjects = Physics2D.OverlapCircleAll(attack1HitBoxPos.position, attack1Radius, whatIsDamageable); // gets all objects considered damageable in an area of a circle

        foreach (Collider2D collider in detectedObjects)
        {
            collider.transform.parent.SendMessage("Damage", attack1Damage); // sends message to the current iteration's object, calling its method called "Damage" with the parameter value of "attack1Damage"
            // can instantiate hit particle here, but it is better on each enemy so it can be different on different enemies
        }
    }

    // Cleans up the animation phase
    // Called on the last frame of the attack animations
    private void FinishAttack1()
    {
        isAttacking = false; // sets the character to not attacking state

        // sets animator's parameters to change the current animation of the character
        anim.SetBool("isAttacking", isAttacking);
        anim.SetBool("attack1", false);
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(attack1HitBoxPos.position, attack1Radius);
    }
}
