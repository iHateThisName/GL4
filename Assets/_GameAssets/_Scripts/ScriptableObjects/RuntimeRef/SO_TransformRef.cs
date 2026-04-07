using UnityEngine;

/// <summary>
/// Runtime reference to a Transform. Use for Player, Flashlight, or any tracked transform.
/// The owning component sets Value in Awake/Start, consumers read it.
/// </summary>
[CreateAssetMenu(menuName = "Runtime/Transform Reference")]
public class SO_TransformRef : SO_RuntimeRef<Transform> { }
