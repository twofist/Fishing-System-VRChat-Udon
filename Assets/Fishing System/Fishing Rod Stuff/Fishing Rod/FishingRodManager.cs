
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
    public Transform flySpawner;
    public Rigidbody rodRigidBody;
    public LineRenderer lineRenderer;
    public VRC_Pickup reelPickUp;
    public GameObject canvas;
    [HideInInspector] public int rotationsNeeded = 5;
    FlyManager flyManager;
    [HideInInspector] public FishManager fishManager;
    [HideInInspector][UdonSynced] public int currentPlayerID;
    public ReelManager reelManager;
    Vector3 startCastPosition;
    public float playerCastStrength = 10;
    public AudioClip caughtFishSound;
    public AudioClip reelWindSound;
    public AudioSource audioSource;
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
        if (flyManager != null)
        {
            linePosition = lineRenderer.transform.InverseTransformPoint(flyManager.transform.position);
        }
        lineRenderer.SetPosition(1, linePosition);
    }

    public override void OnPickup()
    {
        base.OnPickup();
        if (currentPlayerID != Networking.LocalPlayer.playerId)
        {
            SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "ResetRod");
            currentPlayerID = Networking.LocalPlayer.playerId;
            Networking.SetOwner(Networking.LocalPlayer, gameObject);
            Networking.SetOwner(Networking.LocalPlayer, flySpawner.gameObject);
        }
    }

    public void ResetRod()
    {
        ResetFly();
    }

    void ResetFly()
    {
        reelManager.ResetReel();
        canvas.SetActive(false);
        if (flyManager != null)
        {
            flyManager.moveToTarget = false;
        }
        if (fishManager != null)
        {
            fishManager.OnKillFish();
        }
        else
        {
            if (flyManager != null)
            {
                flyManager.flyObjectPool.Return(flyManager.gameObject);
            }
        }
        fishManager = null;
        flyManager = null;
        linePosition = lineRenderer.GetPosition(0);
        audioSource.Stop();
        audioSource.loop = false;
    }
    public override void OnDrop()
    {
        if (!Networking.LocalPlayer.IsUserInVR())
        {
            HandleDesktopCast();
        }
        base.OnDrop();
    }

    public override void InputUse(bool value, VRC.Udon.Common.UdonInputEventArgs args)
    {
        if (currentPlayerID == 0) return;
        if (currentPlayerID != Networking.LocalPlayer.playerId) return;
        if (!Networking.LocalPlayer.IsUserInVR()) return;
        base.InputUse(value, args);
        if (value)
        {
            OnStartCast();
        }
        else
        {
            OnEndCast();
        }
    }

    public void HandleDesktopCast()
    {
        BeginCast();

        DoCast(transform.position);
    }

    public void OnStartCast()
    {
        BeginCast();
        startCastPosition = flySpawner.position;
    }

    void BeginCast()
    {
        ResetFly();
    }

    public void OnEndCast()
    {
        DoCast(startCastPosition);
    }

    void DoCast(Vector3 startPosition)
    {
        GameObject fly = flySpawner.GetComponent<VRCObjectPool>().TryToSpawn();
        if (fly != null)
        {
            fly.transform.SetParent(null);
            fly.transform.position = flySpawner.position;
            fly.transform.rotation = flySpawner.rotation;

            flyManager = fly.GetComponent<FlyManager>();
            flyManager.fishingRodManager = this;
            Rigidbody rb = fly.GetComponent<Rigidbody>();
            flyManager.SetNetworkOwner(Networking.LocalPlayer);

            Vector3 positionDistance = flySpawner.position - startPosition;
            Vector3 velocity = positionDistance * playerCastStrength;
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = false;
            rb.velocity = velocity;
        }

        reelPickUp.pickupable = true;
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
        LoseFish();
    }

    public void LoseFish()
    {
        ResetRod();
    }

    public void CatchFish()
    {
        if (fishManager != null)
        {
            fishManager.SetNetworkOwner(Networking.LocalPlayer);
            fishManager.OnCaught();
            fishManager.transform.position = flySpawner.position;
            fishManager = null;
        }
        ResetRod();
    }

    public void OnCatch()
    {
        CatchFish();
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
