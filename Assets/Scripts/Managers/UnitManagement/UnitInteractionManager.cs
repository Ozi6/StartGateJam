using UnityEngine;
using UnityEngine.EventSystems;

public class UnitInteractionManager : MonoBehaviour
{
    public LayerMask groundLayer;
    public LayerMask unitLayer;

    // Tracking drag vs click
    private GameObject potentialDragUnit; // Unit we clicked on, but haven't dragged yet
    private GameObject draggingUnit;      // Unit we are actively moving
    private Vector3 clickOrigin;          // Mouse pos when we first clicked
    private float dragThreshold = 10f;    // Pixels mouse must move to count as a drag

    private bool isDraggingFromShop = false;
    private string currentUnitTag;
    private int currentUnitPrice = 0;

    [Header("Visual Feedback")]
    [SerializeField] private Color validPlacementColor = Color.green;
    [SerializeField] private Color invalidPlacementColor = Color.red;

    private Renderer unitRenderer;
    private Color originalColor;
    private MarketLogic marketLogic;
    private float unitBottomOffset = 0f;

    void Awake()
    {
        marketLogic = new MarketLogic();
    }

    void Update()
    {
        // 1. Mouse Down: Identify potential target
        if (Input.GetMouseButtonDown(0) && !IsPointerOverUI())
        {
            PrepareInteraction();
        }

        // 2. Mouse Hold: Determine if dragging
        if (Input.GetMouseButton(0))
        {
            // If dragging from shop, we skip threshold check (it's always a drag)
            if (draggingUnit != null)
            {
                HandleDrag();
            }
            // If we have a target but haven't started dragging yet, check distance
            else if (potentialDragUnit != null)
            {
                float dist = Vector3.Distance(Input.mousePosition, clickOrigin);
                if (dist > dragThreshold)
                {
                    StartDraggingExistingUnit();
                }
            }
        }

        // 3. Mouse Up: Drop or Select
        if (Input.GetMouseButtonUp(0))
        {
            if (draggingUnit != null)
            {
                HandleDrop();
            }
            else if (potentialDragUnit != null)
            {
                // We pressed down and up without moving much -> It's a CLICK
                HandleClickSelection();
            }

            // Cleanup
            potentialDragUnit = null;
        }
    }

    public void StartShopDrag(string unitTag)
    {
        // Close upgrade panel if buying new unit
        if (UIManager.Instance) UIManager.Instance.CloseUpgradePanel();

        // Get price from MarketLogic
        if (!marketLogic.marketPrices.ContainsKey(unitTag)) return;
        int price = marketLogic.marketPrices[unitTag];

        if (GameManager.Instance.playersTeam.Count >= GameManager.Instance.CurrentWaveConfig.maxPlaceableUnits)
        {
            Debug.Log("Maximum placeable units reached!");
            return;
        }

        if (GameManager.Instance.currentGold >= price)
        {
            currentUnitTag = unitTag;
            currentUnitPrice = price;
            Vector3 spawnPos = GetMouseWorldPos();
            draggingUnit = ObjectPooler.Instance.SpawnFromPool(unitTag, spawnPos);

            if (draggingUnit != null)
            {
                isDraggingFromShop = true;
                SetupVisuals(draggingUnit);
                CalculateBottomOffset();
            }
        }
        else
        {
            Debug.Log($"Insufficient gold!");
        }
    }

    private void PrepareInteraction()
    {
        if (GameManager.Instance.CurrentState != GameState.Shopping)
            return;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 100f, unitLayer))
        {
            potentialDragUnit = hit.collider.gameObject;
            clickOrigin = Input.mousePosition;
        }
        else
        {
            // Clicked on empty ground -> Close panels
            if (UIManager.Instance) UIManager.Instance.CloseUpgradePanel();
        }
    }

    private void StartDraggingExistingUnit()
    {
        // Threshold passed, convert potential unit to dragging unit
        draggingUnit = potentialDragUnit;
        isDraggingFromShop = false;

        SetupVisuals(draggingUnit);
        CalculateBottomOffset();

        // Close UI if we start moving a unit
        if (UIManager.Instance) UIManager.Instance.CloseUpgradePanel();
    }

    private void HandleClickSelection()
    {
        // Logic for simply clicking a unit (Selecting it)
        if (potentialDragUnit != null && UIManager.Instance != null)
        {
            UIManager.Instance.OpenUpgradePanel(potentialDragUnit);
        }
    }

    private void HandleDrag()
    {
        Vector3 pos = GetMouseWorldPos();
        draggingUnit.transform.position = pos;

        if (isDraggingFromShop && unitRenderer != null)
        {
            bool isOverUI = IsPointerOverUI();
            unitRenderer.material.color = isOverUI ? invalidPlacementColor : validPlacementColor;
        }
    }

    private void HandleDrop()
    {
        bool isOverUI = IsPointerOverUI();

        if (unitRenderer != null)
        {
            unitRenderer.material.color = originalColor;
        }

        if (isOverUI)
        {
            // Dropped on UI
            if (!isDraggingFromShop)
            {
                SellUnit(draggingUnit);
                // Also close upgrade panel if we sell the selected unit
                if (UIManager.Instance) UIManager.Instance.CloseUpgradePanel();
            }
            else
            {
                ObjectPooler.Instance.ReturnToPool(draggingUnit, currentUnitTag);
            }
        }
        else
        {
            // Dropped on Game Area
            if (isDraggingFromShop)
            {
                GameManager.Instance.currentGold -= currentUnitPrice;
                Person person = draggingUnit.GetComponent<Person>();
                if (person != null)
                {
                    GameManager.Instance.playersTeam.Add(person);
                    person.SetFriendly(true);
                }

                // Immediately select the newly bought unit? (Optional)
                // if(UIManager.Instance) UIManager.Instance.OpenUpgradePanel(draggingUnit);
            }
        }

        GameManager.Instance.UpdateUnitCountDisplay();

        // Reset state
        draggingUnit = null;
        isDraggingFromShop = false;
        currentUnitTag = null;
        currentUnitPrice = 0;
        unitRenderer = null;
        unitBottomOffset = 0f;
    }

    private void SetupVisuals(GameObject unit)
    {
        unitRenderer = unit.GetComponent<Renderer>();
        if (unitRenderer != null)
        {
            originalColor = unitRenderer.material.color;
        }
    }

    private void SellUnit(GameObject unitObj)
    {
        Person p = unitObj.GetComponent<Person>();
        if (p != null)
        {
            marketLogic.SellUnit(p);
            int refundAmount = p.GivenGold;
            GameManager.Instance.currentGold += refundAmount;
            GameManager.Instance.playersTeam.Remove(p);
            p.isFriendly = false;
        }
        ObjectPooler.Instance.ReturnToPool(unitObj, unitObj.tag);
        GameManager.Instance.UpdateUnitCountDisplay();
    }

    private void CalculateBottomOffset()
    {
        if (draggingUnit == null) return;
        Collider col = draggingUnit.GetComponent<Collider>();
        if (col != null)
        {
            unitBottomOffset = col.bounds.extents.y;
            return;
        }
        Renderer rend = draggingUnit.GetComponent<Renderer>();
        if (rend != null)
        {
            unitBottomOffset = rend.bounds.extents.y;
            return;
        }
        unitBottomOffset = 0f;
    }

    private Vector3 GetMouseWorldPos()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 100f, groundLayer))
        {
            Vector3 groundPos = hit.point;
            groundPos.y += unitBottomOffset;
            return groundPos;
        }
        return draggingUnit != null ? draggingUnit.transform.position : Vector3.zero;
    }

    private bool IsPointerOverUI()
    {
        return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
    }
}