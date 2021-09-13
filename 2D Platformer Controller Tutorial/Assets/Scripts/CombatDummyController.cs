using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatDummyController : MonoBehaviour
{
    /*
        maxHealth used to set the dummy's health
        knockbackSpeedX used to determine the horizontal velocity when the dummy is being knocked back
        knockbackSpeedY used to determine the vertical velocity when the dummy is being knocked back
        knockbackDuration used to set how long the knockback will happen
        knockbackDeathSpeedX used to determine the horizontal velocity when the dummy is dead and being knocked back
        knockbackDeathSpeedY used to determine the vertical velocity when the dummy is dead and being knocked back
        deathTorque used to determine the rotation of the dummy when it died and being knocked back
    */
    [SerializeField]
    private float maxHealth, knockbackSpeedX, knockbackSpeedY, knockbackDuration, knockbackDeathSpeedX, knockbackDeathSpeedY, deathTorque;
    [SerializeField]
    private bool applyKnockback; // used as a switch to determine whether the dummy, when hit while alive, will be knocked back or not
    [SerializeField]
    private GameObject hitParticle; // prefab used to create copies of particles when the dummy is being hit

    private float currentHealth; // used as the current health of the dummy
    private float knockbackStart;

    private int playerFacingDirection; // determines the facing of the character when it is hitting the dummy

    private bool playerOnLeft; // states whether the attacking character is on the left of the dummy or not
    private bool knockback; // states whether the dummy is currently being knocked back or not

    private PlayerController pc; // reference to the character game object
    private GameObject aliveGO, brokenTopGO, brokenBotGO; // references to the children game objects of the dummy object, they are used to the different states of the dummy
    private Rigidbody2D rbAlive, rbBrokenTop, rbBrokenBot; // references to the rigid body components of the children game objects of the dummy object
    private Animator aliveAnim; // reference to the animator component of the alive version of the dummy

    private void Start()
    {
        currentHealth = maxHealth; // sets the dummy's health

        pc = GameObject.Find("Player").GetComponent<PlayerController>(); // gets a game object named "Player", then returns its "PlayerController" component

        aliveGO = transform.Find("Alive").gameObject; // gets the game object of this object's child named "Alive"
        brokenTopGO = transform.Find("Broken Top").gameObject; // gets the game object of this object's child named "Broken Top"
        brokenBotGO = transform.Find("Broken Bottom").gameObject; // gets the game object of this object's child named "Broken Bottom"

        aliveAnim = aliveGO.GetComponent<Animator>(); // gets the animator component

        // gets the rigid body components of these objects
        rbAlive = aliveGO.GetComponent<Rigidbody2D>();
        rbBrokenTop = brokenTopGO.GetComponent<Rigidbody2D>();
        rbBrokenBot = brokenBotGO.GetComponent<Rigidbody2D>();

        aliveGO.SetActive(true); // activates the alive versions of this object

        // deactivates the broken versions of this object
        brokenTopGO.SetActive(false);
        brokenBotGO.SetActive(false);
    }

    private void Update()
    {
        CheckKnockback();
    }

    private void Damage(float amount)
    {
        currentHealth -= amount; // decreases the current health of the dummy
        playerFacingDirection = pc.GetFacingDirection(); // gets the facing direction of the attacking character

        Instantiate(hitParticle, aliveAnim.transform.position, Quaternion.Euler(0.0f, 0.0f, Random.Range(0.0f, 360.0f))); // creates a clone of "hitParticle" in the position of the dummy's alive version with a random rotation

        
        if (playerFacingDirection == 1) // if the attacking character is facing right while attacking this dummy
        {
            playerOnLeft = true; // sets that the character is on the left side
        }
        else // if the attacking character is facing left while attacking this dummy
        {
            playerOnLeft = false; // sets that the character is on the right side
        }

        // triggers a particular animation of the dummy's alive version depending on the facing direction of the attacking character
        aliveAnim.SetBool("playerOnLeft", playerOnLeft);
        aliveAnim.SetTrigger("damage");

        if (applyKnockback && currentHealth > 0.0f) // if the "applyKnockback" is switched on and the dummy is not yet dead
        {
            Knockback();
        }
        
        if (currentHealth <= 0.0f) // if the dummy has no more health
        {
            Die();
        }
    }

    private void Knockback()
    {
        knockback = true; // sets the dummy on state of being knocked back
        knockbackStart = Time.time; // sets the current time when the knockback is starting
        rbAlive.velocity = new Vector2(knockbackSpeedX * playerFacingDirection, knockbackSpeedY); // sets the velocity of the dummy's alive version to show being knocked back
    }

    private void CheckKnockback()
    {
        if (Time.time >= knockbackStart + knockbackDuration && knockback) // if the duration of the knockback has elapsed and the dummy is still on the knockback state
        {
            knockback = false; // sets the dummy from the state of being knockback
            rbAlive.velocity = new Vector2(0.0f, rbAlive.velocity.y); // stops the dummy's alive version from moving horizontally but keeping the vertical movement
        }
    }

    private void Die()
    {
        aliveGO.SetActive(false); // disables the alive version of the dummy
        brokenTopGO.SetActive(true); // enables the version of the dummy that represents its broken top part
        brokenBotGO.SetActive(true); // enables the version of the dummy that represents its broken bottom part

        brokenTopGO.transform.position = aliveGO.transform.position; // puts the broken top version of the dummy to the position of the alive version of the dummy
        brokenBotGO.transform.position = aliveGO.transform.position; // puts the broken bottom version of the dummy to the position of the alive version of the dummy

        rbBrokenBot.velocity = new Vector2(knockbackSpeedX * playerFacingDirection, knockbackSpeedY); // makes the broken bot version moving away from the attacking character, basic knockback
        rbBrokenTop.velocity = new Vector2(knockbackDeathSpeedX * playerFacingDirection, knockbackDeathSpeedY); // makes the broken top version moving away, with greater velocity, from the attacking character
        rbBrokenTop.AddTorque(deathTorque * -playerFacingDirection, ForceMode2D.Impulse); // rotates the broken top version of the dummy
    }
}
