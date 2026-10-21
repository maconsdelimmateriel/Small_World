using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common.Interfaces;

//Item in the scene that hands out fishing rods when a player clicks on it.
//Rods are taken from a pool of hidden rods placed in the scene, so every player sees and can use them.
//The click is forwarded to the owner of the distributor, who alone picks the rod, so two players clicking at the same time never get the same rod.
[UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
public class FishingRodDistributor : UdonSharpBehaviour
{
    [SerializeField] private FishingRod[] _rods; //Fishing rods in the scene, hidden until distributed.
    [SerializeField] private Transform _spawnPoint; //Where a distributed rod appears.
    [SerializeField] private AudioSource _distributeSound; //Sound played on every client when a rod is distributed.
    [SerializeField] private AudioSource _emptySound; //Sound played locally when the pool is empty.

    public override void Interact()
    {
        if (GetFreeRod() == null)
        {
            if (_emptySound != null) _emptySound.Play();
            return;
        }

        SendCustomNetworkEvent(NetworkEventTarget.Owner, nameof(DistributeRod));
    }

    //Only run by the owner of the distributor.
    public void DistributeRod()
    {
        FishingRod rod = GetFreeRod();
        if (rod == null)
        {
            Debug.LogWarning("[FishingRodDistributor] No free rod left in the pool, add more rods to it.");
            return;
        }

        Transform spawn = _spawnPoint != null ? _spawnPoint : transform;
        rod.Spawn(spawn.position, spawn.rotation);

        SendCustomNetworkEvent(NetworkEventTarget.All, nameof(PlayDistributeSound));
    }

    public void PlayDistributeSound()
    {
        if (_distributeSound != null) _distributeSound.Play();
    }

    private FishingRod GetFreeRod()
    {
        foreach (FishingRod rod in _rods)
        {
            if (rod != null && !rod.isSpawned) return rod;
        }
        return null;
    }
}
