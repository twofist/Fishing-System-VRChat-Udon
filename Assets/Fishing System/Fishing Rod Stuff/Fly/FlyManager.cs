﻿
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class FlyManager : UdonSharpBehaviour
{
    Rigidbody rb;

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
    void Start()
    {

    }

    private void Update()
    {
        if (isTimerOn)
        {
            if (timerCounter >= timer)
            {
                CheckForFishBite();
                timerCounter = 0;
                isTimerOn = false;
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
        WaterManager waterManager = other.gameObject.GetComponent<WaterManager>();
        if (waterManager != null)
        {
            isTimerOn = true;
            catchableFish = waterManager.catchableFish;
            catchChances = waterManager.catchChances;
        }
    }

    void CheckForFishBite()
    {
        if (Random.Range(0, 100) < 10)
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
            fish = Instantiate(catchableFish[bitFish], transform);
            FishManager fishManager = fish.GetComponent<FishManager>();
            fishingRodManager.fishManager = fishManager;
        }
        else
        {
            for (int i = 0; i < catchableFish.Length - 1; i++)
            {
                if (Random.Range(0, 100) < catchChances[i])
                {
                    fish = Instantiate(catchableFish[i], transform);
                    FishManager fishManager = fish.GetComponent<FishManager>();
                    fishingRodManager.fishManager = fishManager;
                    break;
                }
            }
        }
        if (fish != null)
        {
            FishManager fishManager = fish.GetComponent<FishManager>();
            fishingRodManager.OnFishBite(fishManager.sliderSpeed);
        }
    }
}
