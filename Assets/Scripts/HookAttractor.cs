using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class HookAttractor : UdonSharpBehaviour
{
    public FishingRod rod;
    public float magneticPullStrength = 5f;

    private void OnTriggerStay(Collider other)
    {
        Debug.Log("Hook trigger detected: " + other.name);

        if (!rod.triggerHeld || rod.isRewinding || rod.caughtAsteroid != null || rod.currentLineLength < rod.maxLineLength) return;

        // Check if the object is a valid asteroid
        SmallAsteroid asteroid = other.GetComponent<SmallAsteroid>();
        if (asteroid == null) return;

        asteroid.isCaught = true;

        // Attract it slowly toward the hook
        Vector3 direction = (transform.position - asteroid.transform.position).normalized;
        asteroid.transform.position += direction * magneticPullStrength * Time.deltaTime;

        // Close enough? Then catch it
        float dist = Vector3.Distance(transform.position, asteroid.transform.position);
        if (dist < 0.3f)
        {
            rod.CatchAsteroid(asteroid.gameObject);
        }
    }
}
