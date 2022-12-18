
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using VRC.SDK3.Components;

public class FishManager : UdonSharpBehaviour
{
    [Range(0.1f, 1)] public float sliderSpeed = .3f;
    public VRC_Pickup pickup;
    [HideInInspector] public VRCObjectPool fishObjectPool;
    [UdonSynced] public bool pickupable = false;
    [UdonSynced] public bool isKilled = false;

    void Start()
    {

    }

    void Update()
    {
        if (pickupable != pickup.pickupable)
        {
            pickup.pickupable = true;
            HandleReset(false);
        }
        if (isKilled && transform.parent != null)
        {
            HandleReset(true);
        }
    }

    void ReturnFlyToPool(Transform fly)
    {
        if (fly != null)
        {
            FlyManager flyManager = fly.GetComponent<FlyManager>();
            if (flyManager != null)
            {
                flyManager.transform.SetParent(flyManager.flyObjectPool.transform);
                flyManager.flyObjectPool.Return(fly.gameObject);
            }
        }
    }

    public void OnCaught()
    {
        pickupable = true;
        HandleReset(false);
    }

    public void OnKillFish()
    {
        isKilled = true;
        HandleReset(true);
    }

    void HandleReset(bool resetFish)
    {
        Transform fly = transform.parent;
        transform.SetParent(null);
        if (resetFish)
        {
            transform.SetParent(fishObjectPool.transform);
            fishObjectPool.Return(gameObject);
        }
        ReturnFlyToPool(fly);
    }

    public void SetNetworkOwner(VRCPlayerApi player)
    {
        Networking.SetOwner(player, gameObject);
    }
}
