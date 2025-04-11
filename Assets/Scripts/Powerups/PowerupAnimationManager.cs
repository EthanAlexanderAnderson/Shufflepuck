using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PowerupAnimationManager : MonoBehaviour
{
    // self
    public static PowerupAnimationManager Instance;

    [SerializeField] private GameObject floatingIconPrefab;

    [SerializeField] private GameObject powerupPopupPrefab;

    [SerializeField] private GameObject popupEffectParent;

    private GameObject popupEffectIconObject;
    private GameObject popupEffectIconOutlineObject;
    private GameObject popupEffectTextObject;
    private GameObject popupEffectRarityObject;

    Queue<(bool, int)> PowerupPopupEffectAnimationQueue = new();

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(Instance);
    }
    public void ClearPowerupPopupEffectAnimationQueue() { PowerupPopupEffectAnimationQueue.Clear(); }

    public void AddPowerupPopupEffectAnimationToQueue(bool isPlayer, int encodedCard)
    {
        PowerupPopupEffectAnimationQueue.Enqueue((isPlayer, encodedCard));
        if (PowerupPopupEffectAnimationQueue.Count == 1)
        {
            Invoke("PlayNextPowerupPopupEffectAnimationInQueue", 0.1f);
        }
    }

    public void PlayNextPowerupPopupEffectAnimationInQueue()
    {
        if (PowerupPopupEffectAnimationQueue.Count <= 0) { return; }
        float speedMultiplier = Mathf.Min(((PowerupPopupEffectAnimationQueue.Count - 1f) / 4f) + 1f, 3);

        PlayPowerupPopupEffectAnimation(PowerupPopupEffectAnimationQueue.Peek().Item1, PowerupPopupEffectAnimationQueue.Peek().Item2, speedMultiplier);
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
    public void PlayPowerupPopupEffectAnimation(bool isPlayer, int encodedCard, float speedMultiplier)
    {
        GameObject powerupPopupObject = Instantiate(powerupPopupPrefab, popupEffectParent.transform);
        PowerupPopupPrefabScript powerupPopupScript = powerupPopupObject.GetComponent<PowerupPopupPrefabScript>();
        var decodedCard = PowerupCardData.DecodeCard(encodedCard);
        powerupPopupScript.InitializePowerupPopup(decodedCard.cardIndex, decodedCard.rank, decodedCard.holo);
        (popupEffectIconObject, popupEffectIconOutlineObject, popupEffectTextObject, popupEffectRarityObject) = powerupPopupScript.GetObjects();

        LeanTween.cancel(popupEffectIconObject);
        LeanTween.cancel(popupEffectTextObject);
        LeanTween.cancel(powerupPopupObject);

        int side = isPlayer ? -1 : 1;
        powerupPopupObject.transform.localPosition = new Vector3(300f * side, 0, 0);

        float duration = (decodedCard.rank > 0 || decodedCard.holo) ? 0.65f / speedMultiplier : 0.45f / speedMultiplier;

        LeanTween.moveLocalX(powerupPopupObject, 0, duration * 1.5f).setEase(LeanTweenType.easeOutCubic).setDelay(0.01f);
        LeanTween.scale(popupEffectIconObject, new Vector3(1f, 1f, 1f), duration).setEase(LeanTweenType.easeOutElastic).setDelay(0.01f);
        LeanTween.scale(popupEffectIconOutlineObject, new Vector3(1f, 1f, 1f), duration).setEase(LeanTweenType.easeOutElastic).setDelay(0.01f);
        LeanTween.scale(popupEffectTextObject, new Vector3(1f, 1f, 1f), duration).setEase(LeanTweenType.easeOutElastic).setDelay(0.21f);
        LeanTween.scale(popupEffectRarityObject, new Vector3(1f, 1f, 1f), duration).setEase(LeanTweenType.easeOutElastic).setDelay(0.41f);

        LeanTween.moveLocalX(powerupPopupObject, 300f * side, duration * 1.6f).setEase(LeanTweenType.easeInCubic).setDelay(duration * 3);
        LeanTween.scale(popupEffectIconObject, new Vector3(0f, 0f, 0f), duration).setEase(LeanTweenType.easeInElastic).setDelay(duration * 3 + 0.01f);
        LeanTween.scale(popupEffectIconOutlineObject, new Vector3(0f, 0f, 0f), duration).setEase(LeanTweenType.easeInElastic).setDelay(duration * 3 + 0.01f);
        LeanTween.scale(popupEffectTextObject, new Vector3(0f, 0f, 0f), duration).setEase(LeanTweenType.easeInElastic).setDelay(duration * 3 + 0.21f);
        LeanTween.scale(popupEffectRarityObject, new Vector3(0f, 0f, 0f), duration).setEase(LeanTweenType.easeInElastic).setDelay(duration * 3 + 0.41f).setOnComplete(FinishCurrentPowerupPopupEffectAnimationInQueue);

        Destroy(powerupPopupObject, 3.5f);
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
