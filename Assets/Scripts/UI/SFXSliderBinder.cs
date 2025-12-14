using UnityEngine;
using UnityEngine.UI;

public class SFXSliderBinder : MonoBehaviour
{
    [SerializeField] private Slider slider;

    private void Start()
    {
        slider.value = AudioManager.Instance.GetSFXVolume();
        slider.onValueChanged.AddListener(AudioManager.Instance.SetSFXVolume);
    }
}
