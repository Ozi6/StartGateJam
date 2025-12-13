using UnityEngine;
using UnityEngine.EventSystems;

public class UnitInteractionManager : MonoBehaviour
{
    public LayerMask groundLayer;
    public LayerMask unitLayer;

    private GameObject draggingUnit;
    private bool isDraggingFromShop = false;
    private string currentUnitTag;
    private int currentUnitPrice = 0;

    [Header("Visual Feedback")]
    [SerializeField] private Color validPlacementColor = Color.green;
    [SerializeField] private Color invalidPlacementColor = Color.red;

    private Renderer unitRenderer;
    private Color originalColor;
    private MarketLogic marketLogic;

    void Awake()
    {
        marketLogic = new MarketLogic();
    }

    void Update()
    {
        // Only handle manual unit dragging (not shop dragging)
        if (Input.GetMouseButtonDown(0) && !IsPointerOverUI())
        {
            HandleClick();
        }

        if (Input.GetMouseButton(0) && draggingUnit != null)
        {
            HandleDrag();
        }

        if (Input.GetMouseButtonUp(0) && draggingUnit != null)
        {
            HandleDrop();
        }
    }

    public void StartShopDrag(string unitTag)
    {
        // Get price from MarketLogic
        if (!marketLogic.marketPrices.ContainsKey(unitTag))
        {
            Debug.LogError($"Unit tag '{unitTag}' not found in market prices!");
            return;
        }

        int price = marketLogic.marketPrices[unitTag];

        if (GameManager.Instance.currentGold >= price)
        {
            currentUnitTag = unitTag;
            currentUnitPrice = price;

            // Spawn the unit at mouse position
            Vector3 spawnPos = GetMouseWorldPos();
            draggingUnit = ObjectPooler.Instance.SpawnFromPool(unitTag, spawnPos);

            if (draggingUnit != null)
            {
                isDraggingFromShop = true;

                // Store original color for visual feedback
                unitRenderer = draggingUnit.GetComponent<Renderer>();
                if (unitRenderer != null)
                {
                    originalColor = unitRenderer.material.color;
                }
            }
        }
        else
        {
            Debug.Log($"Insufficient gold! Need {price}, have {GameManager.Instance.currentGold}");
        }
    }

    private void HandleClick()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 100f, unitLayer))
        {
            draggingUnit = hit.collider.gameObject;
            isDraggingFromShop = false;

            unitRenderer = draggingUnit.GetComponent<Renderer>();
            if (unitRenderer != null)
            {
                originalColor = unitRenderer.material.color;
            }
        }
    }

    private void HandleDrag()
    {
        Vector3 pos = GetMouseWorldPos();
        draggingUnit.transform.position = pos;

        // Visual feedback for placement validity
        if (isDraggingFromShop && unitRenderer != null)
        {
            bool isOverUI = IsPointerOverUI();
            unitRenderer.material.color = isOverUI ? invalidPlacementColor : validPlacementColor;
        }
    }

    private void HandleDrop()
    {
        bool isOverUI = IsPointerOverUI();

        // Restore original color
        if (unitRenderer != null)
        {
            unitRenderer.material.color = originalColor;
        }

        if (isOverUI)
        {
            // Dropping back on UI - cancel purchase or sell unit
            if (!isDraggingFromShop)
            {
                SellUnit(draggingUnit);
            }
            else
            {
                // Return to pool without charging
                ObjectPooler.Instance.ReturnToPool(draggingUnit, currentUnitTag);
            }
        }
        else
        {
            // Dropping on valid game area
            if (isDraggingFromShop)
            {
                // Deduct gold and finalize purchase
                GameManager.Instance.currentGold -= currentUnitPrice;

                // Add to player's team
                Person person = draggingUnit.GetComponent<Person>();
                if (person != null)
                {
                    GameManager.Instance.playersTeam.Add(person);
                }

                Debug.Log($"Unit purchased! Gold remaining: {GameManager.Instance.currentGold}");
            }
            // If not from shop, just repositioning existing unit (no action needed)
        }

        // Reset state
        draggingUnit = null;
        isDraggingFromShop = false;
        currentUnitTag = null;
        currentUnitPrice = 0;
        unitRenderer = null;
    }

    private void SellUnit(GameObject unitObj)
    {
        Person p = unitObj.GetComponent<Person>();
        if (p != null)
        {
            // Use MarketLogic to handle selling
            marketLogic.SellUnit(p);

            int refundAmount = p.GivenGold;
            GameManager.Instance.currentGold += refundAmount;

            // Remove from player's team
            GameManager.Instance.playersTeam.Remove(p);

            Debug.Log($"Unit sold for {refundAmount} gold!");
        }

        ObjectPooler.Instance.ReturnToPool(unitObj, unitObj.tag);
    }

    private Vector3 GetMouseWorldPos()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 100f, groundLayer))
        {
            return hit.point;
        }

        return draggingUnit != null ? draggingUnit.transform.position : Vector3.zero;
    }

    private bool IsPointerOverUI()
    {
        return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
    }
}