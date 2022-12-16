
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using UnityEngine.UI;

public class ReelManager : UdonSharpBehaviour
{
    VRCPlayerApi localPlayer;
    public VRC_Pickup pickup;
    float angleCounter = 0;
    int previousRotations = 0;
    int currentRotations = 0;
    public FishingRodManager fishingRodManager;
    public Transform reelHandle;
    Vector3 originalReelHandlePosition;
    Vector3 originalReelPosition;
    [HideInInspector] public bool canPullIn = false;
    void Start()
    {
        reelHandle.position = transform.parent.TransformPoint(new Vector3(0.059f, 0.0305f, 0.2912f));
        // Debug.Log(transform.parent.InverseTransformPoint(reelHandle.position) + " - " + transform.parent.TransformPoint(reelHandle.position) + " - " + reelHandle.position);
        originalReelHandlePosition = transform.parent.TransformPoint(new Vector3(0.059f, 0.0305f, 0.2912f));//new Vector3(0.059f, 0.0305f, 0.2912f); //reelHandle.localPosition; //fishingRodManager.transform.TransformPoint(reelHandle.position);

        // Debug.Log(reelHandle.position + " - " + reelHandle.localPosition);
        originalReelPosition = transform.position;
    }

    private void Update()
    {
        if (canPullIn)
        {
            float angle = Vector3.SignedAngle(originalReelPosition, transform.parent.TransformPoint(reelHandle.localPosition), originalReelPosition);
            angleCounter += angle;
            transform.localRotation = Quaternion.Euler(angle, 0, 0);
            currentRotations = (int)Mathf.Abs(angleCounter) / 360;
        }
        if (previousRotations != currentRotations)
        {
            OnRotationsChanged();
        }
    }

    void OnRotationsChanged()
    {
        Debug.Log("rotations changed");
        float offset = (fishingRodManager.reelBar.sizeDelta.x - fishingRodManager.reelBarSize) / 2;
        if (fishingRodManager.slider.value < offset || fishingRodManager.slider.value > fishingRodManager.reelBar.sizeDelta.x - offset)
        {
            fishingRodManager.OnFishLost();
        }
        else if (currentRotations >= fishingRodManager.rotationsNeeded)
        {
            fishingRodManager.OnCatch();
        }
        else
        {
            fishingRodManager.MoveFlyToNextPoint();
        }
    }

    public void ResetReel()
    {
        Debug.Log("reset reel");
        currentRotations = 0;
        previousRotations = 0;
        angleCounter = 0;
        pickup.Drop();
        Debug.Log("pickup position - " + reelHandle.position + " - " + originalReelHandlePosition + " - " + transform.parent.TransformPoint(originalReelHandlePosition) + " - " + transform.parent.TransformPoint(new Vector3(0.059f, 0.0305f, 0.2912f)));
        reelHandle.position = originalReelHandlePosition;
        pickup.pickupable = false;
        canPullIn = false;
    }
}
