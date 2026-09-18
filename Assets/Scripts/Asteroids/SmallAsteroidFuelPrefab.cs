
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

//Script for the small asteroid when in its caught state.
public class SmallAsteroidFuelPrefab : UdonSharpBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        Heater heater = other.GetComponent<Heater>();
        if (heater == null) return;

        //Only the owner should add fuel, so it isn't counted once per client.
        if (Networking.IsOwner(gameObject))
        {
            heater.ReceiveFuelActivate();
        }

        //Every client destroys its own local copy directly, since OnTriggerEnter already fires locally on each client from the synced position.
        VRC_Pickup pickup = GetComponent<VRC_Pickup>();
        if (pickup != null)
        {
            pickup.Drop();
        }

        Destroy(gameObject);
    }
}
