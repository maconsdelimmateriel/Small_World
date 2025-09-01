using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

//Closes the alien's dialog panel.
public class AlienButtonClose : UdonSharpBehaviour
{
    [SerializeField] private Alien _alien; //Alien's script.

    public override void Interact()
    {
        _alien.OnClickClose();
    }
}
