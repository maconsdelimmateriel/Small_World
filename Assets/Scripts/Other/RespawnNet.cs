using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

//Respawns players on the asteroid if they fall off the asteroid.
public class RespawnNet : UdonSharpBehaviour
{
    [SerializeField]
    private Transform _spawnPoint; //Position at which the player will be respawned.
    [SerializeField]

    public override void OnPlayerTriggerEnter(VRCPlayerApi player)
    {
        base.OnPlayerTriggerEnter(player);

        player.TeleportTo(_spawnPoint.position, Quaternion.identity);
    }
}
