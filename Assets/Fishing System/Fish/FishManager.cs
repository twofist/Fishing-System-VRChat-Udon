
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class FishManager : UdonSharpBehaviour
{
    [Range(0.1f, 1)] public float sliderSpeed = .3f;
    public VRC_Pickup pickup;

    void Start()
    {

    }

    public void OnCaught()
    {
        transform.SetParent(null);
        pickup.pickupable = true;
    }
}
