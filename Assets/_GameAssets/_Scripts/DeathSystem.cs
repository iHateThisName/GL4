using System;
using UnityEngine.SceneManagement;

public static class DeathSystem
{
    public static event Action OnPlayerDied;

    public static void KillPlayer(bool completelyRestart = false)
    {
#if !UNITY_EDITOR
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        OnPlayerDied.Invoke();
#endif
    }
    
    public static void Clear() {}
}