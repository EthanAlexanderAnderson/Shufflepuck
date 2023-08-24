using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AudioButtonScript : MonoBehaviour
{

    public Button volumeButton;
    public Sprite volumeEnabled;
    public Sprite volumeDisabled;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void toggleSound()
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
