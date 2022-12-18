
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using VRC.SDK3.Components;

public class WaterManager : UdonSharpBehaviour
{
    public int[] catchChances;
    public VRCObjectPool[] catchableFishObjectPools;
    void Start()
    {

    }
}
