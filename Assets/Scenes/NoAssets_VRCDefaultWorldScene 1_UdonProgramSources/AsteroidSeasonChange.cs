using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class AsteroidSeasonChange : UdonSharpBehaviour
{
    public Renderer targetRenderer;
    public Transform thawOrigin;

    public float maxRadius = 50f;
    public float thawSpeed = 5f;

    private float currentRadius = 0f;
    private bool thawing = false;

    private Material materialInstance;

    void Start()
    {
        materialInstance = targetRenderer.material;
    }

    public void StartThaw()
    {
        thawing = true;
    }

    void Update()
    {
        if (!thawing) return;

        currentRadius += Time.deltaTime * thawSpeed;
        currentRadius = Mathf.Min(currentRadius, maxRadius);

        materialInstance.SetVector("_Center", thawOrigin.position);
        materialInstance.SetFloat("_Radius", currentRadius);
    }
}
