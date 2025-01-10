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
    [SerializeField] private GameObject skipButtonObject;
    private GameObject[] powerupButtonObjects;

    private float startXLocalPos = -1920;
    private float playerEndXLocalPos = 0;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(Instance);

        if (-(Screen.width * 1.2f) < startXLocalPos) { startXLocalPos = -(Screen.width * 1.2f); }
        powerupButtonObjects = new GameObject[] { powerupButtonObject1, powerupButtonObject2, powerupButtonObject3, skipButtonObject };
    }

    void OnEnable()
    {
        Reset();
        for (int i = 0; i < powerupButtonObjects.Length; i++)
        {
            LeanTween.cancel(powerupButtonObjects[i]);
            LeanTween.moveLocalX(powerupButtonObjects[i], playerEndXLocalPos, 1f).setEase(LeanTweenType.easeOutElastic).setDelay((0.2f * i) + 0.01f);
        }
        AlphaHelper(true);
    }

    public void UsePowerup(int index)
    {
        for (int i = 0; i < powerupButtonObjects.Length; i++)
        {
            powerupButtonObjects[i].GetComponent<Button>().onClick.RemoveAllListeners();
            if (i == index)
            {
                powerupButtonObjects[i].transform.localScale = new Vector3(0.9f, 0.9f, 0.9f);
                LeanTween.cancel(powerupButtonObjects[i]);
                LeanTween.scale(powerupButtonObjects[i], new Vector3(1f, 1f, 1f), 0.5f).setEase(LeanTweenType.easeOutElastic).setDelay(0.01f);
                LeanTween.scale(powerupButtonObjects[i], new Vector3(0f, 0f, 0f), 0.5f).setEase(LeanTweenType.easeOutElastic).setDelay(0.81f);
            }
            else
            {
                LeanTween.cancel(powerupButtonObjects[i]);
                LeanTween.moveLocalX(powerupButtonObjects[i], -startXLocalPos, 0.5f).setEase(LeanTweenType.easeInBack).setDelay((0.1f * i) + 0.01f);
            }
        }

        AlphaHelper(false);
    }

    public void Reset()
    {
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
        LeanTween.value(gameObject, currentColor.a, targetAlpha, 1.2f)
            .setEase(fadeIn ? LeanTweenType.easeOutQuint : LeanTweenType.easeInExpo)
            .setOnUpdate((float alpha) =>
            {
                // Update the alpha of the image
                currentColor.a = alpha;
                img.color = currentColor;
            })
            .setDelay(0.01f)
            .setOnComplete(fadeIn ? null : disableMenu);
    }

    void disableMenu()
    {
        powerupsMenu.SetActive(false);
    }
}
