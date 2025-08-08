using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

//Script that handles the attraction of the fishing hook over the asteroids.
public class HookAttractor : UdonSharpBehaviour
{
    [SerializeField] private FishingRod _rod; //Reference to the fishing rod's script.
    [SerializeField] private float _magneticPullStrength = 5f; //Strength at which an asteroid is pulled toward the hook.
    [SerializeField] private AudioSource _catchingAsteroidSound; //Sound played when an asteroid is caught by the hook.
    private SmallAsteroid _asteroid; //The asteroid caught by the hook.
    private bool _hasCaughtSoundPlayed = false; //Has the sound for when the asteroid has been caught been played?

    public void AttractAsteroid()
    {
        _asteroid.isCaught = true;

        // Attract it slowly toward the hook
        Vector3 direction = (transform.position - _asteroid.transform.position).normalized;
        _asteroid.transform.position += direction * _magneticPullStrength * Time.deltaTime;

        // Close enough? Then catch it
        float dist = Vector3.Distance(transform.position, _asteroid.transform.position);
        if (dist < 0.3f)
        {
            _rod.CatchAsteroid(_asteroid.gameObject);

            if(!_hasCaughtSoundPlayed)
            {
                _catchingAsteroidSound.Play();
                _hasCaughtSoundPlayed = true;
            }

        }
    }

    private void OnTriggerStay(Collider other)
    {
        Debug.Log("Hook trigger detected: " + other.name);

        if (!_rod.isSecondTrigger || _rod.isRewinding || _rod.caughtAsteroid != null || _rod.currentLineLength < _rod.maxLineLength) return;

        // Check if the object is a valid asteroid
        _asteroid = other.GetComponent<SmallAsteroid>();
        if (_asteroid == null) return;

        SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "AttractAsteroid");
    }
}
