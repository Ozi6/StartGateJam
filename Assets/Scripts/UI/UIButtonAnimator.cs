using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class UIButtonAnimator : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler
{
    [Header("Hover Settings")]
    [SerializeField] private float hoverScale = 1.08f;
    [SerializeField] private float hoverDuration = 0.15f;

    [Header("Entrance Animation")]
    [SerializeField] private float entranceDelay = 0f;

    [Header("Visuals")]
    [SerializeField] private Image borderHighlight;
    [SerializeField] private GameObject glowEffect;
    [SerializeField] private Color normalBorderColor = Color.white;
    [SerializeField] private Color hoverBorderColor = Color.yellow;

    private RectTransform rectTransform;
    private Vector3 originalScale;

    private bool isHovering;
    private Coroutine hoverCoroutine;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        originalScale = rectTransform.localScale;
    }

    void OnEnable()
    {
        rectTransform.localScale = Vector3.zero;
        StartCoroutine(AnimateEntrance(entranceDelay));
    }

    // =============================
    // ENTRANCE ANIMATION
    // =============================
    private IEnumerator AnimateEntrance(float delay)
    {
        yield return new WaitForSeconds(delay);

        float elapsed = 0f;
        float duration = 0.4f;
        Vector3 startScale = Vector3.zero;
        Vector3 endScale = originalScale;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            t = 1f - Mathf.Pow(1f - t, 3f); // ease-out

            rectTransform.localScale = Vector3.Lerp(startScale, endScale, t);
            yield return null;
        }

        rectTransform.localScale = endScale;
    }

    // =============================
    // POINTER EVENTS
    // =============================
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (isHovering) return;
        isHovering = true;

        if (hoverCoroutine != null)
            StopCoroutine(hoverCoroutine);

        hoverCoroutine = StartCoroutine(AnimateHover(true));
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!isHovering) return;
        isHovering = false;

        if (hoverCoroutine != null)
            StopCoroutine(hoverCoroutine);

        hoverCoroutine = StartCoroutine(AnimateHover(false));
    }

    // =============================
    // HOVER ANIMATION
    // =============================
    private IEnumerator AnimateHover(bool hover)
    {
        Vector3 targetScale = hover ? originalScale * hoverScale : originalScale;
        Color targetColor = hover ? hoverBorderColor : normalBorderColor;

        float elapsed = 0f;
        Vector3 startScale = rectTransform.localScale;

        if (borderHighlight != null)
            borderHighlight.gameObject.SetActive(true);

        if (glowEffect != null)
            glowEffect.SetActive(hover);

        while (elapsed < hoverDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / hoverDuration);

            rectTransform.localScale = Vector3.Lerp(startScale, targetScale, t);

            if (borderHighlight != null)
                borderHighlight.color = Color.Lerp(borderHighlight.color, targetColor, t);

            yield return null;
        }

        rectTransform.localScale = targetScale;

        if (borderHighlight != null)
        {
            borderHighlight.color = targetColor;
            if (!hover)
                borderHighlight.gameObject.SetActive(false);
        }
    }
}
