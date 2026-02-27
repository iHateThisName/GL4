using UnityEngine;

public class DeathFade : MonoBehaviour
{
    [Header("Death UI Settings")]
    [Tooltip("Assign the Canvas Group from your black fade Canvas.")]
    [SerializeField] private CanvasGroup fadeCanvasGroup;

    [Tooltip("How long it takes to fade to black before restarting.")]
    [SerializeField] private float fadeDuration = 2.0f;
    
    [Tooltip("How long it should wait at the full fade")]
    [SerializeField] private float fadeWaitDuration = 0.5f;

    private Timer fadeTimer;

    public System.Action OnFadeFinished;

    private void Start()
    {
        if (this.fadeCanvasGroup == null) this.fadeCanvasGroup = GetComponent<CanvasGroup>();
        if (this.fadeCanvasGroup != null)
        {
            this.fadeCanvasGroup.alpha = 0f;
            this.fadeCanvasGroup.blocksRaycasts = false;

            // Ensure the canvas is turned off at the start of the scene
            this.fadeCanvasGroup.gameObject.SetActive(false);
        }
    }
    
    private void OnEnable()
    {
        DeathSystem.OnPlayerDied += OnPlayerDeath;
    }

    private void OnDisable()
    {
        DeathSystem.OnPlayerDied -= OnPlayerDeath;
    }

    private void OnDestroy()
    {
        if (this.fadeTimer == null) return;
        this.fadeTimer.OnTimerTick -= TickFade;
        this.fadeTimer.OnTimerFinished -= FinishFade;
        this.fadeTimer.Dispose();
    }

    public void OnPlayerDeath()
    {
        if (this.fadeCanvasGroup == null) return;
        this.fadeCanvasGroup.gameObject.SetActive(true);
        
        this.fadeTimer = new Timer(0.1f, this.fadeDuration + this.fadeWaitDuration);
        this.fadeTimer.OnTimerTick += TickFade;
        this.fadeTimer.OnTimerFinished += FinishFade;
        this.fadeTimer.Start();
    }

    private void TickFade()
    {
        if (this.fadeCanvasGroup == null && !this.fadeCanvasGroup.gameObject.activeInHierarchy) return;
        
        this.fadeCanvasGroup.alpha = Mathf.Clamp01(this.fadeTimer.Elapsed / this.fadeDuration);
    }

    private void FinishFade()
    {
        this.OnFadeFinished?.Invoke();
        DeathSystem.deathEvent.LoadScene();
    }
}