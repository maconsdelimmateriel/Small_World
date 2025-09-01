using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

//Translates the alien's dialog lines.
public class AlienButtonTranslate : UdonSharpBehaviour
{
    [SerializeField] private Alien _alien; //Alien's script.

    public override void Interact()
    {
        _alien.OnClickTranslate();
    }
}
