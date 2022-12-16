
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using VRC.SDK3.Components;

public class FishManager : UdonSharpBehaviour
{
    [Range(0.1f, 1)] public float sliderSpeed = .3f;
    public VRC_Pickup pickup;
    [HideInInspector] public VRCObjectPool objectPool;
    [UdonSynced] public bool pickupable = false;

    void Start()
    {

    }

    void Update()
    {
        if (pickupable != pickup.pickupable)
        {
            pickup.pickupable = pickupable;
        }
    }

    public void OnCaught()
    {
        transform.SetParent(null);
        pickup.pickupable = true;
        pickupable = true;
    }

    public void OnKillFish()
    {
        transform.SetParent(null);
        objectPool.Return(gameObject);
    }

    public void SetNetworkOwner(VRCPlayerApi player)
    {
        Networking.SetOwner(player, gameObject);
    }
}
