using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class Food : MonoBehaviour
{
    [SerializeField] private float overrideFoodValue = -1;
    [SerializeField] private Mesh[] meshes;
    [SerializeField] private Material[] materials;
    [SerializeField] private bool destroyOnEaten = false;
    
    private XRGrabInteractable grabInteractable;
    private Rigidbody rb;
    private FMODUnity.StudioEventEmitter soundEmitter;
    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;
    private int value;

    private void Awake()
    {
        this.meshFilter = GetComponentInChildren<MeshFilter>();
        this.value = meshes.Length - 1;
        if (this.meshFilter != null && meshes.Length > 0)
            this.meshFilter.mesh = this.meshes[this.value];
        this.meshRenderer = GetComponentInChildren<MeshRenderer>();
        
        this.rb = GetComponent<Rigidbody>();
        this.grabInteractable = GetComponent<XRGrabInteractable>();
        this.soundEmitter = GetComponentInChildren<FMODUnity.StudioEventEmitter>();
    }

    [ContextMenu("Eat")]
    public void Eat()
    {
        this.value--;
        if (this.value < 0)
        {
            if (this.destroyOnEaten) Destroy(this.gameObject, 0.1f);
            return;
        }
        this.soundEmitter.Play();
        this.meshFilter.mesh = this.meshes[this.value];
        this.meshRenderer.material = this.materials[this.value];
    }
    
    public float FillValue => this.overrideFoodValue;
    
    public int Value => this.value;
    
    public Rigidbody Rigidbody => this.rb;
    
    public XRGrabInteractable GrabInteractable => this.grabInteractable;
}