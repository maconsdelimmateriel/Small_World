
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using TMPro;
using UnityEngine.UI;

//Script for the heater that warms the big asteroid.
public class Heater : UdonSharpBehaviour
{
    [SerializeField] private Slider _uiSlider; //UI that displays how much fuel is left in the heater.
    [SerializeField] private float _fuelBonus = 0.2f; //Placeholver number of how much fuel is added each time an asteroid is inserted in the heater.
    [SerializeField] private float _fuelLoss = 0.01f; //How much fuel is depleted each second.
    [UdonSynced] [SerializeField] private float _counter = 0f; //Counts how much fuel is left in the heater.
    private float _timer = 0f; //Timer for decreasing the fuel counter.


    public void ReceiveFuelActivate()
    {
        SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "ReceiveFuel");
    }

    //Called when fuel is added to the heater.
    public void ReceiveFuel()
    {
        _counter += _fuelBonus;
        SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "UpdateCounterText");
    }

    //Updates the UI each time the fuel decreases by 1.
    public void UpdateCounterText()
    {
        _uiSlider.value = _counter;
    }

    private void Update()
    {
        if (_counter > 0f)
        {
            _timer += Time.deltaTime;

            if (_timer >= 1f)
            {
                _counter -= _fuelLoss;
                RequestSerialization();

                SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "UpdateCounterText");

                _timer -= 1f;
            }
        }
    }

    public override void OnDeserialization()
    {
        // Late joiners now get:
        // - counter
    }
}
