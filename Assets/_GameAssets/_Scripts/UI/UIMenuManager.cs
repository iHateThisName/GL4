using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIMenuManager : MonoBehaviour
{
    [Header("=== Menu References ===")]
    [SerializeField] private Transform uiMenus;
    [SerializeField] private Transform currentMenu;

    [Header("=== Scene Loading ===")]
    [SerializeField] private int sceneToLoadIndex;
    [SerializeField] private string sceneToLoadName;
    [SerializeField] private int loadingScreenSceneIndex;

    [Header("=== Loading Screen References ===")]
    [Tooltip("Assign directly to avoid runtime searching")]
    [SerializeField] private ScreenFade loadingScreenFade;
    [SerializeField] private Slider loadingProgressBar;

    [Header("=== Fade Settings ===")]
    [SerializeField] private float fadeDuration = 0.5f;

    [Header("=== VR Optimization ===")]
    [Tooltip("Update progress bar every N frames to reduce overhead")]
    [SerializeField] private int progressUpdateInterval = 3;
    [Tooltip("Lower priority = less frame impact during load")]
    [SerializeField] private ThreadPriority asyncLoadPriority = ThreadPriority.Low;

    private int currentSceneIndex;
    private ScreenFade currentSceneFade;
    private bool isLoading;
    private bool fadeComplete;

    // Cached to avoid allocations
    private WaitForEndOfFrame waitForEndOfFrame;
    private FadeConfig fadeInConfig;
    private FadeConfig fadeOutConfig;

    private void Awake()
    {
        this.currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        DontDestroyOnLoad(gameObject);
        this.loadingScreenSceneIndex = SceneManager.sceneCountInBuildSettings - 1;

        // Cache wait objects and configs to avoid GC allocations
        this.waitForEndOfFrame = new WaitForEndOfFrame();
        this.fadeInConfig = new FadeConfig(0f, this.fadeDuration, null);
        this.fadeOutConfig = new FadeConfig(1f, this.fadeDuration, null);

        // Try to find ScreenFade in current scene
        this.currentSceneFade = FindObjectOfType<ScreenFade>();
    }

    [ContextMenu("Load Scene")]
    private void LoadSceneContextMenu()
    {
        LoadScene(this.sceneToLoadIndex);
    }

    public void LoadScene(int sceneToLoad)
    {
        if (this.isLoading) return;
        StartCoroutine(LoadSceneRoutine(sceneToLoad));
    }

    public void LoadScene(string sceneName)
    {
        if (this.isLoading) return;
        int sceneIndex = SceneUtility.GetBuildIndexByScenePath(sceneName);
        if (sceneIndex >= 0)
        {
            StartCoroutine(LoadSceneRoutine(sceneIndex));
        }
    }

    private IEnumerator LoadSceneRoutine(int targetSceneIndex)
    {
        this.isLoading = true;

        // Step 1: Fade out current scene
        if (this.currentSceneFade != null)
        {
            yield return FadeAndWait(this.currentSceneFade, this.fadeOutConfig);
        }

        // Step 2: Load loading screen additively with low priority
        AsyncOperation loadingScreenOp = SceneManager.LoadSceneAsync(this.loadingScreenSceneIndex, LoadSceneMode.Additive);
        loadingScreenOp.priority = (int)this.asyncLoadPriority;
        yield return loadingScreenOp;

        // Allow a frame for scene to initialize
        yield return this.waitForEndOfFrame;

        // Step 3: Cache loading screen references if not assigned
        if (this.loadingScreenFade == null || this.loadingProgressBar == null)
        {
            CacheLoadingScreenReferences();
        }

        if (this.loadingProgressBar != null)
        {
            this.loadingProgressBar.value = 0f;
        }

        // Step 4: Unload current scene
        if (this.currentSceneIndex != this.loadingScreenSceneIndex)
        {
            AsyncOperation unloadOp = SceneManager.UnloadSceneAsync(this.currentSceneIndex);
            unloadOp.priority = (int)this.asyncLoadPriority;
            yield return unloadOp;
        }

        // Step 5: Fade in loading screen
        if (this.loadingScreenFade != null)
        {
            this.loadingScreenFade.SetOpacityImmediate(1f);
            yield return FadeAndWait(this.loadingScreenFade, this.fadeInConfig);
        }

        // Step 6: Load target scene with progress tracking
        AsyncOperation targetLoadOp = SceneManager.LoadSceneAsync(targetSceneIndex, LoadSceneMode.Additive);
        targetLoadOp.priority = (int)this.asyncLoadPriority;
        targetLoadOp.allowSceneActivation = false;

        // Step 7: Update progress bar at intervals (VR-friendly)
        int frameCount = 0;
        while (targetLoadOp.progress < 0.9f)
        {
            if (++frameCount >= this.progressUpdateInterval)
            {
                frameCount = 0;
                if (this.loadingProgressBar != null)
                {
                    this.loadingProgressBar.value = targetLoadOp.progress / 0.9f;
                }
            }
            yield return this.waitForEndOfFrame;
        }

        // Fill progress bar
        if (this.loadingProgressBar != null)
        {
            this.loadingProgressBar.value = 1f;
        }

        // Brief pause at 100%
        yield return this.waitForEndOfFrame;
        yield return this.waitForEndOfFrame;

        // Step 8: Fade out loading screen
        if (this.loadingScreenFade != null)
        {
            yield return FadeAndWait(this.loadingScreenFade, this.fadeOutConfig);
        }

        // Step 9: Activate target scene
        targetLoadOp.allowSceneActivation = true;
        yield return targetLoadOp;

        // Step 10: Set active scene and find its ScreenFade
        Scene targetScene = SceneManager.GetSceneByBuildIndex(targetSceneIndex);
        SceneManager.SetActiveScene(targetScene);
        CacheSceneFade(targetScene);

        // Step 11: Unload loading screen
        AsyncOperation unloadLoadingOp = SceneManager.UnloadSceneAsync(this.loadingScreenSceneIndex);
        unloadLoadingOp.priority = (int)this.asyncLoadPriority;
        yield return unloadLoadingOp;

        // Step 12: Fade in new scene
        if (this.currentSceneFade != null)
        {
            this.currentSceneFade.SetOpacityImmediate(1f);
            yield return FadeAndWait(this.currentSceneFade, this.fadeInConfig);
        }

        this.currentSceneIndex = targetSceneIndex;
        this.isLoading = false;
    }

    private void CacheLoadingScreenReferences()
    {
        Scene loadingScene = SceneManager.GetSceneByBuildIndex(this.loadingScreenSceneIndex);
        GameObject[] roots = loadingScene.GetRootGameObjects();

        for (int i = 0; i < roots.Length; i++)
        {
            if (this.loadingProgressBar == null)
            {
                this.loadingProgressBar = roots[i].GetComponentInChildren<Slider>(true);
            }
            if (this.loadingScreenFade == null)
            {
                this.loadingScreenFade = roots[i].GetComponentInChildren<ScreenFade>(true);
            }
            if (this.loadingProgressBar != null && this.loadingScreenFade != null)
            {
                break;
            }
        }
    }

    private void CacheSceneFade(Scene scene)
    {
        this.currentSceneFade = null;
        GameObject[] roots = scene.GetRootGameObjects();

        for (int i = 0; i < roots.Length; i++)
        {
            this.currentSceneFade = roots[i].GetComponentInChildren<ScreenFade>(true);
            if (this.currentSceneFade != null)
            {
                break;
            }
        }
    }

    private IEnumerator FadeAndWait(ScreenFade fade, FadeConfig config)
    {
        this.fadeComplete = false;
        fade.OnFadeComplete += OnFadeComplete;
        fade.StartFadeWithConfig(config);

        while (!this.fadeComplete)
        {
            yield return this.waitForEndOfFrame;
        }

        fade.OnFadeComplete -= OnFadeComplete;
    }

    private void OnFadeComplete()
    {
        this.fadeComplete = true;
    }
}
