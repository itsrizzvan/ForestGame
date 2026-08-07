using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(CanvasGroup))]
public class ScreenFader : MonoBehaviour
{
    public static ScreenFader Instance { get; private set; }

    [Header("Fade Timings")]
    [SerializeField] private float defaultFadeDuration = 0.8f;

    private CanvasGroup canvasGroup;

    private void Awake()
    {
        // Singleton pattern to access from anywhere
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        canvasGroup = GetComponent<CanvasGroup>();
    }

    private void Start()
    {
        // Automatically fade in when the scene starts
        StartCoroutine(FadeInRoutine(defaultFadeDuration));
    }

    /// <summary>
    /// Fades the screen to black, reloads/loads the target scene, and fades back in.
    /// </summary>
    public void FadeToScene(string sceneName, float duration = -1f)
    {
        float fadeTime = duration > 0 ? duration : defaultFadeDuration;
        StartCoroutine(FadeAndLoadRoutine(sceneName, fadeTime));
    }

    private IEnumerator FadeAndLoadRoutine(string sceneName, float duration)
    {
        // Block player interaction during fade
        canvasGroup.blocksRaycasts = true;

        // 1. Fade to Black
        yield return StartCoroutine(FadeRoutine(0f, 1f, duration));

        // 2. Load Target Scene
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        // 3. Fade In from Black
        yield return StartCoroutine(FadeRoutine(1f, 0f, duration));

        canvasGroup.blocksRaycasts = false;
    }

    public IEnumerator FadeInRoutine(float duration)
    {
        canvasGroup.blocksRaycasts = true;
        yield return StartCoroutine(FadeRoutine(1f, 0f, duration));
        canvasGroup.blocksRaycasts = false;
    }

    public IEnumerator FadeOutRoutine(float duration)
    {
        canvasGroup.blocksRaycasts = true;
        yield return StartCoroutine(FadeRoutine(0f, 1f, duration));
    }

    private IEnumerator FadeRoutine(float startAlpha, float targetAlpha, float duration)
    {
        float timer = 0f;
        canvasGroup.alpha = startAlpha;

        while (timer < duration)
        {
            timer += Time.unscaledDeltaTime; // Uses unscaled time in case timeScale = 0
            canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, timer / duration);
            yield return null;
        }

        canvasGroup.alpha = targetAlpha;
    }
}