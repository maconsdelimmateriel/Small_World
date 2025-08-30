using System.Collections;
using System.Collections.Generic;
using UnityEditor.Callbacks;
using UnityEngine;

//Turns the scriptable objects for the dialog with the aliens into a format compatible with VRChat.
public class DialogPostProcessScene : MonoBehaviour
{
    [PostProcessScene(callbackOrder:-10)]
    public static void OnPostProcessScene()
    {
        Alien musica = FindObjectOfType<Alien>();

        Object[] rawLines = musica.dialogLines;
        DialogLine[] dialogLines = new DialogLine[rawLines.Length];
        string[] lineFrench = musica.lineFrench;
        string[] lineEnglish = musica.lineEnglish;

        for (int i = 0; i < rawLines.Length; i++)
        {
            dialogLines[i] = (DialogLine)rawLines[i];

            if (dialogLines[i] == null)
                continue;

            //lineFrench[i] = dialogLines[i].lineFrench;
            //lineEnglish[i] = dialogLines[i].lineEnglish;
        }

        rawLines = null;
    }
}
