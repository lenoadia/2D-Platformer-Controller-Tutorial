using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAfterImagePool : MonoBehaviour
{
    [SerializeField]
    private GameObject afterImagePrefab; // used to store a reference to the prefab we'll be using for the after image

    private Queue<GameObject> availableObjects = new Queue<GameObject>(); // used to store all the objects that have been made that are not currently active

    public static PlayerAfterImagePool Instance { get; private set; } // basic singleton used to access this script from other scripts

    private void Awake()
    {
        Instance = this; // sets the reference to this game object
        GrowPool(); // prepares some game objects when they are needed
    }

    // Creates more objects for the pool
    private void GrowPool()
    {
        // creates ten game objects at a time
        for (int i = 0; i < 10; i++)
        {
            var instanceToAdd = Instantiate(afterImagePrefab);
            instanceToAdd.transform.SetParent(transform); // makes the game object created a child of the game object this script is attached to
            AddToPool(instanceToAdd);
        }
    }

    public void AddToPool(GameObject instance)
    {
        instance.SetActive(false);
        availableObjects.Enqueue(instance);
    }

    // Method called to get an object from the pool, this is called from other scripts instead of instantiate
    public GameObject GetFromPool()
    {
        // when trying to get an after image to spawn but there is no more available object
        if (availableObjects.Count == 0)
        {
            GrowPool(); // makes some more objects
        }

        var instance = availableObjects.Dequeue(); // takes an object from the queue
        instance.SetActive(true); // makes the method "OnEnable" get called in the player after image sprite script
        
        return instance; // returns the object taken from the queue
    }
}
