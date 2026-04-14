using System.Threading;
using UnityEngine;
using UnityEngine.SceneManagement;

public class XRSceneSwitch : MonoBehaviour
{
    private Transform xrOrigin;
    private CancellationTokenSource xrControlCtx; 
    
    private void Awake()
    {
        this.xrOrigin = this.transform.root;
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    
    private void OnSceneLoaded(Scene arg0, LoadSceneMode arg1)
    {
        this.xrControlCtx = new CancellationTokenSource();
        ReEnableXr(this.xrControlCtx.Token);
    }

    private async Awaitable ReEnableXr(CancellationToken ct)
    {
        this.xrOrigin.gameObject.SetActive(false);
        await Awaitable.WaitForSecondsAsync(0.5f, ct);
        this.xrOrigin.gameObject.SetActive(true);
    }
}
