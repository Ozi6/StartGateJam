using UnityEngine;
using System.Collections.Generic;

public class AudioManager : Singleton<AudioManager>
{
    [Header("Audio Sources")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("Audio Clips")]
    [SerializeField] private AudioClip[] musicTracks;
    [SerializeField] private AudioClip[] sfxClips;

    [Header("Settings")]
    [Range(0f, 1f)]
    [SerializeField] private float masterVolume = 1f;
    [Range(0f, 1f)]
    [SerializeField] private float musicVolume = 0.7f;
    [Range(0f, 1f)]
    [SerializeField] private float sfxVolume = 1f;

    private Dictionary<string, AudioClip> sfxDictionary = new Dictionary<string, AudioClip>();

    protected override void OnAwake()
    {
        if (musicSource == null)
        {
            GameObject musicObj = new GameObject("MusicSource");
            musicObj.transform.SetParent(transform);
            musicSource = musicObj.AddComponent<AudioSource>();
            musicSource.loop = true;
            musicSource.playOnAwake = false;
        }
        if (sfxSource == null)
        {
            GameObject sfxObj = new GameObject("SFXSource");
            sfxObj.transform.SetParent(transform);
            sfxSource = sfxObj.AddComponent<AudioSource>();
            sfxSource.playOnAwake = false;
        }
        foreach (var clip in sfxClips)
            if (clip != null && !sfxDictionary.ContainsKey(clip.name)
                sfxDictionary.Add(clip.name, clip);
        UpdateVolumes();
    }

    public void PlayMusic(int trackIndex)
    {
        if (trackIndex < 0 || trackIndex >= musicTracks.Length || musicTracks[trackIndex] == null)
            return;
        musicSource.clip = musicTracks[trackIndex];
        musicSource.Play();
    }

    public void PlayMusic(string trackName)
    {
        AudioClip clip = System.Array.Find(musicTracks, t => t != null && t.name == trackName);
        if (clip != null)
        {
            musicSource.clip = clip;
            musicSource.Play();
        }
    }

    public void StopMusic()
    {
        musicSource.Stop();
    }

    public void PauseMusic()
    {
        musicSource.Pause();
    }

    public void ResumeMusic()
    {
        musicSource.UnPause();
    }

    public void PlaySFX(string clipName, float volumeScale = 1f)
    {
        if (sfxDictionary.TryGetValue(clipName, out AudioClip clip))
            sfxSource.PlayOneShot(clip, volumeScale);
        else
            Debug.LogWarning($"SFX clip '{clipName}' not found!");
    }

    public void PlaySFX(AudioClip clip, float volumeScale = 1f)
    {
        if (clip != null)
            sfxSource.PlayOneShot(clip, volumeScale);
    }

    public void PlaySFXAtPoint(string clipName, Vector3 position, float volumeScale = 1f)
    {
        if (sfxDictionary.TryGetValue(clipName, out AudioClip clip))
            AudioSource.PlayClipAtPoint(clip, position, sfxVolume * masterVolume * volumeScale);
    }

    public void SetMasterVolume(float volume)
    {
        masterVolume = Mathf.Clamp01(volume);
        UpdateVolumes();
    }

    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        UpdateVolumes();
    }

    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        UpdateVolumes();
    }

    private void UpdateVolumes()
    {
        musicSource.volume = musicVolume * masterVolume;
        sfxSource.volume = sfxVolume * masterVolume;
    }

    public float GetMasterVolume() => masterVolume;
    public float GetMusicVolume() => musicVolume;
    public float GetSFXVolume() => sfxVolume;
}