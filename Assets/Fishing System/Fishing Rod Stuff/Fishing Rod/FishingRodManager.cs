﻿
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using UnityEngine.UI;
using VRC.SDK3.Components;

public class FishingRodManager : UdonSharpBehaviour
{
    [Range(10, 150)] public int reelBarSize = 70;
    public RectTransform reelBar;
    public Slider slider;
    float sliderSpeed = 0.1f;
    GameObject fly;
    public Transform flySpawner;
    public Rigidbody rodRigidBody;
    public LineRenderer lineRenderer;
    public VRC_Pickup reelPickUp;
    public GameObject canvas;
    [HideInInspector] public int rotationsNeeded = 5;
    FlyManager flyManager;
    [HideInInspector] public FishManager fishManager;
    [HideInInspector] public VRCPlayerApi currentPlayer;
    public ReelManager reelManager;
    float startCastTime;
    Vector3 startCastPosition;
    Vector3 startCastRotation;
    public float playerCastStrength = 10;

    public AudioClip caughtFishSound;
    public AudioClip reelWindSound;
    public AudioSource audioSource;
    public VRCObjectPool objectPool;
    [UdonSynced] public Vector3 linePosition;
    void Start()
    {
        reelBar.sizeDelta = new Vector2(reelBarSize, reelBar.sizeDelta.y);
    }

    private void Update()
    {
        slider.value += sliderSpeed;
        if (slider.value >= slider.maxValue || slider.value <= slider.minValue)
        {
            sliderSpeed = -sliderSpeed;
        }
        if (fly != null)
        {
            lineRenderer.enabled = true;
            linePosition = lineRenderer.transform.InverseTransformPoint(fly.transform.position);
        }
        lineRenderer.SetPosition(1, linePosition);
    }

    public override void OnPickup()
    {
        base.OnPickup();
        if (currentPlayer != Networking.LocalPlayer)
        {
            SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "ResetRod");
            currentPlayer = Networking.LocalPlayer;
            Networking.SetOwner(currentPlayer, gameObject);
            Networking.SetOwner(currentPlayer, flySpawner.gameObject);
        }
    }

    public void ResetRod()
    {
        flyManager = null;
        if (fishManager)
        {
            OnFishLost();
        }
        fishManager = null;
        canvas.SetActive(false);
        linePosition = Vector3.zero;
        lineRenderer.enabled = false;
        audioSource.Stop();
        audioSource.loop = false;
    }
    public override void OnDrop()
    {
        if (!currentPlayer.IsUserInVR())
        {
            SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.Owner, "HandleDesktopCast");
        }
        base.OnDrop();
    }

    public override void InputUse(bool value, VRC.Udon.Common.UdonInputEventArgs args)
    {
        if (currentPlayer != Networking.LocalPlayer) return;
        if (!currentPlayer.IsUserInVR()) return;
        base.InputUse(value, args);
        if (value)
        {
            SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.Owner, "OnStartCast");
        }
        else
        {
            SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.Owner, "OnEndCast");
        }
    }

    public void HandleDesktopCast()
    {
        if (fishManager != null)
        {
            OnFishLost();
        }
        objectPool.Return(fly);
        fly = null;

        lineRenderer.enabled = false;
        linePosition = Vector3.zero;
        reelPickUp.pickupable = false;

        fly = objectPool.TryToSpawn();
        if (fly != null)
        {
            fly.transform.position = flySpawner.position;
            fly.transform.rotation = flySpawner.rotation;
            fly.transform.SetParent(null);

            flyManager = fly.GetComponent<FlyManager>();
            flyManager.fishingRodManager = this;
            Rigidbody rb = fly.GetComponent<Rigidbody>();
            flyManager.SetNetworkOwner(currentPlayer);

            Vector3 positionDistance = flySpawner.position - transform.position;
            Vector3 velocity = positionDistance * playerCastStrength;
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = false;
            rb.velocity = velocity;
        }

        reelPickUp.pickupable = true;
        lineRenderer.enabled = true;
        startReelWindSound();
    }

    public void OnStartCast()
    {
        if (fishManager != null)
        {
            OnFishLost();
        }
        objectPool.Return(fly);
        fly = null;
        lineRenderer.enabled = false;
        linePosition = Vector3.zero;
        reelPickUp.pickupable = false;
        startCastPosition = flySpawner.position;
        startCastTime = Time.time;
        startCastRotation = flySpawner.rotation.eulerAngles;
    }

    public void OnEndCast()
    {
        fly = objectPool.TryToSpawn();
        fly.transform.position = flySpawner.position;
        fly.transform.rotation = flySpawner.rotation;
        fly.transform.SetParent(null);
        flyManager = fly.GetComponent<FlyManager>();
        flyManager.fishingRodManager = this;
        Rigidbody rb = fly.GetComponent<Rigidbody>();
        flyManager.SetNetworkOwner(currentPlayer);
        Vector3 positionDistance = flySpawner.position - startCastPosition;
        Vector3 velocity = positionDistance * playerCastStrength;
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.isKinematic = false;
        rb.velocity = velocity;
        reelPickUp.pickupable = true;
        lineRenderer.enabled = true;
        startReelWindSound();
    }

    public void OnFishBite(float speed)
    {
        canvas.SetActive(true);
        sliderSpeed = speed;
        reelManager.canPullIn = true;
        audioSource.clip = caughtFishSound;
        audioSource.Play();
    }

    public void OnFishLost()
    {
        SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.Owner, "LoseFish");
    }

    public void LoseFish()
    {
        reelManager.ResetReel();
        canvas.SetActive(false);
        if (fishManager != null)
        {
            fishManager.OnKillFish();
        }
        fishManager = null;
        objectPool.Return(fly);
        fly = null;

        linePosition = Vector3.zero;
        lineRenderer.enabled = false;
        audioSource.Stop();
    }

    public void CatchFish()
    {
        reelManager.ResetReel();
        canvas.SetActive(false);
        if (fishManager != null)
        {
            fishManager.SetNetworkOwner(currentPlayer);
            fishManager.OnCaught();
            fishManager.transform.position = flySpawner.position;
        }
        fishManager = null;
        objectPool.Return(fly);
        fly = null;

        linePosition = Vector3.zero;
        lineRenderer.enabled = false;
        audioSource.Stop();
    }

    public void OnCatch()
    {
        SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.Owner, "CatchFish");
    }

    public void MoveFlyToNextPoint()
    {
        if (flyManager != null && flySpawner != null)
        {
            Vector3 from = flyManager.transform.position;
            Vector3 to = flySpawner.position;
            to = new Vector3(to.x, 0, to.z);
            from = new Vector3(from.x, 0, from.z);
            float distance = Vector3.Distance(from, to);
            Vector3 direction = (from - to).normalized;
            float moveDistance = distance / rotationsNeeded;
            Vector3 target = from + -direction * moveDistance;

            flyManager.targetPosition = target;
            flyManager.moveToTarget = true;

            startReelWindSound();
        }
    }

    void startReelWindSound()
    {
        if (!audioSource.isPlaying)
        {
            audioSource.clip = reelWindSound;
            audioSource.Play();
            audioSource.loop = true;
        }
    }
}
