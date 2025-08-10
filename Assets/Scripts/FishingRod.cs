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
    [SerializeField] private float _arcHeight = 3f; // Controls the arc peak height of the casting.
    public float currentLineLength = 0f; //Current length of the casted fishing line.

    [Header("References")]
    [SerializeField] private Transform _rodTip; //Transform of the tip of the fishing rod.
    [SerializeField] private Transform _hook; //Transform of the hook.
    [SerializeField] private LineRenderer lineRenderer; //Component that generates the fishing line.

    [Header("Booleans")]
    public bool isSecondTrigger = false; //Has the trigger already been pulled once?
    private bool _rewindPressed = false; //Should the line rewind?
    public bool isRewinding = false; //Is the line being rewinded?
    private bool _isCasting = false; //Is the fishing rod in use?
    private bool _isInitialCasting = true;
    private bool _hasExtendingSoundPlayed = false; //Has the sound for when the line is extending been played?
    private bool _hasRewindingSoundPlayed = false; //Has the sound for when the line is extending been played?

    [Header("Sounds")]
    [SerializeField] private AudioSource _extendingLineSound; //Sound played while the fishing line is extending.
    [SerializeField] private AudioSource _rewindingLineSound; //Sound played while the fishing line is rewinding.

    private Vector3 _castDirection; //Direction at which the fishing line is casted.
    public GameObject caughtAsteroid; //The asteroid that is currently hooked.


    void Update()
    {
        SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "UpdateLineRenderer");

        //2. 
        if (!_isCasting && isSecondTrigger)
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
        if (_isCasting && !isSecondTrigger && !isRewinding)
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
        currentLineLength += Time.deltaTime * _extendingSpeed;
        currentLineLength = Mathf.Min(currentLineLength, maxLineLength);

        float t = currentLineLength / maxLineLength; // Progress along the cast (0 to 1)

        // Arc: parabola that peaks at t = 0.5
        float heightOffset = 4 * _arcHeight * t * (1 - t); // max is _arcHeight at midpoint

        Vector3 arcOffset = new Vector3(0f, heightOffset, 0f);
        _hook.position = _rodTip.position + _castDirection * currentLineLength + arcOffset;

        if(!_hasExtendingSoundPlayed)
        {
            if(_extendingLineSound != null)
                _extendingLineSound.Play();

            _hasExtendingSoundPlayed = true;
        }
    }

    public void WobbleHook()
    {
        if (_extendingLineSound != null)
            _extendingLineSound.Stop();

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

        if (!_hasRewindingSoundPlayed)
        {
            if (_rewindingLineSound != null)
                _rewindingLineSound.Play();

            _hasRewindingSoundPlayed = true;
        }

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
        if (_rewindingLineSound != null)
            _rewindingLineSound.Stop();

        _hasExtendingSoundPlayed = false;
        _hasRewindingSoundPlayed = false;

        /*if (caughtAsteroid != null)
        {
            caughtAsteroid.SetActive(false); // Or notify game logic to convert to fuel
            caughtAsteroid = null;
        }

        ResetLine();*/
    }

    //Not used?
    public void ResetLine()
    {
        _isCasting = false;
        isRewinding = false;
        currentLineLength = 0f;
        _hook.position = _rodTip.position;
        _hook.parent = this.gameObject.transform;

        _hasExtendingSoundPlayed = false;
        _hasRewindingSoundPlayed = false;
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
        if(!isSecondTrigger)
        {
            isSecondTrigger = true;
            _hook.parent = this.gameObject.transform.parent;
        }
        else
        {
            isSecondTrigger = false;
        }
        
    }

    /*//A. Trigger is released.
    public override void OnPickupUseUp()
    {
        isSecondTrigger = false;
    }*/
}