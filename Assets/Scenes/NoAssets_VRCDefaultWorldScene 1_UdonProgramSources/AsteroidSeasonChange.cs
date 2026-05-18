using UdonSharp;
using UnityEngine;

public class AsteroidSeasonChange : UdonSharpBehaviour
{
    public Renderer targetRenderer;
    public Transform thawOrigin;

    public float maxRadius = 50f;
    public float thawSpeed = 5f;

    private Material mat;
    private float radius;
    private bool thawing;

    void Start()
    {
        mat = targetRenderer.material;
    }

    public void StartThaw()
    {
        thawing = true;
    }

    void Update()
    {
        if (!thawing) return;

        radius += Time.deltaTime * thawSpeed;
        radius = Mathf.Min(radius, maxRadius);

        mat.SetVector("_Center", thawOrigin.position);
        mat.SetFloat("_Radius", radius);
    }
}