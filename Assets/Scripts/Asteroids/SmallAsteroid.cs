using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class SmallAsteroid : UdonSharpBehaviour
{
    [Header("Orbit Center")]
    public Transform orbitCenter;

    [Header("Orbit Shape")]
    [UdonSynced] public float semiMajorAxis = 5f;
    [UdonSynced] public float semiMinorAxis = 3f;

    [Header("Orbit Speed")]
    [UdonSynced] public float orbitSpeed = 1f;

    [Header("Orbit Inclination (Rotation in Degrees)")]
    [UdonSynced] public Vector3 orbitTiltEuler = Vector3.zero;

    [Header("Start Angle")]
    [UdonSynced] public float startAngle = 0f;

    [UdonSynced] public bool isCaught = false;
    [UdonSynced] public bool isConsumed = false; //Has the asteroid been reeled in and turned into fuel?

    private Transform _originalParent; //Parent before being stuck to a hook.

    // Sync orbit start time (ms since world start)
    [UdonSynced] private double spawnServerTime;

    void Start()
    {
        _originalParent = transform.parent;
        ApplyConsumed();
    }

    void OnEnable()
    {
        if (Networking.IsOwner(gameObject))
        {
            // Record the time this asteroid was activated
            spawnServerTime = Networking.GetServerTimeInMilliseconds();
            RequestSerialization();
        }
    }

    void Update()
    {
        if (orbitCenter == null || isCaught || isConsumed) return;

        // Calculate elapsed time since activation (in seconds)
        double currentTime = Networking.GetServerTimeInMilliseconds();
        double elapsed = (currentTime - spawnServerTime) / 1000.0;

        // Angle progresses deterministically based on server time
        float angle = startAngle + (float)(orbitSpeed * elapsed);

        // Orbit position on ellipse
        float x = semiMajorAxis * Mathf.Cos(angle);
        float z = semiMinorAxis * Mathf.Sin(angle);
        Vector3 localPos = new Vector3(x, 0f, z);

        // Apply tilt
        Quaternion tiltRotation = Quaternion.Euler(orbitTiltEuler);
        Vector3 rotatedPos = tiltRotation * localPos;

        // Final position
        transform.position = orbitCenter.position + rotatedPos;
    }

    public void GenerateOrbit()
    {
        if (!Networking.IsOwner(gameObject)) return;

        orbitSpeed = Random.Range(0.01f, 0.2f);
        startAngle = Random.Range(0f, Mathf.PI * 2f);
        orbitTiltEuler = new Vector3(
            Random.Range(-30f, 30f),
            Random.Range(0f, 360f),
            0f
        );

        // Update spawn time whenever we regenerate orbit
        spawnServerTime = Networking.GetServerTimeInMilliseconds();
        RequestSerialization();
    }

    public override void OnDeserialization()
    {
        // Late joiners now get:
        // - orbitSpeed
        // - startAngle
        // - orbitTiltEuler
        // - spawnServerTime
        // And calculate the correct orbit instantly
        ApplyConsumed();
    }

    //Called by the rod owner once the asteroid is reeled in. Hidden instead of destroyed, so late joiners and the manager stay in sync.
    public void Consume()
    {
        Networking.SetOwner(Networking.LocalPlayer, gameObject);
        isConsumed = true;
        ApplyConsumed();
        RequestSerialization();
    }

    private void ApplyConsumed()
    {
        if (!isConsumed) return;

        transform.SetParent(_originalParent);

        Renderer[] renderers = GetComponentsInChildren<Renderer>(true);
        foreach (Renderer r in renderers)
        {
            r.enabled = false;
        }

        SphereCollider col = GetComponent<SphereCollider>();
        if (col != null) col.enabled = false;
    }
}
