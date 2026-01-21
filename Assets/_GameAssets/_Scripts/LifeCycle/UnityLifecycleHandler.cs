using UnityEngine;

public class UnityLifecycleHandler : MonoBehaviour
{
    [SerializeField]
    private UnityLifecycleObject[] lifecyclers;
    
    [SerializeField] private bool findAllAutomaticly;
        
    private void Awake()
    {
        if (!findAllAutomaticly) lifecyclers = FindObjectsByType<UnityLifecycleObject>(FindObjectsSortMode.None);
        foreach (var unitylife in lifecyclers)
        {
           unitylife.OnAwake(); 
        }
    }

    private void OnEnable()
    {
        foreach (var unitylife in lifecyclers)
        {
            unitylife.OnEnable(); 
        }
    }

    private void OnDisable()
    {
        foreach (var unitylife in lifecyclers)
        {
            unitylife.OnDisable(); 
        }
    }

    void Start()
    {
        foreach (var unitylife in lifecyclers)
        {
            unitylife.OnStart(); 
        }
    }
    
    void Update()
    {
        foreach (var unitylife in lifecyclers)
        {
            unitylife.OnUpdate(); 
        }
    }

    private void OnDestroy()
    {
        foreach (var unitylife in lifecyclers)
        {
            unitylife.OnDestroy(); 
        }
    }
}
