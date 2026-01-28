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
        hunger = maxHunger;
        starving = false;
        this.UpdateHungerText(hungerText,hunger.ToString("F2"));
    }

    private void OnEnable()
    {
        if (mouthCollider == null) return;
        mouthCollider.OnTriggerEntered += this.eatFood;
    }

    private void OnDisable()
    {
        if (mouthCollider == null) return;
        mouthCollider.OnTriggerEntered -= this.eatFood;
    }

    private void eatFood(Collider other)
    {
        Debug.Log("Trigger Enter with: " + other.name);
        if (!other.TryGetComponent<Food>(out var food)) return;
        
        hunger = Mathf.Max(hunger + food.GetFoodValue(), 0);
        Destroy(food.gameObject, 0.1f);
    }

    private void Update()
    {
        elapsedTime += Time.deltaTime;
        if (elapsedTime >= hungerDecayTick)
        {
            elapsedTime = 0;
            hunger = Mathf.Clamp(hunger - hungerDecayRate, 0, 100);
            this.UpdateHungerText(hungerText,hunger.ToString("F2"));
            if (hunger <= hungerThreshold)
            {
                starving = true;
            }
        }
    }
    
    private void UpdateHungerText(TextMeshProUGUI textField, string newText)
    {
        textField.text = newText;
    }
}
