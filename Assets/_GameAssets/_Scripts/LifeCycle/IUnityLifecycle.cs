using UnityEngine;

public class UnityLifecycleObject : MonoBehaviour
{
    public virtual void OnAwake() {}
    
    public virtual void OnEnable() {}
    
    public virtual void OnDisable() {}
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public virtual void OnStart() {}

    // Update is called once per frame
    public virtual void OnUpdate() {}

    public virtual void OnDestroy() {}
}
