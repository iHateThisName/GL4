using UnityEngine;

public class MainMenu : MonoBehaviour
{
    [ContextMenu("Start Game")]
    public void StartGame()
    {
        GameManager.Instance.LoadScene("CabinLayoutFinal");
        //SceneManager.LoadScene("CabinLayoutFinal");
    }

    public void BackToMenu()
    {
        GameManager.Instance.LoadScene("MainMenu");
        //SceneManager.LoadScene("CabinLayoutFinal");
    }

    public void GoToCredits()
    {
        GameManager.Instance.LoadScene("CreditsScene");
        //SceneManager.LoadScene("CabinLayoutFinal");
    }

    //A function for quitting the editor or the game
    public void QuitGame()
    {
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }
}