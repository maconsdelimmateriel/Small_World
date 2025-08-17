
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using TMPro;

public class Alien : UdonSharpBehaviour
{
    public UnityEngine.Object[] dialogLines;
    public DialogLineVRC[] dialogLinesVRC;

    [SerializeField] private GameObject _canvas;
    [SerializeField] private TextMeshProUGUI _text;

    public void StartDialog()
    {
        _canvas.SetActive(true);
        //_text.text = dialogLinesVRC[0].lineFrench;
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

[System.Serializable]
public class DialogLineVRC
{
    public string blocId; //The id for the dialog bloc.
    public string lineId; //The id for the dialog line.
    public SpeakerName speakerName; //Who speaks?
    public string lineFrench; //The dialog line in French.
    public string lineEnglish; //The dialog line in English.
}
