
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
                Debug.Log("timer ended");
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
            isTimerOn = true;
            catchableFish = waterManager.catchableFish;
            catchChances = waterManager.catchChances;
            objectPools = waterManager.objectPools;
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
        Debug.Log("bite! " + catchableFish.Length + " - " + catchableFish + " - " + catchChances.Length);
        Debug.Log(Networking.GetOwner(gameObject).playerId);
        Debug.Log(Networking.LocalPlayer.playerId);
        if (catchChances.Length < 1)
        {
            int bitFish = Random.Range(0, catchableFish.Length - 1);
            bitFishIndex = bitFish;
            Debug.Log("create fish");
            SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.Owner, "CreateFish");
        }
        else
        {
            for (int i = 0; i < catchableFish.Length - 1; i++)
            {
                if (Random.Range(0, 100) < catchChances[i])
                {
                    bitFishIndex = i;
                    Debug.Log("create fish");
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
        Networking.SetOwner(Networking.GetOwner(gameObject), objectPools[bitFishIndex].gameObject);
        Debug.Log("spawn fish");
        //fish = Instantiate(catchableFish[bitFishIndex], transform.position, transform.rotation);
        fish = objectPools[bitFishIndex].TryToSpawn();
        if (fish != null)
        {
            fish.transform.position = transform.position;
            fish.transform.rotation = transform.rotation;
            fish.transform.SetParent(transform);
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
