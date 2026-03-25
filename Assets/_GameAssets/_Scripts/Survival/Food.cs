using UnityEngine;

public class Food : MonoBehaviour
{
    private GameObject[] states;
    private MeshFilter meshFilter;

    private void Awake()
    {
        this.meshFilter = GetComponent<MeshFilter>();
    }

    private void OnEaten()
    {
        this.meshFilter.mesh = this.states[1].GetComponent<MeshFilter>().mesh;
    }
}