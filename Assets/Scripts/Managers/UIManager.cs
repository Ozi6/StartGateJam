using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using static AugmentHandler;

public class UIManager : MonoBehaviour
{
    [Header("Shop UI")]
    public ShopUI shopUI;
    public TMP_Text currentGoldText;

    [Header("Augment Selection UI")]
    public GameObject augmentSelectionPanel;
    public GameObject augmentCardPrefab;
    public Transform augmentCardContainer;
    private GameObject augmentOverlay;

    // Store the current augment options
    private List<Augment> currentAugmentOptions = new();

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
        shopUI?.ToggleShop(false);

        // Initialize augment selection panel
        if (augmentSelectionPanel != null)
        {
            augmentSelectionPanel.SetActive(false);
        }
    }

    public void ShowShopUI(bool show)
    {
        UpdateGoldUI();
        shopUI?.ToggleShop(show);
    }

    public void ShowAugmentSelection()
    {
        if (augmentSelectionPanel == null)
        {
            Debug.LogError("Augment Selection Panel is not assigned!");
            return;
        }

        // Get random augments from AugmentHandler
        if (AugmentHandler.Instance != null)
        {
            currentAugmentOptions = AugmentHandler.Instance.GetRandomAugments(3);
        }
        else
        {
            Debug.LogError("AugmentHandler instance not found!");
            return;
        }

        // Create dark overlay if it doesn't exist
        if (augmentOverlay == null)
        {
            augmentOverlay = new GameObject("AugmentOverlay");
            augmentOverlay.transform.SetParent(canvas.transform, false);
            Image overlayImage = augmentOverlay.AddComponent<Image>();
            overlayImage.color = new Color(0, 0, 0, 0.7f);

            RectTransform overlayRect = augmentOverlay.GetComponent<RectTransform>();
            overlayRect.anchorMin = Vector2.zero;
            overlayRect.anchorMax = Vector2.one;
            overlayRect.sizeDelta = Vector2.zero;
            overlayRect.anchoredPosition = Vector2.zero;
        }

        augmentOverlay.SetActive(true);
        augmentOverlay.transform.SetAsLastSibling();
        augmentSelectionPanel.SetActive(true);
        augmentSelectionPanel.transform.SetAsLastSibling();

        // Clear previous cards
        if (augmentCardContainer != null)
        {
            foreach (Transform child in augmentCardContainer)
            {
                Destroy(child.gameObject);
            }

            // Create augment cards based on the random selection
            for (int i = 0; i < currentAugmentOptions.Count; i++)
            {
                CreateAugmentCard(currentAugmentOptions[i]);
            }
        }
    }

    private void CreateAugmentCard(Augment augment)
    {
        if (augmentCardPrefab == null || augmentCardContainer == null)
        {
            Debug.LogError("Augment card prefab or container is not assigned!");
            return;
        }

        GameObject card = Instantiate(augmentCardPrefab, augmentCardContainer);

        // Get the AugmentCardUI component and set it up
        AugmentCardUI cardUI = card.GetComponentInChildren<AugmentCardUI>();
        if (cardUI != null)
        {
            cardUI.Setup(augment, OnAugmentSelected);
        }
        else
        {
            Debug.LogError("AugmentCardUI component not found on prefab!");
        }
    }

    private void OnAugmentSelected(Augment selectedAugment)
    {
        Debug.Log($"Augment '{selectedAugment.title}' selected!");

        // Apply the augment through AugmentHandler
        if (AugmentHandler.Instance != null)
        {
            AugmentHandler.Instance.PurchaseAugment(selectedAugment.id);
        }

        // TODO: Apply actual gameplay effects based on augment.id
        // Example: 
        // switch(selectedAugment.id) {
        //     case 0: // Vital Boost
        //         PowerUpManager.Instance.EnableVitalBoost();
        //         break;
        //     ...
        // }

        HideAugmentSelection();

        // Move to next wave's shopping phase
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetState(GameState.Shopping);
        }
    }

    public void HideAugmentSelection()
    {
        if (augmentSelectionPanel != null)
        {
            augmentSelectionPanel.SetActive(false);
        }

        if (augmentOverlay != null)
        {
            augmentOverlay.SetActive(false);
        }

        // Clear cards
        if (augmentCardContainer != null)
        {
            foreach (Transform child in augmentCardContainer)
            {
                Destroy(child.gameObject);
            }
        }

        currentAugmentOptions.Clear();
    }

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