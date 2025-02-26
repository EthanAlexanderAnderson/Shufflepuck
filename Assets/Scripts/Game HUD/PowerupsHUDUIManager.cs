using System;
using UnityEngine;
using UnityEngine.UI;

public class PowerupsHUDUIManager : MonoBehaviour
{
    // self
    public static PowerupsHUDUIManager Instance;

    [SerializeField] private GameObject powerupsMenu;

    [SerializeField] private GameObject powerupButtonObject1;
    [SerializeField] private GameObject powerupButtonObject2;
    [SerializeField] private GameObject powerupButtonObject3;
    [SerializeField] private GameObject continueButtonObject;
    private GameObject[] powerupButtonObjects;

    private float startXLocalPos = -1920;
    private float playerEndXLocalPos = 0;

    int cardsInHand = -1; // minus one because we include continue button

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(Instance);

        if (-(Screen.width * 1.2f) < startXLocalPos) { startXLocalPos = -(Screen.width * 1.2f); }
        powerupButtonObjects = new GameObject[] { powerupButtonObject1, powerupButtonObject2, powerupButtonObject3, continueButtonObject };
    }

    void OnEnable()
    {
        Reset();
        for (int i = 0; i < powerupButtonObjects.Length; i++)
        {
            LeanTween.cancel(powerupButtonObjects[i]);
            LeanTween.moveLocalX(powerupButtonObjects[i], playerEndXLocalPos, 1f).setEase(LeanTweenType.easeOutElastic).setDelay((0.2f * i) + 0.01f);
            cardsInHand += powerupButtonObjects[i].activeInHierarchy ? 1 : 0;
        }
        if (cardsInHand > 0)
        {
            AlphaHelper(true); // fade in menu if we have cards in hand
            disablingMenuAlreadyInProgress = false;
        }
        else
        {
            DisableMenu(); // disable menu if we have no cards in hand
        }
    }

    public void UsePowerup(int index, int powerupID)
    {
        for (int i = 0; i < powerupButtonObjects.Length; i++)
        {
            // if paid 2 discard cost, discard the other 2 cards
            if (Array.Exists(PowerupManager.Instance.GetCost2Discard(), x => x == powerupID) && i != index && i != 3)
            {
                cardsInHand--;
                LeanTween.cancel(powerupButtonObjects[i]);
                powerupButtonObjects[i].GetComponent<Button>().onClick.RemoveAllListeners();
                var tempindex = i;
                LeanTween.moveLocalX(powerupButtonObjects[i], -startXLocalPos, 0.5f).setEase(LeanTweenType.easeInBack).setDelay((0.1f * i) + 0.01f).setOnComplete(() => powerupButtonObjects[tempindex].SetActive(false));
            }
            // regular card usage
            if (i == index)
            {
                cardsInHand--;
                powerupButtonObjects[i].GetComponent<Button>().onClick.RemoveAllListeners();
                powerupButtonObjects[i].transform.localScale = new Vector3(0.9f, 0.9f, 0.9f);
                LeanTween.cancel(powerupButtonObjects[i]);
                LeanTween.scale(powerupButtonObjects[i], new Vector3(1f, 1f, 1f), 0.5f).setEase(LeanTweenType.easeOutElastic).setDelay(0.01f).setOnComplete(DisableMenuIfHandEmpty);
                LeanTween.scale(powerupButtonObjects[i], new Vector3(0f, 0f, 0f), 0.5f).setEase(LeanTweenType.easeOutElastic).setDelay(0.81f).setOnComplete(() => powerupButtonObjects[index].SetActive(false));
            }
        }
    }

    // this is so stupid lol
    private bool disablingMenuAlreadyInProgress = false;
    private void FinishedDisablingMenu() { disablingMenuAlreadyInProgress = false;  }
    private void DisableMenuIfHandEmpty()
    {
        // disable menu if we have no more cards in hand
        if (cardsInHand <= 0 && !disablingMenuAlreadyInProgress)
        {
            disablingMenuAlreadyInProgress = true;
            AlphaHelper(false);
            // tween out continue button
            LeanTween.moveLocalX(powerupButtonObjects[3], -startXLocalPos, 0.5f).setEase(LeanTweenType.easeInBack).setDelay(0.01f).setOnComplete(FinishedDisablingMenu);
        }
    }

    public void Continue()
    {
        for (int i = 0; i < powerupButtonObjects.Length; i++)
        {
            LeanTween.cancel(powerupButtonObjects[i]);
            powerupButtonObjects[i].GetComponent<Button>().onClick.RemoveAllListeners();
            var index = i;
            LeanTween.moveLocalX(powerupButtonObjects[i], -startXLocalPos, 0.5f).setEase(LeanTweenType.easeInBack).setDelay((0.1f * i) + 0.01f).setOnComplete(() => powerupButtonObjects[index].SetActive(false));
        }
        AlphaHelper(false);
        LeanTween.cancel(powerupButtonObjects[3]);
        LeanTween.moveLocalX(powerupButtonObjects[3], -startXLocalPos, 0.5f).setEase(LeanTweenType.easeInBack).setDelay(0.01f);
    }

    public void Reset()
    {
        cardsInHand = -1; // minus one because we include continue button
        powerupButtonObjects[3].SetActive(true); // make continue button active always
        for (int i = 0; i < powerupButtonObjects.Length; i++)
        {
            powerupButtonObjects[i].transform.localPosition = new Vector3(startXLocalPos, powerupButtonObjects[i].transform.localPosition.y, powerupButtonObjects[i].transform.localPosition.z);
            powerupButtonObjects[i].transform.localScale = new Vector3(1f, 1f, 1f);
        }
        powerupsMenu.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0f);
    }

    void AlphaHelper(bool fadeIn)
    {
        // Get the current color of the image
        Image img = powerupsMenu.GetComponent<Image>();
        Color currentColor = img.color;
        float targetAlpha = fadeIn ? 0.8f : 0f;

        // Use LeanTween to tween the alpha value
        LeanTween.value(gameObject, currentColor.a, targetAlpha, 1.0f)
            .setEase(fadeIn ? LeanTweenType.easeOutQuint : LeanTweenType.easeInExpo)
            .setOnUpdate((float alpha) =>
            {
                // Update the alpha of the image
                currentColor.a = alpha;
                img.color = currentColor;
            })
            .setDelay(0.01f)
            .setOnComplete(fadeIn ? null : DisableMenu);
    }

    void DisableMenu()
    {
        powerupsMenu.SetActive(false);
    }
}
