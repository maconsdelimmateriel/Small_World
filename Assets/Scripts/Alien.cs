
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
    public string[] lineEnglish; //Dialog lines in English compatible with VRChat.

    [SerializeField] private GameObject _canvas; //Reference to the dialog canvas.
    [SerializeField] private TextMeshProUGUI _text; //Reference to the text displayed on the dialog canvas.

    private int _language = 0; //Language of the dialog 0: French, 1: English
    private int _indexDialog = 0; //Index of the current line of dialog displayed.

    //Starts the dialog.
    public void StartDialog()
    {
        _canvas.SetActive(true);
        _text.text = lineFrench[0];
    }

    //Launched when button Next is clicked.
    public void OnClickNext()
    {
        SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "NextDialog");
    }

    //Progresses to the next line of dialog.
    public void NextDialog()
    {

    }

    //Launched when the Close button is cliked.
    public void OnClickClose()
    {
        SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "CloseDialog");
    }

    //Ends the dialog.
    public void CloseDialog()
    {
        _canvas.SetActive(false);
    }

    //Launched when the translate button is clicked.
    public void OnClickTranslate()
    {
        SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "TranslateDialog");
    }

    //Changes the language of the dialog.
    public void TranslateDialog()
    {
        if (_language == 0)
        {
            _text.text = lineEnglish[_indexDialog];
            _language = 1;
        }
        else if (_language == 1)
        {
            _text.text = lineFrench[_indexDialog];
            _language = 0;
        }
    }

    public override void OnPlayerTriggerEnter(VRCPlayerApi player)
    {
        base.OnPlayerTriggerEnter(player);

        SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "StartDialog");
    }
}