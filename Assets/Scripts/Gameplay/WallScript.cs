using UnityEngine;

public class WallScript : MonoBehaviour
{
    // self
    public static WallScript Instance;

    [SerializeField] SpriteRenderer SR;
    [SerializeField] BoxCollider2D BC;
    [SerializeField] ParticleSystem PS;
    [SerializeField] AudioSource SFX;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(Instance);
    }

    public void WallEnabled(bool enabled)
    {
        if (!enabled && SR.enabled)
        {
            PS.Play();
            SFX.volume = SoundManagerScript.Instance.GetSFXVolume();
            SFX.Play();
        }
        SR.enabled = enabled;
        BC.enabled = enabled;
    }
}
