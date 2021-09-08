using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAfterImageSprite : MonoBehaviour
{
    [SerializeField]
    private float activeTime = 0.1f; // used to define how this game object should be active for
    private float timeActivated; // used to keep track of how long this game object is active
    private float alpha; // used to keep track of what the alpha currently is
    [SerializeField]
    private float alphaSet = 0.8f; // used to set the alpha when the game object was enabled
    private float alphaMultiplier = 0.95f; // used to decrease the alpha overtime

    private Transform player; // used as a reference to a player game object to get its position and rotation

    private SpriteRenderer SR; // reference to this object's SpriteRenderer
    private SpriteRenderer playerSR; // reference to the player game objects's SpriteRenderer to get its current sprite

    private Color color; // used to change the color alpha of the sprite

    private void OnEnable()
    {
        SR = GetComponent<SpriteRenderer>(); // returns a reference of the "SpriteRenderer" component of this object
        player = GameObject.FindGameObjectWithTag("Player").transform; // returns a reference to the player object's "transform" component
        playerSR = player.GetComponent<SpriteRenderer>(); // returns a reference to the player object's "SpriteRenderer" component

        alpha = alphaSet; // sets the alpha

        SR.sprite = playerSR.sprite; // gets the sprite of the player
        transform.position = player.position; // sets this game object's position equal to the player's position
        transform.rotation = player.rotation; // sets this game object's rotation equal to the player's rotation

        timeActivated = Time.time; // starts the activation time
    }

    private void Update()
    {
        alpha *= alphaMultiplier; // decreases the alpha

        // changes the color this objects sprite
        color = new Color(1f, 1f, 1f, alpha);
        SR.color = color;

        // checks if this after image has been activated for long enough
        if (Time.time >= (timeActivated + activeTime))
        {
            PlayerAfterImagePool.Instance.AddToPool(gameObject);
        }
    }
}
