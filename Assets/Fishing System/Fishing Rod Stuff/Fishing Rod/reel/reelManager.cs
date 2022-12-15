
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using UnityEngine.UI;

public class reelManager : UdonSharpBehaviour
{
    Vector3 originalGrabPosition;
    HumanBodyBones currentHand = HumanBodyBones.Head;
    VRCPlayerApi localPlayer;
    public VRC_Pickup pickup;
    float angleCounter = 0;
    int previousRotations = 0;
    int currentRotations = 0;
    public FishingRodManager fishingRodManager;
    void Start()
    {

    }

    private void Update()
    {
        if (localPlayer != null)
        {
            float angle = Vector3.Angle(originalGrabPosition, localPlayer.GetBonePosition(currentHand));
            transform.Rotate(angle, 0, 0);
            angleCounter += angle;
            currentRotations = (int)Mathf.Abs(angleCounter);
            if (currentRotations >= fishingRodManager.rotationsNeeded)
            {
                fishingRodManager.OnCatch();
                pickup.Drop();
                currentRotations = 0;
            }
        }
        if (previousRotations != currentRotations)
        {
            OnRotationsChanged();
        }
    }

    void OnRotationsChanged()
    {
        float offset = (fishingRodManager.reelBar.sizeDelta.x - fishingRodManager.reelBarSize) / 2;
        if (fishingRodManager.slider.value < offset || fishingRodManager.slider.value > fishingRodManager.reelBar.sizeDelta.x - offset)
        {
            fishingRodManager.OnFishLost();
            pickup.Drop();
        }
        else
        {
            fishingRodManager.MoveFlyToNextPoint();
        }

    }

    public override void OnPickup()
    {
        base.OnPickup();
        localPlayer = Networking.LocalPlayer;
        if (localPlayer.GetPickupInHand(VRC_Pickup.PickupHand.Left) == pickup)
        {
            currentHand = HumanBodyBones.LeftHand;
        }
        else if (localPlayer.GetPickupInHand(VRC_Pickup.PickupHand.Right) == pickup)
        {
            currentHand = HumanBodyBones.RightHand;

        }
        else
        {
            currentHand = HumanBodyBones.Head;
        }
        if (currentHand != HumanBodyBones.Head)
        {
            originalGrabPosition = localPlayer.GetBonePosition(currentHand);
            angleCounter = 0;
            currentRotations = 0;
        }
    }

    public override void OnDrop()
    {
        base.OnDrop();
        localPlayer = null;
        angleCounter = 0;
    }
}
