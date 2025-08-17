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

        for (int i = 0; i < rawLines.Length; i++)
        {
            dialogLines[i] = (DialogLine)rawLines[i];

            if (dialogLines[i] == null)
                continue; 
        }
    }
}
