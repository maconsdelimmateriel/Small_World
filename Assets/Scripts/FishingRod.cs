using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class FishingRod : UdonSharpBehaviour
{
    [Header("Rod Settings")]
    public int rodLevel = 1;
    public float maxLineLength = 15f;
    public float rewindSpeed = 5f;
    [SerializeField] private float _wobblingSpeed = 3f;
    [SerializeField] private float _wobblingAmplitude = 0.2f;

    [Header("References")]
    public Transform rodTip;
    public Transform hook;
    public LineRenderer lineRenderer;
    public Transform lineTip;

    [Header("Input Flags")]
    public bool triggerHeld = false;
    public bool rewindPressed = false;

    private bool isCasting = false;
    public bool isRewinding = false;
    public float currentLineLength = 0f;
    

    private Vector3 castDirection;
    public GameObject caughtAsteroid;

    void Start()
    {
        
    }

    void Update()
    {
        UpdateLineRenderer();

        //2. 
        if (!isCasting && triggerHeld)
        {
            BeginCast();
        }

        //4.
        if (isCasting && !isRewinding && currentLineLength < maxLineLength)
        {
            ExtendLine();
        }
        else if (currentLineLength >= maxLineLength)
        {
            WobbleHook();
        }

        //B.
        if (isCasting && !triggerHeld && !isRewinding)
        {
            isCasting = false; // cancel cast if trigger released early
            isRewinding = true;
        }

        //C.
        if (isRewinding)
        {
            RewindLine();
        }
    }

    //3.
    private void BeginCast()
    {
        isCasting = true;
        isRewinding = false;
        currentLineLength = 0f;
        castDirection = rodTip.up;
    }

    //5.
    private void ExtendLine()
    {
        currentLineLength +=  Time.deltaTime;
        currentLineLength = Mathf.Min(currentLineLength, maxLineLength);

        hook.position = rodTip.position + castDirection * currentLineLength;
    }

    private void WobbleHook()
    {
        Vector3 wobble = new Vector3(
            Mathf.PerlinNoise(Time.time * _wobblingSpeed, 0f) - 0.5f,
            Mathf.PerlinNoise(0f, Time.time * _wobblingSpeed) - 0.5f,
            0f
        ) * _wobblingAmplitude;

        hook.position = rodTip.position + castDirection * currentLineLength + wobble;
    }

    //D.
    private void RewindLine()
    {
        currentLineLength -= rewindSpeed * Time.deltaTime;

        currentLineLength = Mathf.Max(currentLineLength, 0f);
        hook.position = rodTip.position + castDirection * currentLineLength;

        if (caughtAsteroid != null)
        {
            caughtAsteroid.transform.position = hook.position;
        }

        if (currentLineLength <= 1f)
        {
            FinishCatch();
        }
    }

    private void FinishCatch()
    {
        /*if (caughtAsteroid != null)
        {
            caughtAsteroid.SetActive(false); // Or notify game logic to convert to fuel
            caughtAsteroid = null;
        }

        ResetLine();*/
    }

    private void ResetLine()
    {
        isCasting = false;
        isRewinding = false;
        currentLineLength = 0f;
        hook.position = rodTip.position;
        hook.parent = this.gameObject.transform;
    }

    private void UpdateLineRenderer()
    {
        if (lineRenderer)
        {
            lineRenderer.positionCount = 2;
            lineRenderer.SetPosition(0, rodTip.position);
            lineRenderer.SetPosition(1, hook.position);
        }
    }

    public void CatchAsteroid(GameObject asteroidObj)
    {
        caughtAsteroid = asteroidObj;
        //isCasting = false;
        isRewinding = false;
        //rewindPressed = true;

        // Optional: stick asteroid to hook
        asteroidObj.transform.SetParent(lineTip);
        asteroidObj.transform.localPosition = Vector3.zero;
        //asteroidObj.transform.localPosition = new Vector3(0f, 0f, -0.1f);
        asteroidObj.GetComponent<SphereCollider>().enabled = false;
    }

    /*public void OnTriggerEnter(Collider other)
    {
        if (!isCasting || isRewinding || caughtAsteroid != null) return;

        SmallAsteroid asteroid = other.GetComponent<SmallAsteroid>();

        if (asteroid != null)
        {
            caughtAsteroid = other.gameObject;
            isRewinding = true;
        }
    }*/

    //1. Trigger is held down.
    public override void OnPickupUseDown()
    {
        triggerHeld = true;
        hook.parent = this.gameObject.transform.parent;
    }

    //A. Trigger is released.
    public override void OnPickupUseUp()
    {
        triggerHeld = false;
    }
}