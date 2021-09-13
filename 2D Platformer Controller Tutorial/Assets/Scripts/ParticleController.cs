using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleController : MonoBehaviour
{
    // Called on the last frame of the hit particle's animation
    private void FinishAnim()
    {
        Destroy(gameObject);
    }
}
