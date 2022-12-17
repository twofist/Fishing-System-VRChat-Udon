
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
    [UdonSynced] bool isKilled = false;

    void Start()
    {

    }

    void Update()
    {
        if (pickupable != pickup.pickupable)
        {
            transform.SetParent(null);
            pickup.pickupable = true;
        }
        if (isKilled && transform.parent != null)
        {
            transform.SetParent(null);
            if (Networking.IsOwner(Networking.LocalPlayer, gameObject))
            {
                objectPool.Return(gameObject);
            }
        }
    }

    public void OnCaught()
    {
        pickupable = true;
    }

    public void OnKillFish()
    {
        isKilled = true;
    }

    public void SetNetworkOwner(VRCPlayerApi player)
    {
        Networking.SetOwner(player, gameObject);
    }
}
