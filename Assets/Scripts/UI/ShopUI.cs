using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

public class ShopUI : MonoBehaviour
{
    [SerializeField] private RectTransform shopPanel;
    [SerializeField] private float animationSpeed = 5f;
    [SerializeField] private float hiddenOffset = 500f;

    private Vector2 hiddenPos;
    private Vector2 visiblePos;
    private Coroutine currentRoutine;

    private void Awake()
    {
        visiblePos = shopPanel.anchoredPosition;
        hiddenPos = visiblePos + Vector2.down * hiddenOffset;

        shopPanel.anchoredPosition = hiddenPos;
    }

    public void ToggleShop(bool show)
    {
        if (!gameObject.activeInHierarchy)
            return;

        if (currentRoutine != null)
            StopCoroutine(currentRoutine);

        currentRoutine = StartCoroutine(show ? ShowRoutine() : HideRoutine());
    }

    private IEnumerator ShowRoutine()
    {
        yield return MoveTo(visiblePos);
        currentRoutine = null;
    }

    private IEnumerator HideRoutine()
    {
        yield return MoveTo(hiddenPos);
        currentRoutine = null;
    }

    private IEnumerator MoveTo(Vector2 target)
    {
        while (Vector2.Distance(shopPanel.anchoredPosition, target) > 0.5f)
        {
            shopPanel.anchoredPosition = Vector2.Lerp(
                shopPanel.anchoredPosition,
                target,
                Time.deltaTime * animationSpeed
            );

            yield return null;
        }

        shopPanel.anchoredPosition = target;
    }

    public void OnDeploymentButtonClicked()
    {
        EventSystem.current.SetSelectedGameObject(null);
        GameManager.Instance.SetState(GameState.Deployment);
    }
}
