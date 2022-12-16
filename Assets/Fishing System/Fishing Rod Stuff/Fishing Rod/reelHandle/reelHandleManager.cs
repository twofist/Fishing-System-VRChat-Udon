
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class reelHandleManager : UdonSharpBehaviour
{
    public ReelManager reelManager;
    void Start()
    {

    }

    public override void OnDrop()
    {
        base.OnDrop();
        reelManager.OnLetGo();
    }
}
