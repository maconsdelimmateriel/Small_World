using UnityEditor.Callbacks;
using UnityEngine;

public class DialogPostProcessScene : MonoBehaviour
{
    [PostProcessScene(callbackOrder: -10)]
    public static void OnPostProcessScene()
    {
        Alien musica = FindObjectOfType<Alien>();
        if (musica == null) return;

        Object[] rawLines = musica.dialogLines;
        if (rawLines == null || rawLines.Length == 0) return;

        musica.lineFrench = new string[rawLines.Length];
        musica.lineEnglish = new string[rawLines.Length];
        musica.season = new string[rawLines.Length];

        for (int i = 0; i < rawLines.Length; i++)
        {
            DialogLine dl = rawLines[i] as DialogLine;
            if (dl == null) continue;

            musica.lineFrench[i] = dl.lineFrench;
            musica.lineEnglish[i] = dl.lineEnglish;
            musica.season[i] = dl.season.ToString();
        }

        // Clear ScriptableObject refs before build
        musica.dialogLines = null;
    }
}