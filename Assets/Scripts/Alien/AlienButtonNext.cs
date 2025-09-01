using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

//Advances to alien's next dialog line.
public class AlienButtonNext : UdonSharpBehaviour
{
    [SerializeField] private Alien _alien; //Alien's script.

    public override void Interact()
    {
        _alien.OnClickNext();
    }
}
