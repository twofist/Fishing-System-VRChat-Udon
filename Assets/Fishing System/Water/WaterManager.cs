
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using VRC.SDK3.Components;

public class WaterManager : UdonSharpBehaviour
{
    public GameObject[] catchableFish;
    public int[] catchChances;
    public VRCObjectPool[] objectPools;
    void Start()
    {

    }
}
