
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
    public Transform reelHandleSpawn;
    public Transform reelSpawn;
    [HideInInspector] public bool canPullIn = false;
    Vector3 lastPosition;
    void Start()
    {

    }

    private void Update()
    {
        float angle = Vector3.SignedAngle(RotatePointAroundPivot(lastPosition, reelSpawn.localPosition, new Vector3(transform.localEulerAngles.x, 0, 0)), reelHandle.localPosition, Vector3.zero);
        transform.Rotate(new Vector3(angle, 0, 0));
        if (canPullIn)
        {
            //float angle = Vector3.SignedAngle(lastPosition, reelHandle.localPosition, Vector3.zero);
            //float angle = Vector3.SignedAngle(RotatePointAroundPivot(lastPosition, reelSpawn.localPosition, new Vector3(transform.localEulerAngles.x, 0, 0)), reelHandle.localPosition, Vector3.zero);
            //float angle = Vector3.SignedAngle(RotatePointAroundPivot(reelHandleSpawn.localPosition, reelSpawn.localPosition, new Vector3(transform.localEulerAngles.x, 0, 0)), reelHandle.localPosition, Vector3.zero);


            angleCounter += angle;
            currentRotations = (int)Mathf.Abs(angleCounter) / 360;
        }
        if (previousRotations != currentRotations)
        {
            OnRotationsChanged();
        }
        lastPosition = reelHandle.localPosition;
    }

    Vector3 RotatePointAroundPivot(Vector3 point, Vector3 pivot, Vector3 angles)
    {
        Vector3 dir = point - pivot;
        dir = Quaternion.Euler(angles) * dir;
        point = dir + pivot;
        return point;
    }

    void OnRotationsChanged()
    {
        Debug.Log("rotations changed");
        float offset = (fishingRodManager.reelBar.sizeDelta.x - fishingRodManager.reelBarSize) / 2;
        Debug.Log("offset - " + offset);
        Debug.Log(fishingRodManager.slider.value);
        Debug.Log(fishingRodManager.slider.value < offset);
        Debug.Log(fishingRodManager.slider.value > fishingRodManager.reelBar.sizeDelta.x - offset);
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
        pickup.pickupable = false;
        canPullIn = false;
        reelHandle.localPosition = RotatePointAroundPivot(reelHandleSpawn.localPosition, reelSpawn.localPosition, new Vector3(transform.localEulerAngles.x, 0, 0));
        reelHandle.localRotation = reelHandleSpawn.localRotation;
    }

    public void OnLetGo()
    {
        reelHandle.localPosition = RotatePointAroundPivot(reelHandleSpawn.localPosition, reelSpawn.localPosition, new Vector3(transform.localEulerAngles.x, 0, 0));
        reelHandle.localRotation = reelHandleSpawn.localRotation;
    }
}
