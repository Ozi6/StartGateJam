using UnityEngine;
using System.Collections.Generic;

public class ShopManager : MonoBehaviour
{
    [Header("Shop Setup")]
    [SerializeField] private GameObject buyButtonPrefab;
    [SerializeField] private Transform shopButtonContainer;

    [Header("Unit Icons")]
    [SerializeField] private List<UnitIconData> unitIcons = new List<UnitIconData>();

    private MarketLogic marketLogic;

    [System.Serializable]
    public class UnitIconData
    {
        public string unitTag;
        public Sprite icon;
        public string displayName;
    }

    void Start()
    {
        marketLogic = new MarketLogic();
        SetupShop();
    }

    private void SetupShop()
    {
        // Create a button for each unit in MarketLogic
        foreach (var kvp in marketLogic.marketPrices)
        {
            string unitTag = kvp.Key;

            // Find matching icon data
            UnitIconData iconData = unitIcons.Find(x => x.unitTag == unitTag);

            if (iconData != null)
            {
                CreateBuyButton(unitTag, iconData.icon, iconData.displayName);
            }
            else
            {
                Debug.LogWarning($"No icon data found for unit: {unitTag}");
            }
        }
    }

    private void CreateBuyButton(string unitTag, Sprite icon, string displayName)
    {
        if (buyButtonPrefab == null || shopButtonContainer == null)
        {
            Debug.LogError("BuyButton prefab or container not assigned!");
            return;
        }

        GameObject buttonObj = Instantiate(buyButtonPrefab, shopButtonContainer);
        BuyButton buyButton = buttonObj.GetComponent<BuyButton>();

        if (buyButton != null)
        {
            buyButton.Initialize(unitTag, icon, displayName);
        }
    }
}