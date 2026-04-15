using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    //A function that starts the scene. It is a work in progress, since it does not use the load screen that we want between scenes
    public void StartGame()
    {
        SceneManager.LoadScene("CabinLayoutFinal");
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