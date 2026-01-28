using TMPro;
using UnityEngine;

public class HungerSystem : MonoBehaviour
{
    [SerializeField] private float hunger;
    [SerializeField] private TriggerArea mouthCollider;
    [SerializeField] private float maxHunger = 100f;
    [SerializeField] private float hungerThreshold = 0.5f;
    [SerializeField] private float hungerDecayRate = 0.01f;
    [SerializeField] private float hungerDecayTick = 0.25f;
    [SerializeField] private TextMeshProUGUI hungerText;

    private float elapsedTime;
    private bool starving;

    private void Start()
    {
        this.hunger = this.maxHunger;
        this.starving = false;
        this.UpdateHungerText(this.hungerText,this.hunger.ToString("F2"));
    }

    private void OnEnable()
    {
        if (this.mouthCollider == null) return;
        this.mouthCollider.OnTriggerEntered += this.eatFood;
    }

    private void OnDisable()
    {
        if (this.mouthCollider == null) return;
        this.mouthCollider.OnTriggerEntered -= this.eatFood;
    }

    private void eatFood(Collider other)
    {
        Debug.Log("Trigger Enter with: " + other.name);
        if (!other.TryGetComponent<Food>(out var food)) return;
        
        this.hunger = Mathf.Max(this.hunger + food.GetFoodValue(), 0);
        Destroy(food.gameObject, 0.1f);
    }

    private void Update()
    {
        this.elapsedTime += Time.deltaTime;
        if (this.elapsedTime >= this.hungerDecayTick)
        {
            this.elapsedTime = 0;
            this.hunger = Mathf.Clamp(this.hunger - this.hungerDecayRate, 0, 100);
            this.UpdateHungerText(this.hungerText,this.hunger.ToString("F2"));
            if (this.hunger <= this.hungerThreshold)
            {
                this.starving = true;
            }
        }
    }
    
    private void UpdateHungerText(TextMeshProUGUI textField, string newText)
    {
        textField.text = newText;
    }
}
