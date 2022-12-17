
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using VRC.SDK3.Components;

public class FlyManager : UdonSharpBehaviour
{
    public Rigidbody rb;

    float timerCounter = 0;
    float timer = 5f;
    bool isTimerOn = false;

    GameObject[] catchableFish;
    int[] catchChances;
    [HideInInspector] GameObject fish;
    [HideInInspector] public FishingRodManager fishingRodManager;
    [HideInInspector] public Vector3 targetPosition;
    [HideInInspector] public bool moveToTarget;
    public float moveSpeed = 1;
    [HideInInspector][UdonSynced] public int bitFishIndex;
    VRCObjectPool[] objectPools;
    void Start()
    {

    }

    private void Update()
    {
        if (isTimerOn)
        {
            if (timerCounter >= timer)
            {
                timerCounter = 0;
                isTimerOn = false;
                CheckForFishBite();
            }
            else
            {
                timerCounter += Time.deltaTime;
            }
        }
        if (moveToTarget)
        {
            if (Vector3.Distance(transform.position, targetPosition) < .1f)
            {
                transform.position = targetPosition;
                moveToTarget = false;
            }
            else
            {
                transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        rb.isKinematic = true;
        if (fishingRodManager != null)
        {
            fishingRodManager.audioSource.Stop();
        }
        WaterManager waterManager = other.gameObject.GetComponent<WaterManager>();
        if (waterManager != null)
        {

            catchableFish = waterManager.catchableFish;
            catchChances = waterManager.catchChances;
            objectPools = waterManager.objectPools;
            if (fishingRodManager != null && fishingRodManager.currentPlayer == Networking.LocalPlayer)
            {
                isTimerOn = true;
            }
        }
    }

    void CheckForFishBite()
    {
        if (Random.Range(0, 100) < 50)
        {
            HandleFishBite();
        }
        else
        {
            isTimerOn = true;
        }
    }

    void HandleFishBite()
    {
        if (catchChances.Length < 1)
        {
            int bitFish = Random.Range(0, catchableFish.Length - 1);
            bitFishIndex = bitFish;

            SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.Owner, "CreateFish");
        }
        else
        {
            for (int i = 0; i < catchableFish.Length - 1; i++)
            {
                if (Random.Range(0, 100) < catchChances[i])
                {
                    bitFishIndex = i;

                    SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.Owner, "CreateFish");
                    break;
                }
            }
        }
        if (fish != null)
        {
            FishManager fishManager = fish.GetComponent<FishManager>();
            fishingRodManager.OnFishBite(fishManager.sliderSpeed);
        }
        else
        {
            isTimerOn = true;
        }
    }

    public void CreateFish()
    {
        if (fishingRodManager.currentPlayer != Networking.LocalPlayer) return;
        Networking.SetOwner(fishingRodManager.currentPlayer, objectPools[bitFishIndex].gameObject);

        fish = objectPools[bitFishIndex].TryToSpawn();
        if (fish != null)
        {
            fish.transform.SetParent(transform);
            fish.transform.position = transform.position;
            fish.transform.rotation = transform.rotation;
            FishManager fishManager = fish.GetComponent<FishManager>();
            fishManager.objectPool = objectPools[bitFishIndex];
            fishingRodManager.fishManager = fishManager;
        }
    }

    public void SetNetworkOwner(VRCPlayerApi player)
    {
        Networking.SetOwner(player, gameObject);
    }
}
