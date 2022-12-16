﻿
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using UnityEngine.UI;

public class FishingRodManager : UdonSharpBehaviour
{
    [Range(10, 150)] public int reelBarSize = 70;
    public RectTransform reelBar;
    public Slider slider;
    float sliderSpeed = 0.1f;
    public GameObject flyPrefab;
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
            lineRenderer.SetPosition(1, lineRenderer.transform.InverseTransformPoint(fly.transform.TransformPoint(fly.transform.position)));
        }
    }

    public override void OnPickup()
    {
        base.OnPickup();
        currentPlayer = Networking.LocalPlayer;
        Networking.SetOwner(currentPlayer, gameObject);
    }
    public override void OnDrop()
    {
        base.OnDrop();
        currentPlayer = null;
    }

    public override void InputUse(bool value, VRC.Udon.Common.UdonInputEventArgs args)
    {
        if (currentPlayer != Networking.LocalPlayer) return;
        base.InputUse(value, args);
        if (value)
        {
            SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "OnStartCast");
        }
        else
        {
            SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "OnEndCast");
        }
    }

    public void OnStartCast()
    {
        Destroy(fly);
        lineRenderer.enabled = false;
        lineRenderer.SetPosition(1, Vector3.zero);
        reelPickUp.pickupable = false;
        startCastPosition = flySpawner.position;
        startCastTime = Time.time;
        startCastRotation = flySpawner.rotation.eulerAngles;
    }

    public void OnEndCast()
    {
        fly = Instantiate(flyPrefab, flySpawner.position, flySpawner.rotation);
        flyManager = fly.GetComponent<FlyManager>();
        flyManager.fishingRodManager = this;
        Rigidbody rb = fly.GetComponent<Rigidbody>();
        flyManager.SetNetworkOwner(currentPlayer);
        Vector3 positionDistance = flySpawner.position - startCastPosition;
        Vector3 velocity = positionDistance * playerCastStrength;
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
        SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "LoseFish");
    }

    public void LoseFish()
    {
        reelManager.ResetReel();
        canvas.SetActive(false);
        Destroy(fishManager.gameObject);
        Destroy(fly);
        lineRenderer.SetPosition(1, lineRenderer.GetPosition(0));
        lineRenderer.enabled = false;
        audioSource.Stop();
    }

    public void CatchFish()
    {
        reelManager.ResetReel();
        canvas.SetActive(false);
        fishManager.OnCaught();
        fishManager.SetNetworkOwner(currentPlayer);
        Destroy(fly);
        fishManager.transform.position = flySpawner.position;
        lineRenderer.SetPosition(1, lineRenderer.GetPosition(0));
        lineRenderer.enabled = false;
        audioSource.Stop();
    }

    public void OnCatch()
    {
        SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "CatchFish");
    }

    public void MoveFlyToNextPoint()
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
