using UnityEngine;

public class TutorialManager : MonoBehaviour
{
    //Bools to see your progress through the night
    public bool hasLitFire = false;
    public bool hasEatenFood = false;
    public bool hasFixedRadio = false;

    //A refrence to the night settings
    [SerializeField] private SO_NightSettings nightSettings;

    //A refrence to the temperture manager
    [SerializeField] private GameObject tempertureManager;

    //A refrence to the hunger manager
    [SerializeField] private GameObject hungerManager;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (nightSettings != null)
        {
            if(nightSettings.DebugStartNight > 1)
            {
                Debug.Log("Tutorial deleted");
                Destroy(this.gameObject);
            }
            else
            {
                nightSettings.nightTimeMinutes = 0f;
                Destroy(tempertureManager);
                Destroy(hungerManager);
                Debug.Log("Tutorial started");
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
