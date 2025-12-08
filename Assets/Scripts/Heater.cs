
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

//Script for the heater that warms the big asteroid.
public class Heater : UdonSharpBehaviour
{
    public void ReceiveFuelActivate()
    {
        SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "ReceiveFuel");
    }

    //Called when fuel is added to the heater.
    public void ReceiveFuel()
    {
        Debug.Log("fuel");
    }
}
