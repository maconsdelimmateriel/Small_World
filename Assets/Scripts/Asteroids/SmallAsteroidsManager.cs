
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using System.Collections.Generic;

public class SmallAsteroidsManager : UdonSharpBehaviour
{
    [SerializeField] private GameObject[] _asteroidPool; //All the small asteroids.

    public float activationInterval = 1f; //Interval at which a new small asteroid appears.

    private float _timer;

    //Used to resynch asteroids already activated before player joins?
    /*[UdonSynced] [SerializeField] private bool[] _randomizeStartAngle;
    [UdonSynced] [SerializeField] private bool[] _randomizeSpeed;
    [UdonSynced] [SerializeField] private Vector3[] _orbitTiltEuler;*/
    [UdonSynced] private int _indexAsteroidActive = 0;

    public override void OnPlayerJoined(VRCPlayerApi player)
    {
        if (Networking.IsOwner(gameObject))
        {
            RequestSerialization(); // Push latest state to late joiner
        }
    }

    public override void OnDeserialization()
    {
        for (int i=0; i<_indexAsteroidActive; i++)
        {
            _asteroidPool[i].SetActive(true);
        }
    }

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

                _indexAsteroidActive++;
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
