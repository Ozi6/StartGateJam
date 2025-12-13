using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class BuyButton : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("Unit Data")]
    [SerializeField] private string unitTag;
    [SerializeField] private Sprite unitIcon;

    [Header("UI References")]
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI priceText;
    [SerializeField] private TextMeshProUGUI nameText;

    [Header("Drag Visual")]
    [SerializeField] private GameObject dragPreview;
    private Canvas canvas;
    private RectTransform dragPreviewRect;
    private UnitInteractionManager interactionManager;
    private MarketLogic marketLogic;
    private bool isDragging = false;
    private int unitPrice;

    void Awake()
    {
        canvas = GetComponentInParent<Canvas>();
        interactionManager = FindFirstObjectByType<UnitInteractionManager>();
        marketLogic = new MarketLogic();
    }

    void Start()
    {
        // Get price from MarketLogic
        if (marketLogic.marketPrices.ContainsKey(unitTag))
        {
            unitPrice = marketLogic.marketPrices[unitTag];
        }
        else
        {
            Debug.LogError($"Unit tag '{unitTag}' not found in MarketLogic!");
        }

        UpdateUI();
    }

    public void Initialize(string tag, Sprite icon, string displayName)
    {
        unitTag = tag;
        unitIcon = icon;

        // Get price from MarketLogic
        if (marketLogic.marketPrices.ContainsKey(tag))
        {
            unitPrice = marketLogic.marketPrices[tag];
        }

        if (nameText != null) nameText.text = displayName;

        UpdateUI();
    }

    private void UpdateUI()
    {
        if (iconImage != null && unitIcon != null)
            iconImage.sprite = unitIcon;

        if (priceText != null)
            priceText.text = $"${unitPrice}";

        if (nameText != null && string.IsNullOrEmpty(nameText.text))
            nameText.text = unitTag;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        // Check if player has enough gold
        if (GameManager.Instance.currentGold < unitPrice)
        {
            Debug.Log("Not enough gold!");
            return;
        }

        isDragging = true;

        // Create drag preview
        if (dragPreview == null)
        {
            dragPreview = new GameObject("DragPreview");
            dragPreview.transform.SetParent(canvas.transform, false);

            Image previewImage = dragPreview.AddComponent<Image>();
            previewImage.sprite = unitIcon;
            previewImage.raycastTarget = false;
            previewImage.color = new Color(1, 1, 1, 0.6f);

            dragPreviewRect = dragPreview.GetComponent<RectTransform>();
            dragPreviewRect.sizeDelta = new Vector2(80, 80);
        }

        dragPreview.SetActive(true);
        dragPreview.transform.SetAsLastSibling();

        // Notify the interaction manager
        if (interactionManager != null)
        {
            interactionManager.StartShopDrag(unitTag);
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isDragging) return;

        if (dragPreview != null && dragPreviewRect != null)
        {
            dragPreviewRect.position = eventData.position;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!isDragging) return;

        isDragging = false;

        if (dragPreview != null)
        {
            dragPreview.SetActive(false);
        }
    }

    void Update()
    {
        // Update button appearance based on affordability
        if (priceText != null)
        {
            bool canAfford = GameManager.Instance.currentGold >= unitPrice;
            priceText.color = canAfford ? Color.white : Color.red;
        }
    }
}