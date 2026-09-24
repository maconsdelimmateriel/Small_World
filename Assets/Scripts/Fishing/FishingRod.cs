using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

//Script for the fishing rod.
//Only the owner of the rod (the player who last picked it up) runs the fishing logic and changes the synced state.
//Every other client only reads the synced state and replays the visuals (hook, line, sounds, hooked asteroid).
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
    [SerializeField] private float _hookingDistance = 0.3f; //Distance at which an attracted asteroid sticks to the hook.
    [UdonSynced] public float currentLineLength = 0f; //Current length of the casted fishing line.

    [Header("References")]
    [SerializeField] private Transform _rodTip; //Transform of the tip of the fishing rod.
    [SerializeField] private Transform _hook; //Transform of the hook.
    [SerializeField] private LineRenderer lineRenderer; //Component that generates the fishing line.
    [SerializeField] private SmallAsteroidsManager _asteroidsManager; //Used to sync which asteroid is hooked, as an index in its pool.
    [SerializeField] private FuelPool _fuelPool; //Pool of networked fuel objects, one is spawned when an asteroid is reeled in.
    private HookAttractor _hookAttractor; //Script on the hook that detects asteroids.

    [Header("Synced state")]
    [UdonSynced] public bool isSecondTrigger = false; //Has the trigger already been pulled once?
    [UdonSynced] public bool isRewinding = false; //Is the line being rewinded?
    [UdonSynced] private bool _isCasting = false; //Is the fishing rod in use?
    [UdonSynced] private Vector3 _castDirection; //Direction at which the fishing line is casted.
    [UdonSynced] private int _targetAsteroidIndex = -1; //Index of the asteroid being attracted or hooked, -1 if none.
    [UdonSynced] private bool _isAsteroidHooked = false; //Has the target asteroid reached the hook?

    [Header("Sounds")]
    [SerializeField] private AudioSource _extendingLineSound; //Sound played while the fishing line is extending.
    [SerializeField] private AudioSource _rewindingLineSound; //Sound played while the fishing line is rewinding.

    //Local state, used by every client to detect changes of the synced state.
    private bool _wasCasting = false;
    private int _soundState = 0; //0 = silent, 1 = extending, 2 = wobbling, 3 = rewinding.
    private int _cachedTargetIndex = -1;
    private SmallAsteroid _targetAsteroid;
    private bool _wasHooked = false;

    void Start()
    {
        _hookAttractor = _hook.GetComponent<HookAttractor>();
    }

    void Update()
    {
        if (Networking.IsOwner(gameObject))
        {
            OwnerUpdate();
        }
        else
        {
            //Predict the line between two syncs so it moves smoothly, the synced value corrects it.
            AdvanceLineLength();
        }

        ApplyHookParent();
        PositionHook();
        UpdateTargetAsteroid();
        UpdateSounds();
        UpdateLineRenderer();
    }

    //Fishing logic, only run by the owner of the rod.
    private void OwnerUpdate()
    {
        if (!_isCasting && isSecondTrigger)
        {
            BeginCast();
        }

        if (!_isCasting) return;

        AdvanceLineLength();

        if (_targetAsteroid != null && !_isAsteroidHooked &&
            Vector3.Distance(_hook.position, _targetAsteroid.transform.position) < _hookingDistance)
        {
            _isAsteroidHooked = true;
        }

        if (isRewinding && currentLineLength <= 1f)
        {
            FinishCatch();
        }

        RequestSerialization();
    }

    private void BeginCast()
    {
        _isCasting = true;
        isRewinding = false;
        currentLineLength = 0f;
        _castDirection = _rodTip.right;
        RequestSerialization();
    }

    private void AdvanceLineLength()
    {
        if (!_isCasting) return;

        if (isRewinding)
        {
            currentLineLength = Mathf.Max(currentLineLength - _rewindSpeed * Time.deltaTime, 0f);
        }
        else
        {
            currentLineLength = Mathf.Min(currentLineLength + _extendingSpeed * Time.deltaTime, maxLineLength);
        }
    }

    //Detaches the hook from the rod while casting, and brings it back afterwards.
    private void ApplyHookParent()
    {
        if (_isCasting == _wasCasting) return;
        _wasCasting = _isCasting;

        if (_isCasting)
        {
            _hook.parent = this.gameObject.transform.parent;
        }
        else
        {
            _hook.parent = this.gameObject.transform;
            _hook.position = _rodTip.position;
        }
    }

    private void PositionHook()
    {
        _hook.rotation = _rodTip.rotation;

        if (!_isCasting) return;

        Vector3 linePosition = _rodTip.position + _castDirection * currentLineLength;

        if (isRewinding)
        {
            _hook.position = linePosition;
        }
        else if (currentLineLength >= maxLineLength)
        {
            Vector3 wobble = new Vector3(
                Mathf.PerlinNoise(Time.time * _wobblingSpeed, 0f) - 0.5f,
                Mathf.PerlinNoise(0f, Time.time * _wobblingSpeed) - 0.5f,
                0f
            ) * _wobblingAmplitude;

            _hook.position = linePosition + wobble;
        }
        else
        {
            float t = currentLineLength / maxLineLength; // Progress along the cast (0 to 1)

            // Arc: parabola that peaks at t = 0.5
            float heightOffset = 4 * _arcHeight * t * (1 - t); // max is _arcHeight at midpoint

            _hook.position = linePosition + new Vector3(0f, heightOffset, 0f);
        }
    }

    //Attracts, then sticks the target asteroid to the hook, on every client.
    private void UpdateTargetAsteroid()
    {
        if (_targetAsteroidIndex != _cachedTargetIndex)
        {
            //The asteroid got away before being hooked: it resumes its orbit.
            if (_targetAsteroid != null && !_wasHooked)
            {
                _targetAsteroid.isCaught = false;
            }

            _cachedTargetIndex = _targetAsteroidIndex;
            _targetAsteroid = null;
            _wasHooked = false;

            GameObject asteroidObj = _asteroidsManager.GetAsteroid(_targetAsteroidIndex);
            if (asteroidObj != null)
            {
                _targetAsteroid = asteroidObj.GetComponent<SmallAsteroid>();
            }
        }

        if (_targetAsteroid == null || _targetAsteroid.isConsumed) return;

        if (!_isAsteroidHooked)
        {
            _targetAsteroid.isCaught = true;
            _hookAttractor.PullAsteroid(_targetAsteroid);
        }
        else if (!_wasHooked)
        {
            _wasHooked = true;
            _targetAsteroid.isCaught = true;

            //Stick asteroid to hook
            _targetAsteroid.transform.SetParent(_hook);
            _targetAsteroid.transform.localPosition = Vector3.zero;
            _targetAsteroid.GetComponent<SphereCollider>().enabled = false;

            _hookAttractor.PlayCatchSound();
        }
    }

    //Called by the hook when an asteroid is in range.
    public void TryAttractAsteroid(SmallAsteroid asteroid)
    {
        if (!Networking.IsOwner(gameObject)) return;
        if (!_isCasting || !isSecondTrigger || isRewinding || _targetAsteroidIndex >= 0 || currentLineLength < maxLineLength) return;
        if (asteroid.isCaught || asteroid.isConsumed) return; //Already taken by another rod.

        int index = _asteroidsManager.GetAsteroidIndex(asteroid.gameObject);
        if (index < 0) return;

        _targetAsteroidIndex = index;
        _isAsteroidHooked = false;
        RequestSerialization();
    }

    //Only called by the owner, once the line is back to the rod.
    private void FinishCatch()
    {
        if (_targetAsteroid != null && _isAsteroidHooked)
        {
            if (_fuelPool != null)
            {
                _fuelPool.SpawnFuel(_targetAsteroid.transform.position, _targetAsteroid.transform.rotation);
            }

            _targetAsteroid.Consume();
        }

        ResetLine();
    }

    //Resets the line after catching an asteroid so it can catch again.
    private void ResetLine()
    {
        _isCasting = false;
        isRewinding = false;
        isSecondTrigger = false;
        currentLineLength = 0f;
        _targetAsteroidIndex = -1;
        _isAsteroidHooked = false;
        RequestSerialization();
    }

    //Plays the sounds matching the synced state, on every client.
    private void UpdateSounds()
    {
        int state = 0;
        if (_isCasting)
        {
            if (isRewinding) state = 3;
            else if (currentLineLength < maxLineLength) state = 1;
            else state = 2;
        }

        if (state == _soundState) return;
        _soundState = state;

        if (_extendingLineSound != null) _extendingLineSound.Stop();
        if (_rewindingLineSound != null) _rewindingLineSound.Stop();

        if (state == 1 && _extendingLineSound != null) _extendingLineSound.Play();
        if (state == 3 && _rewindingLineSound != null) _rewindingLineSound.Play();
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

    //1. Trigger is pressed. Only fires for the player holding the rod, who is its owner.
    public override void OnPickupUseDown()
    {
        if (!Networking.IsOwner(gameObject)) return;

        if (!_isCasting)
        {
            isSecondTrigger = true;
        }
        else if (!isRewinding)
        {
            isRewinding = true;
            isSecondTrigger = false;
        }

        RequestSerialization();
    }

    public override void OnPickup()
    {
        //The pickup already transfers ownership, this makes sure the holder runs the rod.
        Networking.SetOwner(Networking.LocalPlayer, gameObject);
    }
}
