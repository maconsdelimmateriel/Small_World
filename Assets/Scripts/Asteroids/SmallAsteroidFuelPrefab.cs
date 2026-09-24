using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.SDK3.Components;
using VRC.Udon;

//Script for the small asteroid when in its caught state.
//The object lives in a FuelPool and is shown/hidden through a synced flag, so every player sees the same fuel.
[UdonBehaviourSyncMode(BehaviourSyncMode.Continuous)]
public class SmallAsteroidFuelPrefab : UdonSharpBehaviour
{
    [UdonSynced] public bool isSpawned = false; //Is this fuel currently in the world?

    private VRC_Pickup _pickup;
    private VRCObjectSync _objectSync;
    private Renderer[] _renderers;
    private Collider[] _colliders;

    void Start()
    {
        _pickup = (VRC_Pickup)GetComponent(typeof(VRC_Pickup));
        _objectSync = (VRCObjectSync)GetComponent(typeof(VRCObjectSync));
        _renderers = GetComponentsInChildren<Renderer>(true);
        _colliders = GetComponentsInChildren<Collider>(true);
        ApplyState();
    }

    //Called by the FuelPool on the client of the player who caught the asteroid.
    public void Spawn(Vector3 position, Quaternion rotation)
    {
        Networking.SetOwner(Networking.LocalPlayer, gameObject);

        transform.SetPositionAndRotation(position, rotation);
        if (_objectSync != null) _objectSync.FlagDiscontinuity();

        isSpawned = true;
        ApplyState();
        RequestSerialization();
    }

    public override void OnDeserialization()
    {
        ApplyState();
    }

    private void ApplyState()
    {
        if (_renderers == null) return; //Start has not run yet, it will apply the state.

        foreach (Renderer r in _renderers) r.enabled = isSpawned;
        foreach (Collider c in _colliders) c.enabled = isSpawned;
        if (_pickup != null) _pickup.pickupable = isSpawned;
    }

    private void OnTriggerEnter(Collider other)
    {
        //Only the owner (the last player who held it) adds fuel and despawns it, then the state is synced to everyone.
        if (!isSpawned || !Networking.IsOwner(gameObject)) return;

        Heater heater = other.GetComponent<Heater>();
        if (heater == null) return;

        heater.ReceiveFuelActivate();

        if (_pickup != null) _pickup.Drop();

        isSpawned = false;
        ApplyState();
        RequestSerialization();
    }
}
