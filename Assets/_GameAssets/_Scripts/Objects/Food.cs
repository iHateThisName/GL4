using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class Food : MonoBehaviour
{
    [SerializeField] private float overrideFoodValue = -1;
    [SerializeField] private Mesh[] meshes;
    [SerializeField] private bool destroyOnEaten = false;
    
    private XRGrabInteractable grabInteractable;
    private Rigidbody rigidbody;
    private MeshFilter meshFilter;
    private int value;

    private void Awake()
    {
        this.meshFilter = GetComponentInChildren<MeshFilter>();
        this.value = meshes.Length - 1;
        if (this.meshFilter != null && meshes.Length > 0)
            this.meshFilter.mesh = this.meshes[this.value];
        
        this.rigidbody = GetComponent<Rigidbody>();
        this.grabInteractable = GetComponent<XRGrabInteractable>();
    }

    public void Eat()
    {
        this.value--;
        if (this.value < 0)
        {
            if (destroyOnEaten) Destroy(this.gameObject, 0.1f);
            return;
        }
        this.meshFilter.mesh = this.meshes[this.value];
    }
    
    public float FillValue => overrideFoodValue;
    
    public int Value => this.value;
    
    public Rigidbody Rigidbody => this.rigidbody;
    
    public XRGrabInteractable GrabInteractable => this.grabInteractable;
}