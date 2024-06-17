using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SoundManagerScript : MonoBehaviour
{
    // music tracks
    [SerializeField] private AudioClip game_1_Quirkii;
    [SerializeField] private AudioClip game_2_Mana_Trail;
    
    [SerializeField] private AudioClip menu_1_Play_It_Cool;
    
    private AudioClip[] clips;
    
    private void Awake()
    {
        clips = new AudioClip[] { game_1_Quirkii, game_2_Mana_Trail, menu_1_Play_It_Cool };
    }
    private string[] clipNames = {"HeatleyBros - Quirkii", "HeatleyBros - Mana Trail", "HeatleyBros - Play It Cool"};
    private int currentClipIndex = 0;

    // settings page buttons (move to UI manager?)

    [SerializeField] private TMP_Text selectedTrackText;

    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider SFXSlider;

    [SerializeField] private AudioSource musicComponent;

    private float musicVolumeFromPref;
    private float SFXVolumeFromPref;

    // click sfx
    [SerializeField] private AudioSource clickSFX;

    public void Load()
    {
        currentClipIndex = PlayerPrefs.GetInt("SelectedTrack", 0);
        PlayClip(currentClipIndex);

        musicVolumeFromPref = PlayerPrefs.GetFloat("MusicVolume", 0.8f);
        musicComponent.volume = musicVolumeFromPref;
        //Debug.Log("Music volume: " + musicVolumeFromPref);

        SFXVolumeFromPref = PlayerPrefs.GetFloat("SFXVolume", 0.8f);
        //Debug.Log("SFX volume: " + SFXVolumeFromPref);
    }

    private void OnEnable()
    {
        Load();
        musicSlider.value = musicVolumeFromPref;
        SFXSlider.value = SFXVolumeFromPref;
        selectedTrackText.text = clipNames[currentClipIndex];
    }

    public void SetMusicVolume()
    {
        musicComponent.volume = musicSlider.value;
        PlayerPrefs.SetFloat("MusicVolume", musicSlider.value);
        //Debug.Log("Set music volume to: " + SFXVolumeFromPref);
    }

    public void SetSFXVolume()
    {
        PlayerPrefs.SetFloat("SFXVolume", SFXSlider.value);
        //Debug.Log("Set SFX volume to: " + SFXVolumeFromPref);
        SFXVolumeFromPref = SFXSlider.value;
    }

    public float GetMusicVolume()
    {
        return musicVolumeFromPref;
    }

    public float GetSFXVolume()
    {
        return SFXVolumeFromPref;
    }

    public void PlayNextClip()
    {
        currentClipIndex++;
        if (currentClipIndex >= clips.Length)
        {
            currentClipIndex = 0;
        }

        musicComponent.clip = clips[currentClipIndex];
        musicComponent.Play();
        
        selectedTrackText.text = clipNames[currentClipIndex];

        PlayerPrefs.SetInt("SelectedTrack", currentClipIndex);
    }

    private void PlayClip(int index)
    {
        musicComponent.clip = clips[index];
        musicComponent.Play();
    }

    public void PlayClickSFX()
    {
        clickSFX.volume = SFXVolumeFromPref * 0.5f;
        clickSFX.Play();
    }
}
