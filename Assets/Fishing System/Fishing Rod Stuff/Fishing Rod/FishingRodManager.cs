
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
    public GameObject fly;
    public Transform flySpawner;
    public Rigidbody rodRigidBody;
    public LineRenderer lineRenderer;
    public VRC_Pickup reelPickUp;
    public GameObject canvas;
    [HideInInspector] public int rotationsNeeded = 5;
    FlyManager flyManager;
    [HideInInspector] public FishManager fishManager;
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
            lineRenderer.SetPosition(1, transform.InverseTransformPoint(fly.transform.position));
        }
    }

    public override void OnPickupUseDown()
    {
        base.OnPickupUseDown();
        if (fly)
        {
            Destroy(fly);
            lineRenderer.enabled = false;
            lineRenderer.SetPosition(1, Vector3.zero);
            reelPickUp.pickupable = false;
        }
        else
        {
            fly = Instantiate(fly, flySpawner.position, flySpawner.rotation);
            flyManager = fly.GetComponent<FlyManager>();
            flyManager.fishingRodManager = this;
            Rigidbody rb = fly.GetComponent<Rigidbody>();
            rb.velocity = rodRigidBody.velocity;
            rb.angularVelocity = rodRigidBody.angularVelocity;
            reelPickUp.pickupable = true;
            lineRenderer.enabled = true;
        }
    }

    public void OnFishBite(float speed)
    {
        canvas.SetActive(true);
        sliderSpeed = speed;
    }

    public void OnFishLost()
    {
        canvas.SetActive(false);
        Destroy(fishManager.gameObject);
        Destroy(fly);
        lineRenderer.SetPosition(1, Vector3.zero);
        reelPickUp.pickupable = false;
    }

    public void OnCatch()
    {
        canvas.SetActive(false);
        fishManager.OnCaught();
        Destroy(fly);
        fishManager.transform.position = flySpawner.TransformPoint(flySpawner.position);
    }

    public void MoveFlyToNextPoint()
    {
        Vector3 from = flyManager.transform.position;
        Vector3 to = flySpawner.TransformPoint(flySpawner.position);
        to = new Vector3(to.x, 0, to.z);
        from = new Vector3(from.x, 0, from.z);
        float distance = Vector3.Distance(from, to);
        Vector3 direction = (from - to).normalized;
        float moveDistance = distance / rotationsNeeded;
        Vector3 target = from + direction * moveDistance;

        flyManager.targetPosition = target;
        flyManager.moveToTarget = true;
    }
}
