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

    [Header("References")]
    public Transform rodTip;
    public Transform hook;
    public LineRenderer lineRenderer;

    [Header("Input Flags")]
    public bool triggerHeld = false;
    public bool rewindPressed = false;

    private bool isCasting = false;
    private bool isRewinding = false;
    private float currentLineLength = 0f;

    private Vector3 castDirection;
    private GameObject caughtAsteroid;

    void Start()
    {
        
    }

    void Update()
    {
        UpdateLineRenderer();

        if (!isCasting && triggerHeld)
        {
            BeginCast();
        }

        if (isCasting && !triggerHeld && !isRewinding)
        {
            isCasting = false; // cancel cast if trigger released early
        }

        if (isCasting && !isRewinding && currentLineLength < maxLineLength)
        {
            ExtendLine();
        }
    }

    private void BeginCast()
    {
        isCasting = true;
        isRewinding = false;
        currentLineLength = 0f;
        castDirection = rodTip.forward;
    }

    private void ExtendLine()
    {
        currentLineLength +=  Time.deltaTime;
        currentLineLength = Mathf.Min(currentLineLength, maxLineLength);

        Vector3 wobble = new Vector3(
            Mathf.PerlinNoise(Time.time * 3f, 0f) - 0.5f,
            Mathf.PerlinNoise(0f, Time.time * 3f) - 0.5f,
            0f
        ) * 0.2f;

        hook.position = rodTip.position + castDirection * currentLineLength + wobble;
    }

    private void RewindLine()
    {
        
    }

    private void FinishCatch()
    {
        
    }

    private void ResetLine()
    {
        
    }

    private void UpdateLineRenderer()
    {
        
    }

    public void OnTriggerEnter(Collider other)
    {
        if (!isCasting || isRewinding || caughtAsteroid != null) return;

        SmallAsteroid asteroid = other.GetComponent<SmallAsteroid>();

        if (asteroid != null)
        {
            caughtAsteroid = other.gameObject;
            isRewinding = true;
        }
    }

    public override void OnPickupUseDown()
    {
        triggerHeld = true;
        hook.parent = this.gameObject.transform.parent;
    }

    public override void OnPickupUseUp()
    {
        triggerHeld = false;
    }
}