using UnityEngine;
using UnityEngine.UI;

public class AudioButtonScript : MonoBehaviour
{
    public Button volumeButton;
    public Sprite volumeEnabled;
    public Sprite volumeDisabled;

    public void ToggleSound()
    {
        if (AudioListener.volume > 0)
        {
            AudioListener.volume = 0;
            volumeButton.image.sprite = volumeDisabled;
        }
        else
        {
            AudioListener.volume = 1;
            volumeButton.image.sprite = volumeEnabled;
        }
    }
}
