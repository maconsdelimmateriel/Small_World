using UnityEngine;

//Scriptable object for a dialogline from one of the aliens.
[CreateAssetMenu(fileName = "DialogLine", menuName = "SmallWorld/DialogLine", order = 1)]
public class DialogLine : ScriptableObject
{
    public string blocId; //The id for the dialog bloc.
    public string lineId; //The id for the dialog line.
    public SpeakerName speakerName; //Who speaks?
    public string lineFrench; //The dialog line in French.
    public string lineEnglish; //The dialog line in English.
}

[System.Serializable]
public enum SpeakerName //Who speaks?
{
    Musica,
    Intercus,
    Liche
}

