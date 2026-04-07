using UnityEngine;

public abstract class Setting : MonoBehaviour
{
    [SerializeField] protected SettingId settingId;

    protected SettingsMenu menu;

    public void Initialize(SettingsMenu settingsMenu)
    {
        menu = settingsMenu;
        LoadValue();
    }

    protected abstract void LoadValue();
}
