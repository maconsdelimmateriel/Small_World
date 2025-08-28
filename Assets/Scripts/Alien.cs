
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
    public string[] season; //The season this dialog line is said.

    [SerializeField] private GameObject _canvas; //Reference to the dialog canvas.
    [SerializeField] private TextMeshProUGUI _text; //Reference to the text displayed on the dialog canvas.

    private int _language = 0; //Language of the dialog 0: French, 1: English
    private int _indexDialog = 0; //Index of the current line of dialog displayed.

    public string currentSeason = "Winter"; //What season is currently on the asteroid? 

    [SerializeField] private int[] _maxIndexDialog; //Maximum dialog index this alien can reach for each season.

    //Starts the dialog.
    public void StartDialog()
    {
        _canvas.SetActive(true);

        // Find the first line for this season using a simple loop
        int firstIndex = -1;
        for (int i = 0; i < season.Length; i++)
        {
            if (season[i] == currentSeason)
            {
                firstIndex = i;
                break;
            }
        }

        if (firstIndex == -1)
        {
            _text.text = $"{currentSeason} No dialog available.";
            _indexDialog = -1; // No valid line
            return;
        }

        _indexDialog = 0; // Index within season
        DisplayLine(firstIndex); // Show the first line
    }

    //Launched when button Next is clicked.
    public void OnClickNext()
    {
        SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "NextDialog");
    }

    //Progresses to the next line of dialog.
    public void NextDialog()
    {
        // Collect indices of lines for this season
        int[] indicesForSeason = new int[lineFrench.Length];
        int count = 0;

        for (int i = 0; i < season.Length; i++)
        {
            if (season[i] == currentSeason)
            {
                indicesForSeason[count] = i;
                count++;
            }
        }

        // Safety: if no lines exist for this season, stop.
        if (count == 0)
        {
            _text.text = $"{currentSeason} No dialog available.";
            return;
        }

        // 2. Advance dialog index within the season
        _indexDialog++;
        if (_indexDialog >= count)
            _indexDialog = 0;

        int lineIndex = indicesForSeason[_indexDialog];
        DisplayLine(lineIndex);
    }

    //Helper to display the right language line
    private void DisplayLine(int lineIndex)
    {
        if (_language == 0)
        {
            _text.text = lineFrench[lineIndex];
        }
        else
        {
            _text.text = lineEnglish[lineIndex];
        }
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