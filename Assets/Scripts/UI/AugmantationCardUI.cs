using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static AugmentHandler;

public class AugmentCardUI : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    public TMP_Text titleText;
    public TMP_Text descriptionText;
    public Image icon;
    public Image backgroundImage;

    private Augment augment;
    private System.Action<Augment> onSelect;
    private Color originalBackgroundColor;
    private Color hoverColor = new Color(0.3f, 0.5f, 0.8f, 1f);
    private Color normalColor = new Color(0.2f, 0.2f, 0.3f, 1f);
    private Vector3 originalScale;

    private void Awake()
    {
        originalScale = transform.localScale;
        
        // Ensure we have a background image
        if (backgroundImage == null)
        {
            backgroundImage = GetComponent<Image>();
            if (backgroundImage == null)
            {
                backgroundImage = gameObject.AddComponent<Image>();
            }
        }
        
        if (backgroundImage != null)
        {
            originalBackgroundColor = normalColor;
            backgroundImage.color = originalBackgroundColor;
        }
    }

    public void Setup(Augment aug, System.Action<Augment> selectCallback)
    {
        augment = aug;
        onSelect = selectCallback;

        // Find or create text components if they don't exist
        EnsureTextComponents();

        if (titleText != null)
            titleText.text = aug.title;
        if (descriptionText != null)
            descriptionText.text = aug.description;
        if (icon != null && aug.icon != null)
            icon.sprite = aug.icon;

        // Setup card appearance if not already set up
        SetupCardVisuals();
    }

    private void EnsureTextComponents()
    {
        // Find title text if not assigned
        if (titleText == null)
        {
            // Try to find in children
            titleText = GetComponentInChildren<TMP_Text>();
            if (titleText != null && titleText.name.ToLower().Contains("title"))
            {
                // Found it
            }
            else
            {
                // Create title text
                GameObject titleObj = new GameObject("TitleText");
                titleObj.transform.SetParent(transform, false);
                RectTransform titleRect = titleObj.AddComponent<RectTransform>();
                titleRect.anchorMin = new Vector2(0, 0.6f);
                titleRect.anchorMax = new Vector2(1, 1);
                titleRect.offsetMin = new Vector2(10, 0);
                titleRect.offsetMax = new Vector2(-10, -10);
                
                titleText = titleObj.AddComponent<TMP_Text>();
                titleText.fontSize = 24;
                titleText.fontStyle = FontStyles.Bold;
                titleText.alignment = TextAlignmentOptions.Top;
                titleText.color = Color.white;
                titleText.enableWordWrapping = true;
                titleText.overflowMode = TextOverflowModes.Ellipsis;
            }
        }

        // Find description text if not assigned
        if (descriptionText == null)
        {
            // Try to find in children (but not the one we just found/created)
            TMP_Text[] allTexts = GetComponentsInChildren<TMP_Text>();
            foreach (TMP_Text text in allTexts)
            {
                if (text != titleText && (text.name.ToLower().Contains("desc") || text.name.ToLower().Contains("description")))
                {
                    descriptionText = text;
                    break;
                }
            }
            
            if (descriptionText == null)
            {
                // Create description text
                GameObject descObj = new GameObject("DescriptionText");
                descObj.transform.SetParent(transform, false);
                RectTransform descRect = descObj.AddComponent<RectTransform>();
                descRect.anchorMin = new Vector2(0, 0);
                descRect.anchorMax = new Vector2(1, 0.6f);
                descRect.offsetMin = new Vector2(10, 10);
                descRect.offsetMax = new Vector2(-10, -10);
                
                descriptionText = descObj.AddComponent<TMP_Text>();
                descriptionText.fontSize = 16;
                descriptionText.alignment = TextAlignmentOptions.Top;
                descriptionText.color = new Color(0.9f, 0.9f, 0.9f, 1f);
                descriptionText.enableWordWrapping = true;
                descriptionText.overflowMode = TextOverflowModes.Truncate;
            }
        }

        // Find icon if not assigned
        if (icon == null)
        {
            Image[] allImages = GetComponentsInChildren<Image>();
            foreach (Image img in allImages)
            {
                if (img != backgroundImage && (img.name.ToLower().Contains("icon") || img.name.ToLower().Contains("image")))
                {
                    icon = img;
                    break;
                }
            }
        }
    }

    private void SetupCardVisuals()
    {
        RectTransform rectTransform = GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            // Set card size
            if (rectTransform.sizeDelta.x < 50 || rectTransform.sizeDelta.y < 50)
            {
                rectTransform.sizeDelta = new Vector2(250, 400);
            }
        }

        // Ensure background styling
        if (backgroundImage != null)
        {
            backgroundImage.color = normalColor;
        }
        else
        {
            // Create background if it doesn't exist
            backgroundImage = GetComponent<Image>();
            if (backgroundImage == null)
            {
                backgroundImage = gameObject.AddComponent<Image>();
            }
            if (backgroundImage != null)
            {
                backgroundImage.color = normalColor;
            }
        }

        // Add button component for better interaction
        Button button = GetComponent<Button>();
        if (button == null)
        {
            button = gameObject.AddComponent<Button>();
            button.transition = Selectable.Transition.None; // We'll handle transitions manually
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        onSelect?.Invoke(augment);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        // Hover effect
        if (backgroundImage != null)
        {
            backgroundImage.color = hoverColor;
        }
        transform.localScale = originalScale * 1.05f;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // Return to normal
        if (backgroundImage != null)
        {
            backgroundImage.color = normalColor;
        }
        transform.localScale = originalScale;
    }
}
