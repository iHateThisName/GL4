using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DeathManager : MonoBehaviour
{
    [Header("Death UI Settings")]
    [Tooltip("Assign the Canvas Group from your black fade Canvas.")]
    [SerializeField] private CanvasGroup fadeCanvasGroup;

    [Tooltip("How long it takes to fade to black before restarting.")]
    [SerializeField] private float fadeDuration = 2.0f;

    private void Start()
    {
        if (this.fadeCanvasGroup != null)
        {
            this.fadeCanvasGroup.alpha = 0f;
            this.fadeCanvasGroup.blocksRaycasts = false;

            // Ensure the canvas is turned off at the start of the scene
            this.fadeCanvasGroup.gameObject.SetActive(false);
        }
    }

    // Call the DeathSequence
    public void TriggerDeathSequence()
    {
        StartCoroutine(this.FadeAndRestart());
    }

    private IEnumerator FadeAndRestart()
    {
        float elapsedTime = 0f;

        if (this.fadeCanvasGroup != null)
        {
            //Turn the Canvas back on so we can see the fade
            this.fadeCanvasGroup.gameObject.SetActive(true);

            while (elapsedTime < this.fadeDuration)
            {
                elapsedTime += Time.deltaTime;
                this.fadeCanvasGroup.alpha = Mathf.Clamp01(elapsedTime / this.fadeDuration);
                yield return null;
            }
        }
        else
        {
            Debug.LogWarning("DeathManager: No CanvasGroup assigned! Skipping fade.");
            yield return new WaitForSeconds(this.fadeDuration);
        }

        // Wait half a second in the pitch black
        yield return new WaitForSeconds(0.5f);

        // Reload the current scene
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}