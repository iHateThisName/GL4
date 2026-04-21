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
    [SerializeField] private HungerSystem hungerManager;

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
                this.hungerManager.Pause();
                Debug.Log("Tutorial started");
            }
        }
    }

    private void OnEnable() => HungerSystem.OnHungerChanged += OnHungerChanged;
    
    private void OnDisable() => HungerSystem.OnHungerChanged -= OnHungerChanged;

    private void OnHungerChanged(float newHungerValue)
    {
        if (!this.hasLitFire) return;
        
        this.hasEatenFood = true;
        Debug.Log("eaten food");
    }

    [ContextMenu("Turn on fire")]
    public void TurnOnFire()
    {
        hungerManager.ModifyHunger(-21);
        hasLitFire = true;
    }

    public void FixRadio()
    {
        hasFixedRadio = true;
    }
}
