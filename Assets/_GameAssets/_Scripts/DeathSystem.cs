using System;
using UnityEngine.SceneManagement;

public static class DeathSystem
{
    public static event Action OnPlayerDied;

    public static void KillPlayer(bool completelyRestart = false)
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        OnPlayerDied.Invoke();
    }
    
    public static void Clear() {}
}