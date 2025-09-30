using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class SmallAsteroidsManager : UdonSharpBehaviour
{
    [SerializeField] private GameObject[] _asteroidPool; // All the small asteroids

    public float activationInterval = 1f; // Interval at which a new asteroid appears
    private float _timer;

    [UdonSynced] private int _indexAsteroidActive = 0;

    public override void OnPlayerJoined(VRCPlayerApi player)
    {
        if (Networking.IsOwner(gameObject))
        {
            RequestSerialization(); // Push latest state to late joiners
        }
    }

    public override void OnDeserialization()
    {
        // Late joiners activate the correct number of asteroids
        for (int i = 0; i < _indexAsteroidActive; i++)
        {
            if (!_asteroidPool[i].activeSelf)
            {
                _asteroidPool[i].SetActive(true);
            }
        }
    }

    void Update()
    {
        if (!Networking.IsOwner(gameObject)) return; // Only master activates

        _timer += Time.deltaTime;
        if (_timer >= activationInterval)
        {
            _timer = 0f;
            ActivateAsteroidFromPool();
        }
    }

    // Activates the next asteroid in the pool
    public void ActivateAsteroidFromPool()
    {
        if (_indexAsteroidActive >= _asteroidPool.Length) return;

        GameObject asteroid = _asteroidPool[_indexAsteroidActive];
        SmallAsteroid orbit = asteroid.GetComponent<SmallAsteroid>();

        if (orbit != null)
        {
            // Master randomizes orbit and syncs
            Networking.SetOwner(Networking.LocalPlayer, asteroid);
            orbit.GenerateOrbit();
        }

        asteroid.SetActive(true);

        _indexAsteroidActive++;
        RequestSerialization();
    }
}
