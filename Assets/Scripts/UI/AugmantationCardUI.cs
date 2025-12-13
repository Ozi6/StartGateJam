using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;
using static AugmentHandler;

public class AugmentCardUI : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    [Header("UI References")]
    public TMP_Text titleText;
    public TMP_Text descriptionText;
    public Image icon;
    public Image cardBackground;
    public Image borderHighlight;
    public GameObject glowEffect;

    [Header("Hover Animation Settings")]
    [SerializeField] private float hoverScale = 1.15f;
    [SerializeField] private float hoverDuration = 0.2f;
    [SerializeField] private Color hoverBorderColor = new Color(1f, 0.8f, 0f, 1f);
    [SerializeField] private Color normalBorderColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);

    private Augment augment;
    private System.Action<Augment> onSelect;
    private RectTransform rectTransform;
    private Vector3 originalScale;
    private bool isHovering = false;
    private Coroutine hoverCoroutine;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        originalScale = rectTransform.localScale;

        // Add Button component if not present for better interaction
        Button button = GetComponent<Button>();
        if (button == null)
        {
            button = gameObject.AddComponent<Button>();
        }
        button.onClick.AddListener(() => OnPointerClick(null));

        // Initialize card state
        if (borderHighlight != null)
        {
            borderHighlight.color = normalBorderColor;
            borderHighlight.gameObject.SetActive(false);
        }

        if (glowEffect != null)
        {
            glowEffect.SetActive(false);
        }
    }

    public void Setup(Augment aug, System.Action<Augment> selectCallback, float delay = 0f)
    {
        augment = aug;
        onSelect = selectCallback;

        if (titleText != null)
            titleText.text = aug.title;

        if (descriptionText != null)
            descriptionText.text = aug.description;

        if (icon != null && aug.icon != null)
            icon.sprite = aug.icon;

        // Animate entrance if delay is provided
        if (delay > 0f)
        {
            rectTransform.localScale = Vector3.zero;
            StartCoroutine(AnimateEntrance(delay));
        }
    }

    private IEnumerator AnimateEntrance(float delay)
    {
        yield return new WaitForSeconds(delay);

        float elapsed = 0f;
        float duration = 0.4f;
        Vector3 startScale = Vector3.zero;
        Vector3 endScale = originalScale;

        // Add a slight bounce effect
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            // Ease out bounce
            t = 1f - Mathf.Pow(1f - t, 3f);
            rectTransform.localScale = Vector3.Lerp(startScale, endScale, t);
            yield return null;
        }

        rectTransform.localScale = endScale;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (isHovering) return;
        isHovering = true;

        // Stop any existing hover animation
        if (hoverCoroutine != null)
        {
            StopCoroutine(hoverCoroutine);
        }

        hoverCoroutine = StartCoroutine(AnimateHover(true));
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!isHovering) return;
        isHovering = false;

        // Stop any existing hover animation
        if (hoverCoroutine != null)
        {
            StopCoroutine(hoverCoroutine);
        }

        hoverCoroutine = StartCoroutine(AnimateHover(false));
    }

    private IEnumerator AnimateHover(bool isHovering)
    {
        Vector3 targetScale = isHovering ? originalScale * hoverScale : originalScale;
        Color targetBorderColor = isHovering ? hoverBorderColor : normalBorderColor;

        float elapsed = 0f;
        Vector3 startScale = rectTransform.localScale;

        if (borderHighlight != null)
        {
            borderHighlight.gameObject.SetActive(true);
        }

        if (glowEffect != null)
        {
            glowEffect.SetActive(isHovering);
        }

        while (elapsed < hoverDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / hoverDuration;
            // Smooth step for more natural animation
            t = Mathf.SmoothStep(0f, 1f, t);

            rectTransform.localScale = Vector3.Lerp(startScale, targetScale, t);

            if (borderHighlight != null)
            {
                borderHighlight.color = Color.Lerp(borderHighlight.color, targetBorderColor, t);
            }

            yield return null;
        }

        rectTransform.localScale = targetScale;
        if (borderHighlight != null)
        {
            borderHighlight.color = targetBorderColor;
            if (!isHovering)
            {
                borderHighlight.gameObject.SetActive(false);
            }
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        onSelect?.Invoke(augment);
    }
}
