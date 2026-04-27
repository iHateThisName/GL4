using UnityEngine;
using TMPro;

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

    //A refrence to the radio
    [SerializeField] private Radio radio;

    //A refrence to the tutorial UI text
    [SerializeField] private TMP_Text tutorialText;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (nightSettings != null)
        {
            if (nightSettings.DebugStartNight > 1)
            {
                Debug.Log("Tutorial deleted");
                tutorialText.text = "";
                Destroy(this.gameObject);
            }
            else
            {
                tutorialText.text = "Chop wood and turn on fire";
                //Destroy(tempertureManager);
                //this.hungerManager.Pause();
                Debug.Log("Tutorial started");
            }
        }
    }

    private void OnEnable() => HungerSystem.OnHungerChanged += OnHungerChanged;
    
    private void OnDisable() => HungerSystem.OnHungerChanged -= OnHungerChanged;

    private void OnHungerChanged(float newHungerValue)
    {
        if (!this.hasLitFire || this.hasEatenFood) return;
        
        this.hasEatenFood = true;
        radio.SetChannel(8);
        tutorialText.text = "Put the radio frequency back to Channel 30";
        Debug.Log("eaten food");
    }

    [ContextMenu("Turn on fire")]
    public void TurnOnFire()
    {
        if(hasLitFire)
        {
            return;
        }
        tutorialText.text = "Eat a can of food";
        hungerManager.ModifyHunger(-21);
        hasLitFire = true;
    }

    public void FixRadio()
    {
        if(!hasLitFire || !hasEatenFood || hasFixedRadio)
        {
            Debug.Log("Tutorial stopped 2");
            return;
        }
        hasFixedRadio = true;
        tutorialText.text = "Survive the night";
    }
}
