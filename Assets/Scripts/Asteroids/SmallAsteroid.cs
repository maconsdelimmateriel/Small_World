
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

//Script attached to the basic small asteroid.
public class SmallAsteroid : UdonSharpBehaviour
{
    [Header("Orbit Center")]
    public Transform orbitCenter;

    [Header("Orbit Shape")]
    [UdonSynced] public float semiMajorAxis = 5f;
    [UdonSynced] public float semiMinorAxis = 3f;

    [Header("Orbit Speed")]
    [UdonSynced] public float baseOrbitSpeed = 1f;
    [UdonSynced] public bool randomizeSpeed = true;
    [UdonSynced] public float minSpeed = 0.5f;
    [UdonSynced] public float maxSpeed = 2f;

    [Header("Orbit Inclination (Rotation in Degrees)")]
    [UdonSynced] public Vector3 orbitTiltEuler = new Vector3(0f, 0f, 0f); // X, Y, Z tilt

    [Header("Start Angle")]
    [UdonSynced] public float startAngle = 0f;
    [UdonSynced] public bool randomizeStartAngle = true;

    [UdonSynced] private float angle;
    [UdonSynced] private float orbitSpeed; // final speed used

    [UdonSynced] public bool isCaught = false;

    [UdonSynced] private Vector3 _position = new Vector3(0f, 0f, 0f);

    public override void OnPlayerJoined(VRCPlayerApi player)
    {
        if (Networking.IsOwner(gameObject))
        {
            RequestSerialization(); // Push latest state to late joiner
        }
    }

    public override void OnDeserialization()
    {
        transform.position = _position;
    }

    void OnEnable()
    {
        SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "GenerateOrbit");
    }

    void Update()
    {
        if (orbitCenter == null || isCaught) return;

        // Advance angle over time
        angle += orbitSpeed * Time.deltaTime;
        angle %= Mathf.PI * 2f;

        // Compute position on ellipse
        float x = semiMajorAxis * Mathf.Cos(angle);
        float z = semiMinorAxis * Mathf.Sin(angle);
        Vector3 localPos = new Vector3(x, 0f, z);

        // Apply 3D rotation (orbit inclination)
        Quaternion tiltRotation = Quaternion.Euler(orbitTiltEuler);
        Vector3 rotatedPos = tiltRotation * localPos;

        // Final world position
        transform.position = orbitCenter.position + rotatedPos;
        _position = transform.position;
    }

    public void GenerateOrbit()
    {
        angle = randomizeStartAngle ? Random.Range(0f, Mathf.PI * 2f) : startAngle;
        orbitSpeed = randomizeSpeed ? Random.Range(minSpeed, maxSpeed) : baseOrbitSpeed;
    }
}

