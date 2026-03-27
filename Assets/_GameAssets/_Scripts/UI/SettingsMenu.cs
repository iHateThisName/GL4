using UnityEngine;

public class SettingsMenu : MonoBehaviour
{
    [SerializeField] private GameObject confirmationDialog;
    [SerializeField] private Setting[] settings;

    private XRSettings pendingSettings;
    private bool isDirty;

    private void OnEnable()
    {
        isDirty = false;
        pendingSettings = XRSettings.Default;

        foreach (var setting in settings)
        {
            if (setting != null)
                setting.Initialize(this);
        }

        if (confirmationDialog != null)
            confirmationDialog.SetActive(false);
    }

    public bool GetBool(SettingId id) => XRSettingsManager.Instance.GetBool(id);

    public float GetFloat(SettingId id) => XRSettingsManager.Instance.GetFloat(id);

    public void SetBool(SettingId id, bool value)
    {
        pendingSettings.SetBool(id, value);
        isDirty = true;
    }

    public void SetFloat(SettingId id, float value)
    {
        pendingSettings.SetFloat(id, value);
        isDirty = true;
    }

    public void RequestClose()
    {
        if (isDirty)
        {
            ShowConfirmationDialog();
        }
        else
        {
            Close();
        }
    }

    private void ShowConfirmationDialog()
    {
        if (confirmationDialog != null)
            confirmationDialog.SetActive(true);
    }

    public void OnSaveClicked()
    {
        XRSettingsManager.Instance.SaveSettings(pendingSettings);
        Close();
    }

    public void OnDiscardClicked()
    {
        Close();
    }

    private void Close()
    {
        if (confirmationDialog != null)
            confirmationDialog.SetActive(false);

        gameObject.SetActive(false);
    }
}
