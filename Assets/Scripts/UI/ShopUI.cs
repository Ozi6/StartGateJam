using UnityEngine;
using System.Collections;

public class ShopUI : MonoBehaviour
{
    [SerializeField] private RectTransform shopPanel;
    [SerializeField] private float animationSpeed = 5f;

    private Vector2 hiddenPos;
    private Vector2 visiblePos;
    private bool isAnimating = false;

    void Awake()
    {
        visiblePos = shopPanel.anchoredPosition;
        hiddenPos = new Vector2(visiblePos.x, -visiblePos.y - 500f);
        shopPanel.anchoredPosition = hiddenPos;
    }

    public void ToggleShop(bool show)
    {
        StopAllCoroutines();
        if (show) StartCoroutine(ShowRoutine());
        else StartCoroutine(HideRoutine());
    }

    private IEnumerator ShowRoutine()
    {
        // "Go up, then a bit down when appearing"
        Vector2 peakPos = visiblePos + new Vector2(0, 30f);
        yield return MoveTo(peakPos);
        yield return MoveTo(visiblePos);
    }

    private IEnumerator HideRoutine()
    {
        // "Go up a bit then go down when disappearing"
        Vector2 bumpPos = shopPanel.anchoredPosition + new Vector2(0, 30f);
        yield return MoveTo(bumpPos);
        yield return MoveTo(hiddenPos);
    }

    private IEnumerator MoveTo(Vector2 target)
    {
        while (Vector2.Distance(shopPanel.anchoredPosition, target) > 0.1f)
        {
            shopPanel.anchoredPosition = Vector2.Lerp(shopPanel.anchoredPosition, target, Time.deltaTime * animationSpeed);
            yield return null;
        }
    }

    public void OnDeploymentButtonClicked()
    {
        GameManager.Instance.SetState(GameState.Deployment);
    }
}