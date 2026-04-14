using UnityEngine;

public class Food : MonoBehaviour
{
    [SerializeField] private float overrideFoodValue = -1;
    [SerializeField] private Mesh[] meshes;
    [SerializeField] private bool destroyOnEaten = false;
    
    private MeshFilter meshFilter;
    private int value;

    private void Awake()
    {
        this.meshFilter = GetComponentInChildren<MeshFilter>();
        this.value = meshes.Length;
    }

    public void Eat()
    {
        this.value--;
        this.meshFilter.mesh = this.meshes[this.value - 1];
        
        if (this.value == -1 && destroyOnEaten) 
            Destroy(this.gameObject, 0.1f);
    }
    
    public float FillValue => overrideFoodValue;
    
    public int Value => this.value;
}