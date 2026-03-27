using UnityEngine;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private Button startButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button quitButton;
    [SerializeField] private UIMenuManager UIMenuManager;
    
    private void OnEnable()
    {
        if (startButton != null)
            startButton.onClick.AddListener(OnStartClicked);
        if (settingsButton != null)
            settingsButton.onClick.AddListener(OnSettingsClicked);
        if (quitButton != null)
            quitButton.onClick.AddListener(OnQuitClicked);
    }

    private void OnDisable()
    {
        if (startButton != null)
            startButton.onClick.RemoveListener(OnStartClicked);
        if (settingsButton != null)
            settingsButton.onClick.RemoveListener(OnSettingsClicked);
        if (quitButton != null)
            quitButton.onClick.RemoveListener(OnQuitClicked);
    }

    private void OnStartClicked()
    {
        GameManager.Instance.ContinueGame();
    }

    private void OnSettingsClicked()
    {
        this.UIMenuManager.OpenSettingsMenu();
    }
    
    private void OnQuitClicked()
    {
        Application.Quit();
    }
}