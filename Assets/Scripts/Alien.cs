
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using TMPro;

//Base class for an alien.
public class Alien : UdonSharpBehaviour
{
    public UnityEngine.Object[] dialogLines; //Reference to the scriptable objects for the alien's dialog lines.

    public string[] lineFrench; //Dialog lines in French compatible with VRChat.

    [SerializeField] private GameObject _canvas; //Reference to the dialog canvas.
    [SerializeField] private TextMeshProUGUI _text; //Reference to the text displayed on the dialog canvas.

    //Starts the dialog.
    public void StartDialog()
    {
        _canvas.SetActive(true);
        _text.text = lineFrench[0];
    }

    //Ends the dialog.
    public void EndDialog()
    {
        _canvas.SetActive(false);
    }

    public override void OnPlayerTriggerEnter(VRCPlayerApi player)
    {
        base.OnPlayerTriggerEnter(player);

        SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "StartDialog");
    }

    public override void OnPlayerTriggerExit(VRCPlayerApi player)
    {
        base.OnPlayerTriggerEnter(player);

        SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "EndDialog");
    }
}