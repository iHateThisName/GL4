using UnityEngine;

/// <summary>
/// Manages in-game UI menus (pause menu, settings, etc.).
/// Scene loading is handled by SceneTransition — this only manages menu panels.
/// </summary>
public class UIMenuManager : MonoBehaviour
{
    [Header("=== Menu References ===")]
    [SerializeField] private Transform uiMenus;
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

    /// <summary>
    /// Called by UI buttons to load a scene by build index through SceneTransition.
    /// </summary>
    public void LoadScene(int buildIndex)
    {
        SceneTransition.LoadScene(buildIndex, this.screenFadeRef);
    }

    /// <summary>
    /// Called by UI buttons to load a scene by name through SceneTransition.
    /// </summary>
    public void LoadScene(string sceneName)
    {
        SceneTransition.LoadScene(sceneName, this.screenFadeRef);
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
