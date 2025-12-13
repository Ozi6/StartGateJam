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
    private TMP_Text panelTitleText;

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
            // Don't setup here - will be set up when shown if needed
        }
    }

    public void ShowShopUI(bool show)
    {
        UpdateGoldUI();
        shopUI?.ToggleShop(show);
    }

    public void ShowAugmentSelection()
    {
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

        // Create or setup the panel if it doesn't exist or isn't properly configured
        if (augmentSelectionPanel == null || canvas == null)
        {
            if (canvas == null)
            {
                Debug.LogError("Canvas is not assigned! Cannot create augment selection panel.");
                return;
            }
            CreateAugmentSelectionPanel();
        }
        else
        {
            // Only setup if panel is valid (has transform and is not destroyed)
            if (augmentSelectionPanel != null && augmentSelectionPanel.transform != null)
            {
                SetupAugmentSelectionPanel();
            }
            else
            {
                // Panel is invalid, create a new one
                CreateAugmentSelectionPanel();
            }
        }

        // Create dark overlay if it doesn't exist
        if (augmentOverlay == null)
        {
            augmentOverlay = new GameObject("AugmentOverlay");
            augmentOverlay.transform.SetParent(canvas.transform, false);
            Image overlayImage = augmentOverlay.AddComponent<Image>();
            overlayImage.color = new Color(0, 0, 0, 0.75f);

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
        
        // Force panel to center of screen
        RectTransform panelRect = augmentSelectionPanel.GetComponent<RectTransform>();
        if (panelRect != null)
        {
            panelRect.anchorMin = new Vector2(0.5f, 0.5f);
            panelRect.anchorMax = new Vector2(0.5f, 0.5f);
            panelRect.pivot = new Vector2(0.5f, 0.5f);
            panelRect.anchoredPosition = Vector2.zero;
        }

        // Update title
        if (panelTitleText != null)
        {
            panelTitleText.text = "Choose an Augment";
        }

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

    private void CreateAugmentSelectionPanel()
    {
        // Create main panel
        augmentSelectionPanel = new GameObject("AugmentSelectionPanel");
        augmentSelectionPanel.transform.SetParent(canvas.transform, false);

        RectTransform panelRect = augmentSelectionPanel.AddComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        panelRect.sizeDelta = new Vector2(900, 600);
        panelRect.anchoredPosition = Vector2.zero;

        // Add background image
        Image panelImage = augmentSelectionPanel.AddComponent<Image>();
        panelImage.color = new Color(0.15f, 0.15f, 0.2f, 0.95f);

        // Add outline/shadow effect using a simple border
        GameObject border = new GameObject("Border");
        border.transform.SetParent(augmentSelectionPanel.transform, false);
        RectTransform borderRect = border.AddComponent<RectTransform>();
        borderRect.anchorMin = Vector2.zero;
        borderRect.anchorMax = Vector2.one;
        borderRect.sizeDelta = Vector2.zero;
        borderRect.anchoredPosition = Vector2.zero;
        Image borderImage = border.AddComponent<Image>();
        borderImage.color = new Color(0.3f, 0.5f, 0.8f, 1f);
        borderRect.offsetMin = new Vector2(-3, -3);
        borderRect.offsetMax = new Vector2(3, 3);

        // Create title
        GameObject titleObj = new GameObject("Title");
        titleObj.transform.SetParent(augmentSelectionPanel.transform, false);
        RectTransform titleRect = titleObj.AddComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0, 1);
        titleRect.anchorMax = new Vector2(1, 1);
        titleRect.pivot = new Vector2(0.5f, 1);
        titleRect.sizeDelta = new Vector2(0, 80);
        titleRect.anchoredPosition = new Vector2(0, -10);

        panelTitleText = titleObj.AddComponent<TMP_Text>();
        panelTitleText.text = "Choose an Augment";
        panelTitleText.fontSize = 36;
        panelTitleText.fontStyle = FontStyles.Bold;
        panelTitleText.alignment = TextAlignmentOptions.Center;
        panelTitleText.color = Color.white;

        // Create card container
        GameObject containerObj = new GameObject("CardContainer");
        containerObj.transform.SetParent(augmentSelectionPanel.transform, false);
        RectTransform containerRect = containerObj.AddComponent<RectTransform>();
        containerRect.anchorMin = new Vector2(0, 0);
        containerRect.anchorMax = new Vector2(1, 1);
        containerRect.sizeDelta = Vector2.zero;
        containerRect.anchoredPosition = Vector2.zero;
        containerRect.offsetMin = new Vector2(20, 100);
        containerRect.offsetMax = new Vector2(-20, -20);

        HorizontalLayoutGroup layoutGroup = containerObj.AddComponent<HorizontalLayoutGroup>();
        layoutGroup.spacing = 20;
        layoutGroup.padding = new RectOffset(20, 20, 20, 20);
        layoutGroup.childAlignment = TextAnchor.MiddleCenter;
        layoutGroup.childControlWidth = false;
        layoutGroup.childControlHeight = false;
        layoutGroup.childForceExpandWidth = false;
        layoutGroup.childForceExpandHeight = false;

        ContentSizeFitter sizeFitter = containerObj.AddComponent<ContentSizeFitter>();
        sizeFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
        sizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        augmentCardContainer = containerObj.transform;
    }

    private void SetupAugmentSelectionPanel()
    {
        if (augmentSelectionPanel == null) return;
        
        // Ensure panel is parented to canvas
        if (canvas != null && augmentSelectionPanel.transform.parent != canvas.transform)
        {
            augmentSelectionPanel.transform.SetParent(canvas.transform, false);
        }

        RectTransform panelRect = augmentSelectionPanel.GetComponent<RectTransform>();
        if (panelRect == null)
        {
            panelRect = augmentSelectionPanel.AddComponent<RectTransform>();
        }
        
        if (panelRect == null) return; // Safety check

        // Ensure proper positioning (centered) - force it
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        panelRect.anchoredPosition = Vector2.zero;
        panelRect.localPosition = Vector3.zero;
        panelRect.localScale = Vector3.one;

        // Set size if not already set
        if (panelRect.sizeDelta.x < 100 || panelRect.sizeDelta.y < 100)
        {
            panelRect.sizeDelta = new Vector2(900, 600);
        }

        // Ensure background exists
        Image panelImage = augmentSelectionPanel.GetComponent<Image>();
        if (panelImage == null)
        {
            panelImage = augmentSelectionPanel.AddComponent<Image>();
            panelImage.color = new Color(0.15f, 0.15f, 0.2f, 0.95f);
        }

        // Find or create title
        if (augmentSelectionPanel.transform != null)
        {
            Transform titleTransform = augmentSelectionPanel.transform.Find("Title");
            if (titleTransform == null)
            {
                GameObject titleObj = new GameObject("Title");
                if (titleObj != null && augmentSelectionPanel.transform != null)
                {
                    titleObj.transform.SetParent(augmentSelectionPanel.transform, false);
                    RectTransform titleRect = titleObj.AddComponent<RectTransform>();
                    if (titleRect != null)
                    {
                        titleRect.anchorMin = new Vector2(0, 1);
                        titleRect.anchorMax = new Vector2(1, 1);
                        titleRect.pivot = new Vector2(0.5f, 1);
                        titleRect.sizeDelta = new Vector2(0, 80);
                        titleRect.anchoredPosition = new Vector2(0, -10);
                    }

                    panelTitleText = titleObj.AddComponent<TMP_Text>();
                    if (panelTitleText != null)
                    {
                        panelTitleText.text = "Choose an Augment";
                        panelTitleText.fontSize = 36;
                        panelTitleText.fontStyle = FontStyles.Bold;
                        panelTitleText.alignment = TextAlignmentOptions.Center;
                        panelTitleText.color = Color.white;
                    }
                }
            }
            else
            {
                panelTitleText = titleTransform.GetComponent<TMP_Text>();
                if (panelTitleText == null)
                {
                    panelTitleText = titleTransform.gameObject.AddComponent<TMP_Text>();
                    if (panelTitleText != null)
                    {
                        panelTitleText.text = "Choose an Augment";
                        panelTitleText.fontSize = 36;
                        panelTitleText.fontStyle = FontStyles.Bold;
                        panelTitleText.alignment = TextAlignmentOptions.Center;
                        panelTitleText.color = Color.white;
                    }
                }
            }
        }

        // Find or create card container
        Transform containerTransform = augmentSelectionPanel.transform.Find("CardContainer");
        if (containerTransform == null)
        {
            GameObject containerObj = new GameObject("CardContainer");
            containerObj.transform.SetParent(augmentSelectionPanel.transform, false);
            RectTransform containerRect = containerObj.AddComponent<RectTransform>();
            containerRect.anchorMin = new Vector2(0, 0);
            containerRect.anchorMax = new Vector2(1, 1);
            containerRect.sizeDelta = Vector2.zero;
            containerRect.anchoredPosition = Vector2.zero;
            containerRect.offsetMin = new Vector2(20, 100);
            containerRect.offsetMax = new Vector2(-20, -20);

            HorizontalLayoutGroup layoutGroup = containerObj.AddComponent<HorizontalLayoutGroup>();
            layoutGroup.spacing = 20;
            layoutGroup.padding = new RectOffset(20, 20, 20, 20);
            layoutGroup.childAlignment = TextAnchor.MiddleCenter;
            layoutGroup.childControlWidth = false;
            layoutGroup.childControlHeight = false;
            layoutGroup.childForceExpandWidth = false;
            layoutGroup.childForceExpandHeight = false;

            augmentCardContainer = containerObj.transform;
        }
        else
        {
            augmentCardContainer = containerTransform;
            
            // Ensure layout group exists
            HorizontalLayoutGroup layoutGroup = containerTransform.GetComponent<HorizontalLayoutGroup>();
            if (layoutGroup == null)
            {
                layoutGroup = containerTransform.gameObject.AddComponent<HorizontalLayoutGroup>();
                layoutGroup.spacing = 20;
                layoutGroup.padding = new RectOffset(20, 20, 20, 20);
                layoutGroup.childAlignment = TextAnchor.MiddleCenter;
                layoutGroup.childControlWidth = false;
                layoutGroup.childControlHeight = false;
                layoutGroup.childForceExpandWidth = false;
                layoutGroup.childForceExpandHeight = false;
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