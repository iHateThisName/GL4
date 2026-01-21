using UnityEngine;

public class TestScript : UnityLifecycleObject
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public override void OnStart()
    {
        Debug.Log("TestScript Start");
    }

    // Update is called once per frame
    public override void OnUpdate()
    {
        Debug.Log("TestScript Update");
    }
}
