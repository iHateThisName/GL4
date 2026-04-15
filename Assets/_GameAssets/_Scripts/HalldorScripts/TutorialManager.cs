using UnityEngine;

public class TutorialManager : MonoBehaviour
{
    //A refrence to the night settings
    [SerializeField] private SO_NightSettings nightSettings;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (nightSettings != null)
        {
            if(nightSettings.DebugStartNight > 1)
            {

            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
