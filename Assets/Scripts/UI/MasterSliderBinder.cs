using UnityEngine;
using UnityEngine.UI;

public class MasterSliderBinder : MonoBehaviour
{
    [SerializeField] private Slider slider;

    private void Start()
    {
        slider.value = AudioManager.Instance.GetMasterVolume();
        slider.onValueChanged.AddListener(AudioManager.Instance.SetMasterVolume);
    }
}