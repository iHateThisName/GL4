using UnityEngine;
using UnityEngine.UI;

public class BoolSetting : Setting
{
    [SerializeField] private Image checkbox;
    [SerializeField] private Sprite trueSprite;
    [SerializeField] private Sprite falseSprite;

    private bool value;

    private void OnMouseDown()
    {
        SetValue(!value);
    }

    public void SetValue(bool newValue)
    {
        value = newValue;
        menu?.SetBool(settingId, newValue);
        UpdateVisual();
    }

    private void UpdateVisual()
    {
        if (checkbox != null)
            checkbox.sprite = value ? trueSprite : falseSprite;
    }

    protected override void LoadValue()
    {
        if (menu == null) return;
        value = menu.GetBool(settingId);
        UpdateVisual();
    }
}
