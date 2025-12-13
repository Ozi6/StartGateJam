using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static AugmentHandler;

public class AugmentCardUI : MonoBehaviour, IPointerClickHandler
{
    public TMP_Text titleText;
    public TMP_Text descriptionText;
    public Image icon;

    private Augment augment;
    private System.Action<Augment> onSelect;

    public void Setup(Augment aug, System.Action<Augment> selectCallback)
    {
        augment = aug;
        onSelect = selectCallback;

        titleText.text = aug.title;
        descriptionText.text = aug.description;
        if(icon != null)
            icon.sprite = aug.icon;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        onSelect?.Invoke(augment);
    }
}
