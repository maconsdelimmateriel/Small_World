
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using TMPro;

//Script for the heater that warms the big asteroid.
public class Heater : UdonSharpBehaviour
{
    [SerializeField] private TextMeshProUGUI _uiText; //UI placeholder that displays how much fuel is left in the heater.
    [SerializeField] private float _fuelBonus = 10f; //Placeholver number of how much fuel is added each time an asteroid is inserted in the heater.
    [UdonSynced] private float _counter = 0f; //Counts how much fuel is left in the heater.
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
        _uiText.text = _counter.ToString();
    }

    private void Update()
    {
        if (_counter > 0f)
        {
            _timer += Time.deltaTime;

            if (_timer >= 1f)
            {
                _counter--;
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
