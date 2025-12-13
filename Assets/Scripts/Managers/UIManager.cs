using System.Collections;
using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    [Header("Shop UI")]
    public ShopUI shopUI; // Reference to the new ShopUI script
    public TMP_Text currentGoldText; // Display player currency

    [Header("UI Elements")]
    public GameObject speechBubblePrefab;
    public Canvas canvas;
    public GameObject sidePanel;
    public TMP_Text firstKeywordText;
    public TMP_Text secondKeywordText;

    private GameObject speechBubbleInstance;
    private TMP_Text bubbleText;

    private Vector2 panelClosedPos;
    private Vector2 panelOpenPos;
    private const float panelSlideDuration = 0.5f;
    private bool isPanelOpen = false;

    public void InitializeUI()
    {
        if (sidePanel != null)
        {
            RectTransform rt = sidePanel.GetComponent<RectTransform>();
            if (rt != null)
            {
                panelOpenPos = rt.anchoredPosition;
                panelClosedPos = new Vector2(-rt.rect.width, panelOpenPos.y);
                rt.anchoredPosition = panelClosedPos;
            }
        }
        SetKeywordUIVisibility(false, firstKeywordText);
        SetKeywordUIVisibility(false, secondKeywordText);

        // Initialize Shop UI visibility (Start hidden)
        shopUI?.ToggleShop(false);
    }

    // --- New Shop UI Methods ---

    /// <summary>
    /// Triggers the shop panel's animated appearance/disappearance.
    /// </summary>
    public void ShowShopUI(bool show)
    {
        // Ensure the gold display is up-to-date when the shop is shown
        UpdateGoldUI();
        shopUI?.ToggleShop(show);
    }

    /// <summary>
    /// Updates the text element displaying the player's gold.
    /// </summary>
    public void UpdateGoldUI()
    {
        if (currentGoldText != null && GameManager.Instance != null)
        {
            currentGoldText.text = $"Gold: {GameManager.Instance.currentGold}";
        }
    }

    private void SetKeywordUIVisibility(bool visible, TMP_Text textObject)
    {
        if (textObject != null)
        {
            textObject.gameObject.SetActive(visible);
        }
    }

    public void UpdateSpeechBubble(string text)
    {
        if (speechBubbleInstance == null)
        {
            speechBubbleInstance = Instantiate(speechBubblePrefab, canvas.transform);
            bubbleText = speechBubbleInstance.GetComponentInChildren<TMP_Text>();
        }
        speechBubbleInstance.SetActive(!string.IsNullOrEmpty(text));
        if (bubbleText != null)
        {
            bubbleText.text = text;
        }
    }

    public void DisplayKeyword(string keyword)
    {
        if (firstKeywordText != null)
        {
            firstKeywordText.text = keyword;
            SetKeywordUIVisibility(true, firstKeywordText);
        }
    }

    public void DisplayAndHideSecondKeyword(string keyword, float delay)
    {
        if (secondKeywordText != null)
        {
            secondKeywordText.text = keyword;
            SetKeywordUIVisibility(true, secondKeywordText);
            StartCoroutine(HideSecondUI(delay));
        }
    }

    public void ClearHeldKeywordUI()
    {
        if (firstKeywordText != null)
        {
            firstKeywordText.text = "";
            SetKeywordUIVisibility(false, firstKeywordText);
        }
    }

    private IEnumerator HideSecondUI(float delay)
    {
        yield return new WaitForSeconds(delay);
        SetKeywordUIVisibility(false, secondKeywordText);
        SetKeywordUIVisibility(false, firstKeywordText);
        secondKeywordText.text = "";
        firstKeywordText.text = "";
    }

    public void TogglePanel()
    {
        isPanelOpen = !isPanelOpen;
        StartCoroutine(SlidePanel(isPanelOpen));
    }

    private IEnumerator SlidePanel(bool open)
    {
        if (sidePanel == null) yield break;
        RectTransform rt = sidePanel.GetComponent<RectTransform>();
        if (rt == null) yield break;
        Vector2 start = rt.anchoredPosition;
        Vector2 end = open ? panelOpenPos : panelClosedPos;
        float time = 0f;
        while (time < panelSlideDuration)
        {
            time += Time.deltaTime;
            rt.anchoredPosition = Vector2.Lerp(start, end, time / panelSlideDuration);
            yield return null;
        }
        rt.anchoredPosition = end;
    }
}