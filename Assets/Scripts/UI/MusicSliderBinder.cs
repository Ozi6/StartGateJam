using UnityEngine;
using UnityEngine.UI;

public class MusicSliderBinder : MonoBehaviour
{
    [SerializeField] private Slider slider;

    private void Start()
    {
        slider.value = AudioManager.Instance.GetMusicVolume();
        slider.onValueChanged.AddListener(AudioManager.Instance.SetMusicVolume);
    }
}