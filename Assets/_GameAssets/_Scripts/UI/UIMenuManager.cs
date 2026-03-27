using UnityEngine;

/// <summary>
/// Manages in-game UI menus (pause menu, settings, etc.).
/// Scene loading is handled by SceneTransition — this only manages menu panels.
/// </summary>
public class UIMenuManager : MonoBehaviour
{
    [Header("=== Menu References ===")]
    [SerializeField] private Transform[] uiMenus;
    [SerializeField] private Transform currentMenu;

    [Header("=== Scene Transition ===")]
    [SerializeField] private SO_ScreenFadeRef screenFadeRef;

    public void ShowMenu(Transform menu)
    {
        if (this.currentMenu != null)
            this.currentMenu.gameObject.SetActive(false);

        this.currentMenu = menu;

        if (this.currentMenu != null)
            this.currentMenu.gameObject.SetActive(true);
    }

    public void HideCurrentMenu()
    {
        if (this.currentMenu != null)
            this.currentMenu.gameObject.SetActive(false);

        this.currentMenu = null;
    }

    public void OpenSettingsMenu()
    {
        ShowMenu(uiMenus[1]);
    }

    public void QuitGame()
    {
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }
}
