
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
    public float semiMajorAxis = 5f;
    public float semiMinorAxis = 3f;

    [Header("Orbit Speed")]
    public float baseOrbitSpeed = 1f;
    public bool randomizeSpeed = true;
    public float minSpeed = 0.5f;
    public float maxSpeed = 2f;

    [Header("Orbit Inclination (Rotation in Degrees)")]
    public Vector3 orbitTiltEuler = new Vector3(0f, 0f, 0f); // X, Y, Z tilt

    [Header("Start Angle")]
    public float startAngle = 0f;
    public bool randomizeStartAngle = true;

    private float angle;
    private float orbitSpeed; // final speed used

    public bool isCaught = false;

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
    }

    public void GenerateOrbit()
    {
        angle = randomizeStartAngle ? Random.Range(0f, Mathf.PI * 2f) : startAngle;
        orbitSpeed = randomizeSpeed ? Random.Range(minSpeed, maxSpeed) : baseOrbitSpeed;
    }
}

