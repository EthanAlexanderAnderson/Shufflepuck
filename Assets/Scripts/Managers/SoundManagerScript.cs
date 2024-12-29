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

    private AudioClip[] clips;

    void Awake()
    {
        clips = new AudioClip[] { music_Shufflepuck, game_1_Quirkii, game_2_Mana_Trail, menu_1_Play_It_Cool };

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

    private string[] clipNames = { "Ethan Larose - Shufflepuck", "HeatleyBros - Quirkii", "HeatleyBros - Mana Trail", "HeatleyBros - Play It Cool" };
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
    // win sfx
    [SerializeField] private AudioSource winSFX;
    private bool isInitialized = false;

    public void Load()
    {
        currentClipIndex = PlayerPrefs.GetInt("SelectedTrack", 0);
        PlayClip(currentClipIndex);

        musicVolumeFromPref = PlayerPrefs.GetFloat("MusicVolume", 0.5f);
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
            yield return null;
        }

        musicComponent.volume = targetVolume; // Ensure it ends at the correct volume
    }
}
