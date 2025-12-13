using UnityEngine;
using UnityEngine.EventSystems;

public class UnitInteractionManager : MonoBehaviour
{
    public LayerMask groundLayer;
    public LayerMask unitLayer;
    private GameObject draggingUnit;
    private bool isDraggingFromShop = false;
    private string currentUnitTag;

    void Update()
    {
        if (Input.GetMouseButtonDown(0)) HandleClick();
        if (Input.GetMouseButton(0) && draggingUnit != null) HandleDrag();
        if (Input.GetMouseButtonUp(0) && draggingUnit != null) HandleDrop();
    }

    // Called by UI Buttons (Event Trigger: Pointer Down)
    public void StartShopDrag(string unitTag)
    {
        int price = new MarketLogic().marketPrices[unitTag];
        if (GameManager.Instance.currentGold >= price)
        {
            currentUnitTag = unitTag;
            // Spawn a temporary visual unit from pool
            draggingUnit = ObjectPooler.Instance.SpawnFromPool(unitTag, GetMouseWorldPos());
            isDraggingFromShop = true;
        }
    }

    private void HandleClick()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 100f, unitLayer))
        {
            draggingUnit = hit.collider.gameObject;
            isDraggingFromShop = false;
        }
    }

    private void HandleDrag()
    {
        Vector3 pos = GetMouseWorldPos();
        draggingUnit.transform.position = pos;
    }

    private void HandleDrop()
    {
        // Check if dropped over UI (to sell)
        if (EventSystem.current.IsPointerOverGameObject())
        {
            if (!isDraggingFromShop) SellUnit(draggingUnit);
            else ObjectPooler.Instance.ReturnToPool(draggingUnit, currentUnitTag);
        }
        else
        {
            // Finalize purchase if from shop
            if (isDraggingFromShop)
            {
                int price = new MarketLogic().marketPrices[currentUnitTag];
                GameManager.Instance.currentGold -= price;
            }
            // Logic to snap to grid could go here
        }
        draggingUnit = null;
    }

    private void SellUnit(GameObject unitObj)
    {
        Person p = unitObj.GetComponent<Person>();
        GameManager.Instance.currentGold += p.GivenGold; // Or custom sell price
        ObjectPooler.Instance.ReturnToPool(unitObj, unitObj.tag);
    }

    private Vector3 GetMouseWorldPos()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 100f, groundLayer)) return hit.point;
        return draggingUnit != null ? draggingUnit.transform.position : Vector3.zero;
    }
}