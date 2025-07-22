using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class FishingRod : UdonSharpBehaviour
{
    [Header("Rod Settings")]
    public int rodLevel = 1;
    public float maxLineLength = 15f;
    public float throwSpeed = 5f;
    public float rewindSpeed = 5f;

    [Header("References")]
    public Transform rodTip;
    public Transform hook;
    public LineRenderer lineRenderer;

    [Header("Input Flags (Set by external logic)")]
    public bool triggerHeld = false;
    public bool rewindPressed = false;

    private bool isCasting = false;
    private bool isRewinding = false;
    private float currentLineLength = 0f;

    private Vector3 castDirection;
    private GameObject caughtAsteroid;

    void Start()
    {
        ResetLine();
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

        if (rewindPressed && isCasting)
        {
            isRewinding = true;
        }

        if (isRewinding)
        {
            RewindLine();
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
        currentLineLength += throwSpeed * Time.deltaTime;
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
        if (caughtAsteroid != null)
        {
            caughtAsteroid.SetActive(false); // Or notify game logic to convert to fuel
            caughtAsteroid = null;
        }

        ResetLine();
    }

    private void ResetLine()
    {
        isCasting = false;
        isRewinding = false;
        currentLineLength = 0f;
        hook.position = rodTip.position;
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
    }

    public override void OnPickupUseUp()
    {
        triggerHeld = false;
    }
}