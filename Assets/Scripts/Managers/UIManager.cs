using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using static AugmentHandler;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("Shop UI")]
    public ShopUI shopUI;
    public TMP_Text currentGoldText;

    [Header("Augment Selection UI")]
    public GameObject augmentSelectionPanel;
    public GameObject augmentCardPrefab;
    public Transform augmentCardContainer;
    private GameObject augmentOverlay;
    private List<Augment> currentAugmentOptions = new();

    [Header("Upgrade UI (Right Panel)")]
    public GameObject rightSidePanel;
    public Button upgradeButton;
    private Vector2 rightPanelClosedPos;
    private Vector2 rightPanelOpenPos;
    private bool isRightPanelOpen = false;
    private GameObject currentSelectedUnit;

    [Header("UI Elements (Left Panel)")]
    public GameObject speechBubblePrefab;
    public Canvas canvas;
    public GameObject sidePanel; // Left side panel
    public TMP_Text firstKeywordText;
    public TMP_Text secondKeywordText;

    private GameObject speechBubbleInstance;
    private TMP_Text bubbleText;

    // Left panel positions
    private Vector2 panelClosedPos;
    private Vector2 panelOpenPos;
    private const float panelSlideDuration = 0.3f;
    private bool isPanelOpen = false;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void InitializeUI()
    {
        // --- Setup Left Panel (Slides Left to Hide) ---
        if (sidePanel != null)
        {
            RectTransform rt = sidePanel.GetComponent<RectTransform>();
            if (rt != null)
            {
                panelOpenPos = rt.anchoredPosition;
                // Subtract Width to move LEFT
                panelClosedPos = new Vector2(panelOpenPos.x - rt.rect.width, panelOpenPos.y);
                rt.anchoredPosition = panelClosedPos;
            }
        }

        // --- Setup Right Panel (Slides Right to Hide) ---
        if (rightSidePanel != null)
        {
            RectTransform rt = rightSidePanel.GetComponent<RectTransform>();
            if (rt != null)
            {
                rightPanelOpenPos = rt.anchoredPosition;
                // Add Width to move RIGHT
                rightPanelClosedPos = new Vector2(rightPanelOpenPos.x + rt.rect.width, rightPanelOpenPos.y);
                rt.anchoredPosition = rightPanelClosedPos;
            }
        }

        SetKeywordUIVisibility(false, firstKeywordText);
        SetKeywordUIVisibility(false, secondKeywordText);
        shopUI?.ToggleShop(false);

        if (augmentSelectionPanel != null) augmentSelectionPanel.SetActive(false);
    }

    // --- Right Panel / Upgrade Logic ---

    public void OpenUpgradePanel(GameObject unit)
    {
        currentSelectedUnit = unit;

        Person person = unit.GetComponent<Person>();

        person.GetStats(
            out int maxHp,
            out int hp,
            out float moveSpd,
            out float dmg,
            out float atkRange,
            out float atkSpd,
            out float dmgArea,
            out float dmgMult,
            out float dmgTakenMult,
            out bool invulnerable,
            out bool canMove,
            out bool areaDamage,
            out bool lifeSteal
        );

        if (rightSidePanel != null)
        {
            Transform menu = rightSidePanel.transform;

            menu.Find("Stats_Unit").GetComponent<TMP_Text>().text =
                $"{unit.tag}";

            menu.Find("Stats_Health").GetComponent<TMP_Text>().text =
                $"{maxHp}";

            menu.Find("Stats_Attack").GetComponent<TMP_Text>().text =
                $"{dmg}";

            menu.Find("Stats_AttackSpeed").GetComponent<TMP_Text>().text =
                $"{atkSpd}";

            menu.Find("Stats_Range").GetComponent<TMP_Text>().text =
                $"{atkRange}";

            menu.Find("Stats_MovementSpeed").GetComponent<TMP_Text>().text =
                $"{moveSpd}";

            // === UPGRADE COST + ICON ===
            TMP_Text upgradeCostText =
                menu.Find("stats_upgrade_cost").GetComponent<TMP_Text>();

            Image upgradeCostIcon =
                menu.Find("Stats_UpgradeIcon").GetComponent<Image>();

            upgradeCostText.text = $"{person.upgradeCost}";

            // ALREADY UPGRADED → hide cost & icon
            if (person.upgradeCost == 0)
            {
                upgradeCostText.gameObject.SetActive(false);
                upgradeCostIcon.gameObject.SetActive(false);
            }
            else
            {
                upgradeCostText.gameObject.SetActive(true);
                upgradeCostIcon.gameObject.SetActive(true);

                // NOT ENOUGH GOLD → red
                if (person.upgradeCost > GameManager.Instance.currentGold)
                {
                    upgradeCostText.color = Color.red;
                }
                else
                {
                    // Enough gold → normal color
                    upgradeCostText.color = Color.black;
                }
            }
        }

        TMP_Text buttonText = upgradeButton.GetComponentInChildren<TMP_Text>();

        // === BUTTON LOGIC ===

        // ALREADY UPGRADED
        if (person.upgradeCost == 0)
        {
            upgradeButton.onClick.RemoveAllListeners();
            upgradeButton.interactable = false;

            if (buttonText != null)
                buttonText.text = "UPGRADED";
        }
        // CAN UPGRADE
        else if (person.upgradeCost <= GameManager.Instance.currentGold)
        {
            upgradeButton.interactable = true;

            if (buttonText != null)
                buttonText.text = "UPGRADE";

            upgradeButton.onClick.RemoveAllListeners();
            upgradeButton.onClick.AddListener(() => UpgradeUnit(unit));
        }
        // NOT ENOUGH GOLD
        else
        {
            upgradeButton.onClick.RemoveAllListeners();
            upgradeButton.interactable = false;

            if (buttonText != null)
                buttonText.text = "NOT ENOUGH GOLD";
        }

        if (!isRightPanelOpen)
        {
            isRightPanelOpen = true;
            StartCoroutine(
                SlidePanel(
                    rightSidePanel.GetComponent<RectTransform>(),
                    rightPanelClosedPos,
                    rightPanelOpenPos
                )
            );
        }
    }


    private void UpgradeUnit(GameObject unit)
    {
        if (unit == null || ObjectPooler.Instance == null)
        {
            Debug.LogWarning("Cannot upgrade: unit or ObjectPooler is null");
            return;
        }

        Person oldPerson = unit.GetComponent<Person>();
        if (oldPerson == null)
        {
            Debug.LogWarning("Cannot upgrade: unit does not have a Person component");
            return;
        }

        string currentTag = unit.tag;
        string upgradedTag = currentTag + "_lvl2";

        GameObject upgradedUnitGO = ObjectPooler.Instance.SpawnFromPool(
            upgradedTag,
            unit.transform.position,
            unit.transform.rotation
        );

        if (upgradedUnitGO != null)
        {
            Person upgradedPerson = upgradedUnitGO.GetComponent<Person>();
            if (upgradedPerson != null)
            {
                // Remove old unit from playersTeam
                GameManager.Instance.currentGold -= oldPerson.upgradeCost;
                GameManager.Instance.playersTeam.Remove(oldPerson);

                upgradedPerson.isFriendly = true;
                GameManager.Instance.playersTeam.Add(upgradedPerson);

                Debug.Log($"Upgraded {currentTag} to {upgradedTag}");

                ObjectPooler.Instance.ReturnToPool(unit, currentTag);

                GameManager.Instance.UpdateUnitCountDisplay();

                CloseUpgradePanel();
            }
            else
            {
                Debug.LogWarning($"Spawned upgraded unit '{upgradedTag}' does not have a Person component.");
                ObjectPooler.Instance.ReturnToPool(upgradedUnitGO, upgradedTag);
            }
        }
        else
        {
            Debug.LogWarning($"Failed to spawn upgraded unit with tag '{upgradedTag}'. Make sure this tag exists in ObjectPooler.");
        }
    }

    public void ResetSpells()
    {
        firstKeywordText.text = "";
        secondKeywordText.text = "";
    }
    public void CloseUpgradePanel()
    {
        currentSelectedUnit = null;
        if (isRightPanelOpen)
        {
            isRightPanelOpen = false;
            // Slide from Open to Closed (Right)
            StartCoroutine(SlidePanel(rightSidePanel.GetComponent<RectTransform>(), rightPanelOpenPos, rightPanelClosedPos));
        }
    }

    // --- Existing Functionality ---

    public void ShowShopUI(bool show)
    {
        UpdateGoldUI();
        shopUI?.ToggleShop(show);
    }

    public void ShowAugmentSelection()
    {
        if (augmentSelectionPanel == null) return;

        if (AugmentHandler.Instance != null)
            currentAugmentOptions = AugmentHandler.Instance.GetRandomAugments(3);

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
        }

        augmentOverlay.SetActive(true);
        augmentSelectionPanel.SetActive(true);
        augmentSelectionPanel.transform.SetAsLastSibling();

        if (augmentCardContainer != null)
        {
            foreach (Transform child in augmentCardContainer) Destroy(child.gameObject);
            for (int i = 0; i < currentAugmentOptions.Count; i++)
            {
                CreateAugmentCard(currentAugmentOptions[i]);
            }
        }
    }

    private void CreateAugmentCard(Augment augment)
    {
        GameObject card = Instantiate(augmentCardPrefab, augmentCardContainer);
        AugmentCardUI cardUI = card.GetComponentInChildren<AugmentCardUI>();
        if (cardUI != null) cardUI.Setup(augment, OnAugmentSelected);
    }

    private void OnAugmentSelected(Augment selectedAugment)
    {
        if (AugmentHandler.Instance != null)
            AugmentHandler.Instance.PurchaseAugment(selectedAugment.id);

        HideAugmentSelection();
        if (GameManager.Instance != null) GameManager.Instance.SetState(GameState.Shopping);
    }

    public void HideAugmentSelection()
    {
        if (augmentSelectionPanel != null) augmentSelectionPanel.SetActive(false);
        if (augmentOverlay != null) augmentOverlay.SetActive(false);
        currentAugmentOptions.Clear();
    }

    public void UpdateGoldUI()
    {
        if (currentGoldText != null && GameManager.Instance != null)
            currentGoldText.text = $"Gold: {GameManager.Instance.currentGold}";
    }

    private void SetKeywordUIVisibility(bool visible, TMP_Text textObject)
    {
        if (textObject != null) textObject.gameObject.SetActive(visible);
    }

    [SerializeField] Transform bubbleLoc;

    public void UpdateSpeechBubble(string text)
    {
        if (speechBubbleInstance == null)
        {
            speechBubbleInstance = Instantiate(speechBubblePrefab, bubbleLoc);
            bubbleText = speechBubbleInstance.GetComponentInChildren<TMP_Text>();
        }
        speechBubbleInstance.SetActive(!string.IsNullOrEmpty(text));
        if (bubbleText != null) bubbleText.text = text;
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
        if (sidePanel != null)
            StartCoroutine(SlidePanel(sidePanel.GetComponent<RectTransform>(), isPanelOpen ? panelClosedPos : panelOpenPos, isPanelOpen ? panelOpenPos : panelClosedPos));
    }

    private IEnumerator SlidePanel(RectTransform rt, Vector2 start, Vector2 end)
    {
        if (rt == null) yield break;

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