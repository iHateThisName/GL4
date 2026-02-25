using UnityEngine;

public class GameOverManager : MonoBehaviour
{
    [SerializeField] private float sceneDuration = 5f;
    
    private Timer reloadSceneTimer;

    private void Start()
    {
        this.reloadSceneTimer = new Timer(0, this.sceneDuration);
        this.reloadSceneTimer.OnTimerFinished += ReloadGameScene;
        this.reloadSceneTimer.Start();
    }
    
    /// <summary>
    /// Clean up the timer when this component is destroyed.
    /// </summary>
    private void OnDestroy()
    {
        if (this.reloadSceneTimer != null)
        {
            this.reloadSceneTimer.Dispose();
            this.reloadSceneTimer = null;
        }
    }

    public void ReloadGameScene()
    {
        this.reloadSceneTimer.Dispose();
        GameManager.Instance.ContinueGame();
    }
}