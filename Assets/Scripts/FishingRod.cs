using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

//Script for the fishing rod.
public class FishingRod : UdonSharpBehaviour
{
    [Header("Rod Settings")]
    private int _rodLevel = 1; //Determines level of asteroids this rod can catch.
    public float maxLineLength = 15f; //Maximum length of the fishing line.
    [SerializeField] private float _extendingSpeed = 5f; //Speed at which the line extends if trigger is pressed.
    [SerializeField] private float _rewindSpeed = 5f; //Speed at which the line comes back to the rod when trigger is released.
    [SerializeField] private float _wobblingSpeed = 3f; //Speed at which the hook woobles in space.
    [SerializeField] private float _wobblingAmplitude = 0.2f; //Amplitude at which the hook wobbles in space.
    public float currentLineLength = 0f; //Current length of the casted fishing line.

    [Header("References")]
    [SerializeField] private Transform _rodTip; //Transform of the tip of the fishing rod.
    [SerializeField] private Transform _hook; //Transform of the hook.
    [SerializeField] private LineRenderer lineRenderer; //Component that generates the fishing line.

    [Header("Booleans")]
    public bool triggerHeld = false; //Is the trigger held?
    private bool _rewindPressed = false; //Should the line rewind?
    public bool isRewinding = false; //Is the line being rewinded?
    private bool _isCasting = false; //Is the fishing rod in use?
    
    private Vector3 _castDirection; //Direction at which the fishing line is casted.
    public GameObject caughtAsteroid; //The asteroid that is currently hooked.


    void Update()
    {
        SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "UpdateLineRenderer");

        //2. 
        if (!_isCasting && triggerHeld)
        {
            SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "BeginCast");
        }

        //4.
        if (_isCasting && !isRewinding && currentLineLength < maxLineLength)
        {
            SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "ExtendLine");
        }
        else if (currentLineLength >= maxLineLength)
        {
            SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "WobbleHook");
        }

        //B.
        if (_isCasting && !triggerHeld && !isRewinding)
        {
            _isCasting = false; // cancel cast if trigger released early
            isRewinding = true;
        }

        //C.
        if (isRewinding)
        {
            SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "RewindLine");
        }
    }

    //3.
    public void BeginCast()
    {
        _isCasting = true;
        isRewinding = false;
        currentLineLength = 0f;
        _castDirection = _rodTip.up;
    }

    //5.
    public void ExtendLine()
    {
        currentLineLength +=  Time.deltaTime * _extendingSpeed;
        currentLineLength = Mathf.Min(currentLineLength, maxLineLength);

        _hook.position = _rodTip.position + _castDirection * currentLineLength;
    }

    public void WobbleHook()
    {
        Vector3 wobble = new Vector3(
            Mathf.PerlinNoise(Time.time * _wobblingSpeed, 0f) - 0.5f,
            Mathf.PerlinNoise(0f, Time.time * _wobblingSpeed) - 0.5f,
            0f
        ) * _wobblingAmplitude;

        _hook.position = _rodTip.position + _castDirection * currentLineLength + wobble;
    }

    //D.
    public void RewindLine()
    {
        currentLineLength -= _rewindSpeed * Time.deltaTime;

        currentLineLength = Mathf.Max(currentLineLength, 0f);
        _hook.position = _rodTip.position + _castDirection * currentLineLength;

        if (caughtAsteroid != null)
        {
            caughtAsteroid.transform.position = _hook.position;
        }

        if (currentLineLength <= 1f)
        {
            SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "FinishCatch");
        }
    }

    public void FinishCatch()
    {
        /*if (caughtAsteroid != null)
        {
            caughtAsteroid.SetActive(false); // Or notify game logic to convert to fuel
            caughtAsteroid = null;
        }

        ResetLine();*/
    }

    public void ResetLine()
    {
        _isCasting = false;
        isRewinding = false;
        currentLineLength = 0f;
        _hook.position = _rodTip.position;
        _hook.parent = this.gameObject.transform;
    }

    public void UpdateLineRenderer()
    {
        if (lineRenderer)
        {
            lineRenderer.positionCount = 2;
            lineRenderer.SetPosition(0, _rodTip.position);
            lineRenderer.SetPosition(1, _hook.position);
        }
    }

    public void CatchAsteroid(GameObject asteroidObj)
    {
        caughtAsteroid = asteroidObj;
        //isCasting = false;
        isRewinding = false;
        //rewindPressed = true;

        // Optional: stick asteroid to hook
        asteroidObj.transform.SetParent(_hook);
        asteroidObj.transform.localPosition = Vector3.zero;
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
        _hook.parent = this.gameObject.transform.parent;
    }

    //A. Trigger is released.
    public override void OnPickupUseUp()
    {
        triggerHeld = false;
    }
}