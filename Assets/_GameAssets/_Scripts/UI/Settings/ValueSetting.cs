using UnityEngine;
using UnityEngine.UI;

public class ValueSetting : Setting
{
    [SerializeField] private Slider slider;

    private float value;
    private bool isUpdatingSlider;

    private void OnEnable()
    {
        if (slider != null)
            slider.onValueChanged.AddListener(OnSliderChanged);
    }

    private void OnDisable()
    {
        if (slider != null)
            slider.onValueChanged.RemoveListener(OnSliderChanged);
    }

    private void OnSliderChanged(float newValue)
    {
        if (!isUpdatingSlider)
            SetValue(newValue);
    }

    public void SetValue(float newValue)
    {
        value = newValue;
        menu?.SetFloat(settingId, newValue);
        UpdateVisual();
    }

    private void UpdateVisual()
    {
        if (slider != null)
        {
            isUpdatingSlider = true;
            slider.value = value;
            isUpdatingSlider = false;
        }
    }

    protected override void LoadValue()
    {
        if (menu == null) return;
        value = menu.GetFloat(settingId);
        UpdateVisual();
    }
}
