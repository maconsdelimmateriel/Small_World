
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

//Script for the small asteroid when in its caught state.
public class SmallAsteroidFuelPrefab : UdonSharpBehaviour
{
    //Destroys the asteroid when put in the heater.
    public void DestroyShell()
    {
        Destroy(gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<Heater>() != null)
        {
            other.GetComponent<Heater>().ReceiveFuelActivate();
            SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "DestroyShell");
        }
    }
}
