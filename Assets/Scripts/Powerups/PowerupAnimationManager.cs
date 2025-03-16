using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PowerupAnimationManager : MonoBehaviour
{
    // self
    public static PowerupAnimationManager Instance;

    [SerializeField] private GameObject floatingIconPrefab;

    [SerializeField] private GameObject popupEffectIconObject;
    [SerializeField] private Image popupEffectIcon;
    [SerializeField] private GameObject popupEffectTextObject;
    [SerializeField] private TMP_Text popupEffectText;

    Queue<int> PowerupPopupEffectAnimationQueue = new Queue<int>();

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(Instance);
    }
    public void ClearPowerupPopupEffectAnimationQueue() { PowerupPopupEffectAnimationQueue.Clear(); }

    public void AddPowerupPopupEffectAnimationToQueue(int index)
    {
        PowerupPopupEffectAnimationQueue.Enqueue(index);
        if (PowerupPopupEffectAnimationQueue.Count == 1)
        {
            PlayNextPowerupPopupEffectAnimationInQueue();
        }
    }

    public void PlayNextPowerupPopupEffectAnimationInQueue()
    {
        if (PowerupPopupEffectAnimationQueue.Count <= 0) { return; }
        float speedMultiplier = (PowerupPopupEffectAnimationQueue.Count - 1) / 3 + 1;
        PlayPowerupPopupEffectAnimation(PowerupPopupEffectAnimationQueue.Peek(), speedMultiplier);
    }

    public void FinishCurrentPowerupPopupEffectAnimationInQueue()
    {
        if (PowerupPopupEffectAnimationQueue.Count <= 0) { return; }
        PowerupPopupEffectAnimationQueue.Dequeue();
        if (PowerupPopupEffectAnimationQueue.Count >= 1)
        {
            PlayNextPowerupPopupEffectAnimationInQueue();
        }
    }

    // this is the large icon and text effect than shows when a card is played
    public void PlayPowerupPopupEffectAnimation(int index, float speedMultiplier)
    {
        popupEffectIcon.sprite = PowerupManager.Instance.powerupIcons[index];
        popupEffectText.text = PowerupManager.Instance.powerupTexts[index];

        LeanTween.cancel(popupEffectIconObject);
        LeanTween.cancel(popupEffectTextObject);

        popupEffectIconObject.transform.localScale = new Vector3(0f, 0f, 0f);
        popupEffectTextObject.transform.localScale = new Vector3(0f, 0f, 0f);

        float duration = 0.5f / speedMultiplier;

        LeanTween.scale(popupEffectIconObject, new Vector3(1f, 1f, 1f), duration).setEase(LeanTweenType.easeOutElastic).setDelay(0.01f);
        LeanTween.scale(popupEffectTextObject, new Vector3(1f, 1f, 1f), duration).setEase(LeanTweenType.easeOutElastic).setDelay(0.2f);

        LeanTween.scale(popupEffectIconObject, new Vector3(0f, 0f, 0f), duration).setEase(LeanTweenType.easeInElastic).setDelay(duration * 3 + 0.01f);
        LeanTween.scale(popupEffectTextObject, new Vector3(0f, 0f, 0f), duration).setEase(LeanTweenType.easeInElastic).setDelay(duration * 3 + 0.2f).setOnComplete(FinishCurrentPowerupPopupEffectAnimationInQueue);
    }


    // this is the small icon effect than shows at a pucks location when an effect in activated
    public void PlayPowerupActivationAnimation(int index, Vector3 position)
    {
        Sprite icon = PowerupManager.Instance.powerupIcons[index];

        GameObject floatingIcon = Instantiate(floatingIconPrefab, position, Quaternion.identity);
        SpriteRenderer sr = floatingIcon.GetComponent<SpriteRenderer>();
        sr.sprite = icon;
        sr.color = UIManagerScript.Instance.GetDarkMode() ? Color.white : Color.black;


        floatingIcon.transform.localScale = new Vector3(0f, 0f, 0f);

        float duration = 0.5f;

        LeanTween.scale(floatingIcon, new Vector3(0.75f, 0.75f, 0.75f), duration).setEase(LeanTweenType.easeOutElastic).setDelay(0.01f);
        LeanTween.scale(floatingIcon, new Vector3(0f, 0f, 0f), duration).setEase(LeanTweenType.easeInElastic).setDelay(duration * 3 + 0.01f).setDestroyOnComplete(true);
    }
}
