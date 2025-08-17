using System.Collections;
using System.Collections.Generic;
using UnityEditor.Callbacks;
using UnityEngine;

public class DialogPostProcessScene : MonoBehaviour
{
    [PostProcessScene(callbackOrder:-10)]
    public static void OnPostProcessScene()
    {
        Alien musica = FindObjectOfType<Alien>();

        Object[] rawLines = musica.dialogLines;
        DialogLine[] dialogLines = new DialogLine[rawLines.Length];
        DialogLineVRC[] dialogLinesVRC = musica.dialogLinesVRC;

        for (int i = 0; i < rawLines.Length; i++)
        {
            dialogLines[i] = (DialogLine)rawLines[i];

            if (dialogLines[i] == null)
                continue;

            dialogLinesVRC[i].blocId = dialogLines[i].blocId;
            dialogLinesVRC[i].lineId = dialogLines[i].lineId;
            dialogLinesVRC[i].speakerName = dialogLines[i].speakerName;
            dialogLinesVRC[i].lineFrench = dialogLines[i].lineFrench;
            dialogLinesVRC[i].lineEnglish = dialogLines[i].lineEnglish;
        }
    }
}
