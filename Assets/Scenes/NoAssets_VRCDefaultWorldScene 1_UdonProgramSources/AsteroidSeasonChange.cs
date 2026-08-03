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

    [SerializeField] private Animator[] _plantAnimators; //Animators of the plants that grow when the season is changing.
    private int _plantIndex = 0; //Keeps track of the number of plants that grew;
    [SerializeField] private int _growingInterval = 1; //Interval between launching the growing of another plant.
    private float _growingTimer = 0f; //Timer until next plant growing.

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
    }

    //Make a single plant grow.
    public void GrowPlant()
    {
        if (_plantIndex <= _plantAnimators.Length)
        {
            _plantAnimators[_plantIndex].SetTrigger("Grow");
            _plantIndex++;
            Debug.Log(_plantIndex + "plant");
        }
    }

    void Update()
    {
        if (!_changing) return;

        _radius += Time.deltaTime * changeSpeed;
        _radius = Mathf.Min(_radius, maxRadius);

        _mat.SetVector("_Center", changeOrigin.position);
        _mat.SetFloat("_Radius", _radius);

        _growingTimer += Time.deltaTime;
        if(_growingTimer >= _growingInterval)
        {
            GrowPlant();
            _growingTimer = 0f;
        }
    }
}