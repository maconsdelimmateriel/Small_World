
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using System.Collections.Generic;

public class SmallAsteroidsManager : UdonSharpBehaviour
{
    [Header("Manually Assigned Asteroids (Disabled in Scene)")]
    [SerializeField] private GameObject[] _asteroidPool; //All the small asteroids.

    [Header("Activation Settings")]
    public float activationInterval = 1f; //Interval at which a new small asteroid appears.

    private float _timer;

    void Update()
    {
        _timer += Time.deltaTime;

        if (_timer >= activationInterval)
        {
            _timer = 0f;
            SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "ActivateAsteroidFromPool");
        }
    }

    //Activates an asteroid.
    public void ActivateAsteroidFromPool()
    {
        foreach (GameObject asteroid in _asteroidPool)
        {
            if (!asteroid.activeInHierarchy)
            {
                SmallAsteroid orbit = asteroid.GetComponent<SmallAsteroid>();
                if (orbit != null)
                {
                    orbit.randomizeStartAngle = true;
                    orbit.randomizeSpeed = true;
                    orbit.orbitTiltEuler = new Vector3(
                        Random.Range(-30f, 30f),
                        Random.Range(0f, 360f),
                        0f
                    );
                }

                asteroid.SetActive(true);
                break;
            }
        }
    }

    //Deactivates an asteroid. Will probably be used when all the fuel in an asteroid has been depleted.
    /*public void ReturnToPool(GameObject asteroid)
    {
        if (asteroidPool.Contains(asteroid))
        {
            asteroid.SetActive(false);
        }
    }
    */
}
