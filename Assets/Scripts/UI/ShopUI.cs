using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using TMPro;

public class ShopUI : MonoBehaviour
{
    [SerializeField] private RectTransform shopPanel;
    [SerializeField] private float animationSpeed = 5f;
    [SerializeField] private float hiddenOffset = 500f;

    [Header("Warning Text")]
    [SerializeField] private TMP_Text text;
    [SerializeField] private Color alarmColor = Color.red;
    [SerializeField] private float alarmInterval = 0.15f;
    [SerializeField] private int alarmFlashes = 4;

    private Vector2 hiddenPos;
    private Vector2 visiblePos;
    private Coroutine currentRoutine;
    private Coroutine alarmRoutine;
    private Color originalTextColor;

    private void Awake()
    {
        visiblePos = shopPanel.anchoredPosition;
        hiddenPos = visiblePos + Vector2.down * hiddenOffset;

        shopPanel.anchoredPosition = hiddenPos;

        if (text != null)
            originalTextColor = text.color;
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

    // =============================
    // DEPLOY BUTTON
    // =============================
    public void OnDeploymentButtonClicked()
    {
        if (GameManager.Instance.playersTeam.Count > 0)
        {
            EventSystem.current.SetSelectedGameObject(null);
            GameManager.Instance.SetState(GameState.Deployment);
        }
        else
        {
            TriggerAlarm();
        }
    }

    // =============================
    // TEXT ALARM
    // =============================
    private void TriggerAlarm()
    {
        if (text == null)
            return;

        if (alarmRoutine != null)
            StopCoroutine(alarmRoutine);

        alarmRoutine = StartCoroutine(AlarmRoutine());
    }

    private IEnumerator AlarmRoutine()
    {
        for (int i = 0; i < alarmFlashes; i++)
        {
            text.color = alarmColor;
            yield return new WaitForSeconds(alarmInterval);

            text.color = originalTextColor;
            yield return new WaitForSeconds(alarmInterval);
        }

        text.color = originalTextColor;
        alarmRoutine = null;
    }
}
