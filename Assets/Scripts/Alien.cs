
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using TMPro;

public class Alien : UdonSharpBehaviour
{
    [SerializeField] private UnityEngine.Object[] _dialogLines;
    [SerializeField] private GameObject _canvas;
    [SerializeField] private TextMeshProUGUI _text;

    public void StartDialog()
    {
        _canvas.SetActive(true);
        //_text.text = _dialogLines[0].lineFrench;
    }

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
