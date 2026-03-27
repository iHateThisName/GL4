using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Lives in the loading screen scene. Subscribes to SceneTransition.OnProgress
/// and updates the progress bar with smooth animation and cycling loading text.
/// </summary>
public class LoadingScreenProgress : MonoBehaviour
{
    [SerializeField] private Slider progressBar;
    [SerializeField] private TextMeshProUGUI loadingText;
    [SerializeField] private float progressSpeed = 2f;
    [SerializeField] private float dotCycleSpeed = 3f;

    private static readonly string[] DotPatterns = { "", ".", "..", "...", "..", "." };

    private float targetProgress;
    private float displayedProgress;
    private float dotTimer;
    private int dotIndex;

    private void OnEnable()
    {
        SceneTransition.OnProgress += SetTargetProgress;
        displayedProgress = 0f;
        targetProgress = 0f;
        dotTimer = 0f;
        dotIndex = 0;
    }

    private void OnDisable()
    {
        SceneTransition.OnProgress -= SetTargetProgress;
    }

    private void FixedUpdate()
    {
        UpdateProgressBar();
        UpdateLoadingText();
    }

    private void SetTargetProgress(float progress)
    {
        targetProgress = progress;
    }

    private void UpdateProgressBar()
    {
        if (progressBar == null) return;

        displayedProgress = Mathf.MoveTowards(displayedProgress, targetProgress, progressSpeed * Time.unscaledDeltaTime);
        progressBar.value = displayedProgress;
    }

    private void UpdateLoadingText()
    {
        if (loadingText == null) return;

        dotTimer += Time.unscaledDeltaTime * dotCycleSpeed;
        if (dotTimer >= 1f)
        {
            dotTimer -= 1f;
            dotIndex = (dotIndex + 1) % DotPatterns.Length;
        }

        loadingText.text = $"Loading{DotPatterns[dotIndex]}";
    }
}