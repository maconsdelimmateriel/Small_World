using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

//Pool of fuel asteroids placed in the scene.
//VRCInstantiate only creates a local object that other players never see, so caught fuel is taken from this pool instead.
[UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
public class FuelPool : UdonSharpBehaviour
{
    [SerializeField] private SmallAsteroidFuelPrefab[] _fuels; //Fuel objects in the scene, hidden until spawned.

    public void SpawnFuel(Vector3 position, Quaternion rotation)
    {
        foreach (SmallAsteroidFuelPrefab fuel in _fuels)
        {
            if (fuel != null && !fuel.isSpawned)
            {
                fuel.Spawn(position, rotation);
                return;
            }
        }

        Debug.LogWarning("[FuelPool] No free fuel left in the pool, add more fuel objects to it.");
    }
}
