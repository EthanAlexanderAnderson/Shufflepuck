using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class SoundManagerScript : MonoBehaviour
{
    public static SoundManagerScript Instance;

    // music tracks
    [SerializeField] private AudioClip music_Shufflepuck;
    [SerializeField] private AudioClip game_1_Quirkii;
    [SerializeField] private AudioClip game_2_Mana_Trail;

    [SerializeField] private AudioClip menu_1_Play_It_Cool;

    private AudioClip[] tracks;

    private string[] clipNames = { "Ethan Larose - Shufflepuck", "HeatleyBros - Quirkii", "HeatleyBros - Mana Trail", "HeatleyBros - Play It Cool" };
    private int currentClipIndex = 0;

    // settings page buttons (move to UI manager?)

    [SerializeField] private TMP_Text selectedTrackText;

    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider SFXSlider;

    [SerializeField] private AudioSource musicComponent;

    private float musicVolumeFromPref;
    private float SFXVolumeFromPref;

    // win sfx
    [SerializeField] private AudioSource winSFX;
    private bool isInitialized = false;

    // click sfx
    private AudioSource[] clicks;
    [SerializeField] private AudioSource clickSFX1;
    [SerializeField] private AudioSource clickSFX2;
    [SerializeField] private AudioSource clickSFX3;
    [SerializeField] private AudioSource clickSFX4;
    [SerializeField] private AudioSource clickSFXUp;
    [SerializeField] private AudioSource clickSFXDown;

    void Awake()
    {
        tracks = new AudioClip[] { music_Shufflepuck, game_1_Quirkii, game_2_Mana_Trail, menu_1_Play_It_Cool };
        clicks = new AudioSource[] { clickSFX1, clickSFX2, clickSFX3, clickSFX4, clickSFXUp, clickSFXDown };

        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        Load();
        StartCoroutine(FadeInMusic());
    }

    public void Load()
    {
        currentClipIndex = PlayerPrefs.GetInt("SelectedTrack", 0);
        PlayClip(currentClipIndex);

        musicVolumeFromPref = PlayerPrefs.GetFloat("MusicVolume", 0.2f);
        musicComponent.volume = 0f; // Start at 0 volume for fade-in

        SFXVolumeFromPref = PlayerPrefs.GetFloat("SFXVolume", 1.0f);
    }

    private void OnEnable()
    {
        Load();
        musicSlider.value = musicVolumeFromPref;
        SFXSlider.value = SFXVolumeFromPref;
        selectedTrackText.text = clipNames[currentClipIndex];
        isInitialized = true;
    }

    public void SetMusicVolume()
    {
        float roundedMusic = Mathf.Round(musicSlider.value * 100f) / 100f;
        musicComponent.volume = roundedMusic;
        PlayerPrefs.SetFloat("MusicVolume", roundedMusic);
    }

    public void SetSFXVolume()
    {
        float roundedSFX = Mathf.Round(SFXSlider.value * 100f) / 100f;
        PlayerPrefs.SetFloat("SFXVolume", roundedSFX);
        SFXVolumeFromPref = SFXSlider.value;
        // Play sound effect only if initialized
        if (isInitialized)
        {
            PlayWinSFX();
        }
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
        if (currentClipIndex >= tracks.Length)
        {
            currentClipIndex = 0;
        }

        musicComponent.clip = tracks[currentClipIndex];
        musicComponent.Play();

        selectedTrackText.text = clipNames[currentClipIndex];

        PlayerPrefs.SetInt("SelectedTrack", currentClipIndex);
    }

    private void PlayClip(int index)
    {
        musicComponent.clip = tracks[index];
        musicComponent.Play();
    }

    public void PlayClickSFX(int i = 0)
    {
        // very important this doesn't throw an error and stop the rest of whatever the button is trying to do
        try
        {
            clicks[i].mute = false;
            clicks[i].pitch = 1f;
            clicks[i].volume = SFXVolumeFromPref;
            clicks[i].Play();
        }
        catch (System.Exception e){ Debug.LogError(e); }
    }

    public void PlayClickSFXAlterPitch(int i = 0, float pitch = 1f)
    {
        // very important this doesn't throw an error and stop the rest of whatever the button is trying to do
        try
        {
            clicks[i].mute = false;
            clicks[i].pitch = pitch;
            clicks[i].volume = SFXVolumeFromPref;
            clicks[i].Play();
        }
        catch (System.Exception e) { Debug.LogError(e); }
    }

    public void PlayWinSFX()
    {
        winSFX.volume = SFXVolumeFromPref * 0.5f;
        winSFX.Play();
    }

    private IEnumerator FadeInMusic()
    {
        float targetVolume = musicVolumeFromPref;
        float fadeDuration = 3f; // Duration of the fade-in in seconds
        float startVolume = 0f;
        float elapsedTime = 0f;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            musicComponent.volume = Mathf.Lerp(startVolume, targetVolume, elapsedTime / fadeDuration);
            if ( musicComponent.volume > 0.01) { musicComponent.mute = false; } // this is weird but it prevents the 0.1 seconds of full volume music on startup
            yield return null;
        }

        musicComponent.volume = targetVolume; // Ensure it ends at the correct volume
    }
}
