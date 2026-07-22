using UdonSharp;
using UnityEngine;

//Changes season when enough fuel is consumed in the heater.
public class AsteroidSeasonChange : UdonSharpBehaviour
{
    public Renderer targetRenderer; //Reference to the renderer of the asteroid map.
    public Transform changeOrigin; //Origin point from which the season change starts.

    public float maxRadius = 60f; //Maximum radius of the asteroid map affected by the season change.
    public float changeSpeed = 5f; //Speed at which the season change spreads on the asteroid map.

    private Material _mat; //Material that contains before and after states of the asteroid map.
    private float _radius; //Current radius of the asteroid map affected by the change.
    private bool _changing; //Is the season changing?

    [SerializeField] private Animator _plantAnimator; //Animator of a plant that grows when the season is changing.

    void Start()
    {
        _mat = targetRenderer.material;
    }

    //Called by the heater to activate the season change. 
    public void StartChangeActivate()
    {
        SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "StartChange");
    }

    //Lets the season change script know that it can start the season change.
    public void StartChange()
    {
        _changing = true;
        _plantAnimator.SetTrigger("Grow");
    }

    void Update()
    {
        if (!_changing) return;

        _radius += Time.deltaTime * changeSpeed;
        _radius = Mathf.Min(_radius, maxRadius);

        _mat.SetVector("_Center", changeOrigin.position);
        _mat.SetFloat("_Radius", _radius);
    }
}