using TMPro;
using UnityEngine;

public class HandWatch : MonoBehaviour
{
    [SerializeField] private HungerSystem hungerSystem;
    [SerializeField] private TextMeshProUGUI hungerText;

    private void OnEnable()
    {
        HungerSystem.OnHungerChanged += OnHungerChanged;
        HungerSystem.HungerStateChangedEvent += OnHungerStateChanged;
    }

    private void OnDisable()
    {
        HungerSystem.OnHungerChanged -= OnHungerChanged;
        HungerSystem.HungerStateChangedEvent -= OnHungerStateChanged;
    }

    private void OnHungerChanged(float hunger)
    {
        UpdateWatchUI();
    }
    
    private void OnHungerStateChanged(HungerSystem.EnumHungerState previous, HungerSystem.EnumHungerState current)
    {
        UpdateWatchUI();
    }
    
    private void UpdateWatchUI()
    {
        if (this.hungerText != null)
            this.hungerText.text = "Hunger: (" + this.hungerSystem.State + ") " + this.hungerSystem.Hunger.ToString("F1");
    }
}