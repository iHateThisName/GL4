using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Static scene transition system. No MonoBehaviour, no singleton, no DontDestroyOnLoad.
/// Callers provide the target scene and an SO_ScreenFadeRef.
/// The SO_ScreenFadeRef acts as a mailbox — each scene's ScreenFade writes to it on Awake,
/// SceneTransition reads from it at each phase.
/// </summary>
public static class SceneTransition
{
    private static int LoadingScreenIndex => SceneManager.sceneCountInBuildSettings - 1;

    private static bool isTransitioning;

    // Transition timing
    private static float fadeInDuration = 2f;
    private static float fadeOutLoadingDuration = 0.5f;
    private static float holdAt100Duration = 0.5f;
    private static float minimumLoadingDisplayTime = 1f;
    private static int progressUpdateInterval = 3;
    private static ThreadPriority asyncLoadPriority = ThreadPriority.Low;

    // --- Public API ---

    public static void LoadScene(string sceneName, SO_ScreenFadeRef fadeRef)
    {
        LoadScene(sceneName, FadeConfig.FadeToBlack(fadeInDuration), fadeRef);
    }

    public static void LoadScene(string sceneName, FadeConfig fadeOutConfig, SO_ScreenFadeRef fadeRef)
    {
        if (isTransitioning) return;
        _ = TransitionAsync(sceneName, fadeOutConfig, fadeRef);
    }

    public static void LoadScene(int buildIndex, SO_ScreenFadeRef fadeRef)
    {
        LoadScene(buildIndex, FadeConfig.FadeToBlack(fadeInDuration), fadeRef);
    }

    public static void LoadScene(int buildIndex, FadeConfig fadeOutConfig, SO_ScreenFadeRef fadeRef)
    {
        if (isTransitioning) return;
        string scenePath = SceneUtility.GetScenePathByBuildIndex(buildIndex);
        string sceneName = System.IO.Path.GetFileNameWithoutExtension(scenePath);
        _ = TransitionAsync(sceneName, fadeOutConfig, fadeRef);
    }

    // --- Core transition ---

    private static async Awaitable TransitionAsync(string targetSceneName, FadeConfig fadeOutConfig, SO_ScreenFadeRef fadeRef)
    {
        isTransitioning = true;
        var ct = Application.exitCancellationToken;
        Scene sceneToUnload = SceneManager.GetActiveScene();

        // Fade-in always uses fadeInDuration (not tied to fade-out duration)
        var fadeInConfig = new FadeConfig(0f, fadeInDuration, fadeOutConfig.imageConfigs);

        // ===== PHASE 1: Fade out current scene =====
        var currentFade = fadeRef?.Value;
        Debug.Log($"[SceneTransition] Phase 1: Fade out. ScreenFade={currentFade != null}");
        if (currentFade != null)
            await currentFade.FadeAsync(fadeOutConfig, ct);

        // Screen is now fully opaque. Load loading screen ADDITIVELY behind the opaque overlay.
        // ===== PHASE 2: Load loading screen =====
        Debug.Log("[SceneTransition] Phase 2: Loading screen");
        int loadingIndex = LoadingScreenIndex;

        var loadingOp = SceneManager.LoadSceneAsync(loadingIndex, LoadSceneMode.Additive);
        loadingOp.priority = (int)asyncLoadPriority;
        await loadingOp;

        // The loading screen scene is now loaded. Its ScreenFade.Awake() ran and registered
        // in the fadeRef. Its Start() will run next frame and set alpha=0.
        // We must find and set the loading screen overlay opaque NOW, before Start() resets it.
        ScreenFade loadingFade = null;
        Slider progressBar = null;
        CacheLoadingScreenRefs(loadingIndex, ref loadingFade, ref progressBar);

        // Set opaque IMMEDIATELY — before the next frame where Start() would reset to 0.
        // This ensures seamless coverage: old scene overlay (opaque) + loading screen overlay (opaque).
        if (loadingFade != null)
            loadingFade.SetOpacityImmediate(1f);

        if (progressBar != null)
            progressBar.value = 0f;

        // Now safe to wait a frame — both overlays are opaque, no flash possible.
        await Awaitable.NextFrameAsync(ct);

        // Unload old scene (invisible behind loading screen's opaque overlay)
        if (sceneToUnload.IsValid() && sceneToUnload.isLoaded
            && sceneToUnload.buildIndex != loadingIndex)
        {
            Debug.Log($"[SceneTransition] Unloading: {sceneToUnload.name}");
            var unloadOp = SceneManager.UnloadSceneAsync(sceneToUnload);
            if (unloadOp != null)
            {
                unloadOp.priority = (int)asyncLoadPriority;
                await unloadOp;
            }
        }

        // Reveal loading screen (fade from opaque to transparent)
        if (loadingFade != null)
            await loadingFade.FadeAsync(new FadeConfig(0f, fadeOutLoadingDuration, null), ct);

        // ===== PHASE 3: Load target scene with progress =====
        Debug.Log($"[SceneTransition] Phase 3: Loading '{targetSceneName}'");
        float loadStartTime = Time.unscaledTime;

        var targetOp = SceneManager.LoadSceneAsync(targetSceneName, LoadSceneMode.Additive);
        if (targetOp == null)
        {
            Debug.LogError($"[SceneTransition] Failed to load '{targetSceneName}'. Is it in Build Settings?");
            isTransitioning = false;
            return;
        }
        targetOp.priority = (int)asyncLoadPriority;
        targetOp.allowSceneActivation = false;

        int frameCount = 0;
        while (targetOp.progress < 0.9f)
        {
            if (++frameCount >= progressUpdateInterval)
            {
                frameCount = 0;
                if (progressBar != null)
                    progressBar.value = targetOp.progress / 0.9f;
            }
            await Awaitable.NextFrameAsync(ct);
        }

        if (progressBar != null)
            progressBar.value = 1f;

        // Minimum display time
        float elapsed = Time.unscaledTime - loadStartTime;
        if (elapsed < minimumLoadingDisplayTime)
        {
            float remaining = minimumLoadingDisplayTime - elapsed;
            float waited = 0f;
            while (waited < remaining)
            {
                waited += Time.unscaledDeltaTime;
                await Awaitable.NextFrameAsync(ct);
            }
        }

        // Hold at 100%
        {
            float waited = 0f;
            while (waited < holdAt100Duration)
            {
                waited += Time.unscaledDeltaTime;
                await Awaitable.NextFrameAsync(ct);
            }
        }

        // ===== PHASE 4: Transition to new scene =====
        Debug.Log("[SceneTransition] Phase 4: Transitioning");

        // Fade loading screen overlay to opaque (hides loading screen content)
        if (loadingFade != null)
            await loadingFade.FadeAsync(new FadeConfig(1f, fadeOutLoadingDuration, null), ct);

        // Activate target scene (Awake/Start run behind loading screen overlay)
        targetOp.allowSceneActivation = true;
        await targetOp;

        Scene targetScene = SceneManager.GetSceneByName(targetSceneName);
        if (targetScene.IsValid())
            SceneManager.SetActiveScene(targetScene);

        // Wait for new scene's Start() to run (timers, UI setup, etc.)
        await Awaitable.NextFrameAsync(ct);
        await Awaitable.NextFrameAsync(ct);

        // New scene's ScreenFade registered in fadeRef during Awake.
        // Set it opaque with matching color BEFORE unloading loading screen.
        var newFade = fadeRef?.Value;
        if (newFade != null)
        {
            if (fadeInConfig.imageConfigs != null && fadeInConfig.imageConfigs.Length > 0)
                newFade.StartFadeWithConfig(new FadeConfig(1f, 0f, fadeInConfig.imageConfigs));
            newFade.SetOpacityImmediate(1f);
        }

        // Unload loading screen — new scene's opaque overlay has taken over
        var unloadLoadingOp = SceneManager.UnloadSceneAsync(loadingIndex);
        if (unloadLoadingOp != null)
        {
            unloadLoadingOp.priority = (int)asyncLoadPriority;
            await unloadLoadingOp;
        }

        // ===== PHASE 5: Fade into new scene =====
        // All Start() methods have run. Scene is covered by opaque overlay.
        Debug.Log("[SceneTransition] Phase 5: Fade in");
        if (newFade != null)
            await newFade.FadeAsync(fadeInConfig, ct);

        isTransitioning = false;
        Debug.Log("[SceneTransition] Complete");
    }

    private static void CacheLoadingScreenRefs(int loadingIndex, ref ScreenFade fade, ref Slider progressBar)
    {
        Scene loadingScene = SceneManager.GetSceneByBuildIndex(loadingIndex);
        if (!loadingScene.IsValid()) return;

        GameObject[] roots = loadingScene.GetRootGameObjects();
        for (int i = 0; i < roots.Length; i++)
        {
            if (progressBar == null)
                progressBar = roots[i].GetComponentInChildren<Slider>(true);
            if (fade == null)
                fade = roots[i].GetComponentInChildren<ScreenFade>(true);
            if (progressBar != null && fade != null)
                break;
        }
    }
}
