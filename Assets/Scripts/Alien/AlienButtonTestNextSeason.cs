using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

//Advances to next season immediately.
public class AlienButtonTestNextSeason : UdonSharpBehaviour
{
    [SerializeField] private Alien _alien; //Alien's script.

    public override void Interact()
    {
        _alien.OnClickSeason();
    }
}
