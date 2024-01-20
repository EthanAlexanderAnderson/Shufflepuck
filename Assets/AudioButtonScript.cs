// Script for AudioButton GameObject. Toggles game sound

using UnityEngine;
using UnityEngine.UI;

public class AudioButtonScript : MonoBehaviour
{
    public Button volumeButton;
    public Sprite volumeEnabled;
    public Sprite volumeDisabled;

    public AudioSource musicComponent;
    [SerializeField] private bool music;

    public void ToggleMusic()
    {
        if (music)
        {
            musicComponent.volume = 0;
            volumeButton.image.sprite = volumeDisabled;
            music = false;
            PlayerPrefs.SetInt("music", 2); // use 2 for off, because default is 0
        }
        else
        {
            musicComponent.volume = 0.4f;
            volumeButton.image.sprite = volumeEnabled;
            PlayerPrefs.SetInt("music", 1);
            music = true;
        }
    }

    [SerializeField] private int SFX;
    public void ToggleSFX()
    {
        if (SFX > 0)
        {
            volumeButton.image.sprite = volumeDisabled;
            SFX = 0;
            PlayerPrefs.SetInt("SFX", 2); // use 2 for off, because default is 0
        }
        else
        {
            volumeButton.image.sprite = volumeEnabled;
            SFX = 1;
            PlayerPrefs.SetInt("SFX", 1);
        }
    }

    // this returns 0 if muted, 1 if not muted
    public int GetSFX()
    {
        return SFX;
    }
}
