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

    //Attract the asteroid slowly toward the hook. Called by the rod on every client.
    public void PullAsteroid(SmallAsteroid asteroid)
    {
        Vector3 direction = (transform.position - asteroid.transform.position).normalized;
        asteroid.transform.position += direction * _magneticPullStrength * Time.deltaTime;
    }

    public void PlayCatchSound()
    {
        if (_catchingAsteroidSound != null)
            _catchingAsteroidSound.Play();
    }

    private void OnTriggerStay(Collider other)
    {
        //Only the owner of the rod decides which asteroid is caught, the choice is then synced by the rod.
        if (!Networking.IsOwner(_rod.gameObject)) return;

        SmallAsteroid hookedAsteroid = other.GetComponent<SmallAsteroid>();
        if (hookedAsteroid == null) return;

        _rod.TryAttractAsteroid(hookedAsteroid);
    }
}
