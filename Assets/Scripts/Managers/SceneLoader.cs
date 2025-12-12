using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System;
using System.Collections;

public class SceneLoader : Singleton<SceneLoader>
{

    [Header("Fade Settings")]
    [SerializeField] private bool useFadeTransitions = true;
    [SerializeField] private float fadeInDuration = 0.5f;
    [SerializeField] private float fadeOutDuration = 0.5f;
    [SerializeField] private Color fadeColor = Color.black;

    public event Action<string> OnSceneLoadStarted;
    public event Action<string, float> OnSceneLoadProgress;
    public event Action<string> OnSceneLoadCompleted;
    public event Action OnFadeOutComplete;
    public event Action OnFadeInComplete;

    private Canvas fadeCanvas;
    private Image fadeImage;
    private bool isFading;

    protected override void OnAwake()
    {
        CreateFadeCanvas();
    }

    private void CreateFadeCanvas()
    {
        GameObject canvasObj = new GameObject("FadeCanvas");
        canvasObj.transform.SetParent(transform);
        fadeCanvas = canvasObj.AddComponent<Canvas>();
        fadeCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        fadeCanvas.sortingOrder = 9999;
        canvasObj.AddComponent<CanvasScaler>();
        canvasObj.AddComponent<GraphicRaycaster>();
        GameObject imageObj = new GameObject("FadeImage");
        imageObj.transform.SetParent(canvasObj.transform);
        fadeImage = imageObj.AddComponent<Image>();
        fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, 0f);
        RectTransform rect = imageObj.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.sizeDelta = Vector2.zero;
        fadeCanvas.enabled = false;
    }

    public void LoadScene(string sceneName, bool useFade = true, Action onComplete = null)
    {
        StartCoroutine(LoadSceneAsync(sceneName, useFade, onComplete));
    }

    public void LoadScene(int sceneIndex, bool useFade = true, Action onComplete = null)
    {
        StartCoroutine(LoadSceneAsync(sceneIndex, useFade, onComplete));
    }

    public void ReloadCurrentScene(bool useFade = true, Action onComplete = null)
    {
        string currentScene = SceneManager.GetActiveScene().name;
        LoadScene(currentScene, useFade, onComplete);
    }

    public void LoadNextScene(bool useFade = true, Action onComplete = null)
    {
        int nextIndex = SceneManager.GetActiveScene().buildIndex + 1;
        if (nextIndex < SceneManager.sceneCountInBuildSettings)
            LoadScene(nextIndex, useFade, onComplete);
    }

    private IEnumerator LoadSceneAsync(string sceneName, bool useFade, Action onComplete)
    {
        if (useFade && useFadeTransitions)
            yield return StartCoroutine(FadeOut());
        OnSceneLoadStarted?.Invoke(sceneName);
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);
        operation.allowSceneActivation = false;
        while (!operation.isDone)
        {
            float progress = Mathf.Clamp01(operation.progress / 0.9f);
            OnSceneLoadProgress?.Invoke(sceneName, progress);
            if (operation.progress >= 0.9f)
                operation.allowSceneActivation = true;
            yield return null;
        }
        OnSceneLoadCompleted?.Invoke(sceneName);
        if (useFade && useFadeTransitions)
            yield return StartCoroutine(FadeIn());
        onComplete?.Invoke();
    }

    private IEnumerator LoadSceneAsync(int sceneIndex, bool useFade, Action onComplete)
    {
        if (useFade && useFadeTransitions)
            yield return StartCoroutine(FadeOut());
        string sceneName = SceneManager.GetSceneByBuildIndex(sceneIndex).name;
        OnSceneLoadStarted?.Invoke(sceneName);
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneIndex);
        operation.allowSceneActivation = false;
        while (!operation.isDone)
        {
            float progress = Mathf.Clamp01(operation.progress / 0.9f);
            OnSceneLoadProgress?.Invoke(sceneName, progress);
            if (operation.progress >= 0.9f)
                operation.allowSceneActivation = true;
            yield return null;
        }
        OnSceneLoadCompleted?.Invoke(sceneName);
        if (useFade && useFadeTransitions)
            yield return StartCoroutine(FadeIn());
        onComplete?.Invoke();
    }

    public void LoadSceneWithDelay(string sceneName, float delay, bool useFade = true, Action onComplete = null)
    {
        StartCoroutine(LoadSceneDelayed(sceneName, delay, useFade, onComplete));
    }

    private IEnumerator LoadSceneDelayed(string sceneName, float delay, bool useFade, Action onComplete)
    {
        yield return new WaitForSeconds(delay);
        LoadScene(sceneName, useFade, onComplete);
    }

    public void FadeOutScreen(Action onComplete = null)
    {
        StartCoroutine(FadeOutWithCallback(onComplete));
    }

    public void FadeInScreen(Action onComplete = null)
    {
        StartCoroutine(FadeInWithCallback(onComplete));
    }

    private IEnumerator FadeOutWithCallback(Action onComplete)
    {
        yield return StartCoroutine(FadeOut());
        onComplete?.Invoke();
    }

    private IEnumerator FadeInWithCallback(Action onComplete)
    {
        yield return StartCoroutine(FadeIn());
        onComplete?.Invoke();
    }

    private IEnumerator FadeOut()
    {
        if (isFading) yield break;
        isFading = true;
        fadeCanvas.enabled = true;
        float elapsed = 0f;
        Color startColor = new Color(fadeColor.r, fadeColor.g, fadeColor.b, 0f);
        Color endColor = new Color(fadeColor.r, fadeColor.g, fadeColor.b, 1f);
        while (elapsed < fadeOutDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeOutDuration;
            fadeImage.color = Color.Lerp(startColor, endColor, t);
            yield return null;
        }
        fadeImage.color = endColor;
        OnFadeOutComplete?.Invoke();
        isFading = false;
    }

    private IEnumerator FadeIn()
    {
        if (isFading) yield break;
        isFading = true;
        fadeCanvas.enabled = true;
        float elapsed = 0f;
        Color startColor = new Color(fadeColor.r, fadeColor.g, fadeColor.b, 1f);
        Color endColor = new Color(fadeColor.r, fadeColor.g, fadeColor.b, 0f);
        while (elapsed < fadeInDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeInDuration;
            fadeImage.color = Color.Lerp(startColor, endColor, t);
            yield return null;
        }
        fadeImage.color = endColor;
        fadeCanvas.enabled = false;
        OnFadeInComplete?.Invoke();
        isFading = false;
    }

    public void SetFadeSettings(float fadeInTime, float fadeOutTime, Color color)
    {
        fadeInDuration = fadeInTime;
        fadeOutDuration = fadeOutTime;
        fadeColor = color;
    }

    public void SetUseFadeTransitions(bool enabled)
    {
        useFadeTransitions = enabled;
    }
}