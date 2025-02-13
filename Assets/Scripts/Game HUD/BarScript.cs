// Script to control the angle/power/spin bar

using UnityEngine;
using TMPro;

public class BarScript : MonoBehaviour
{
    // self
    public static BarScript Instance;

    // dependancies
    private LineScript line;

    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private TMP_Text barText;
    [SerializeField] private GameObject barTextCanvas;
    [SerializeField] private Sprite angleBarLeft;
    [SerializeField] private Sprite angleBarRight;
    [SerializeField] private Sprite powerBarMid;
    [SerializeField] private Sprite powerBarOut;
    [SerializeField] private Sprite spinBar;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(Instance);
    }

    // Start is called before the first frame update
    void Start()
    {
        spriteRenderer.enabled = false;
        line = LineScript.Instance;
    }

    // change bar sprite
    public string ChangeBar(string type, bool isPlayer = true)
    {
        switch (type)
        {
            case "angle":
                // animation
                transform.localScale = new Vector3(0.95f, 1f, 1f);
                transform.localPosition = new Vector3(transform.localPosition.x, -5, transform.localPosition.z);
                LeanTween.cancel(gameObject);
                LeanTween.moveLocalY(gameObject, 0, 1f).setEase(LeanTweenType.easeOutElastic).setDelay(0.1f);
                // set sprite
                spriteRenderer.enabled = true;
                spriteRenderer.sprite = (isPlayer ? angleBarLeft : angleBarRight);
                // set text
                barText.text = "angle";
                barTextCanvas.SetActive(true);
                break;
            case "power":
                // animation
                transform.localScale = new Vector3(0.8f, 0.8f, 0.8f);
                transform.localPosition = new Vector3(transform.localPosition.x, 0, transform.localPosition.z);
                LeanTween.cancel(gameObject);
                LeanTween.scale(gameObject, new Vector3(0.95f, 1f, 1f), 1f).setEase(LeanTweenType.easeOutElastic).setDelay(0.01f);
                // set sprite
                spriteRenderer.enabled = true;
                var lineValue = line.GetValue();
                var notPlayerAngleModifier = (isPlayer ? 0 : 27);
                spriteRenderer.sprite = (lineValue > (50 - notPlayerAngleModifier) && lineValue < (77 - notPlayerAngleModifier) ? powerBarMid : powerBarOut);
                // set text
                barText.text = "power";
                barTextCanvas.SetActive(true);
                break;
            case "spin":
                // animation
                transform.localScale = new Vector3(0.8f, 0.8f, 0.8f);
                transform.localPosition = new Vector3(transform.localPosition.x, 0, transform.localPosition.z);
                LeanTween.cancel(gameObject);
                LeanTween.scale(gameObject, new Vector3(0.95f, 1f, 1f), 1f).setEase(LeanTweenType.easeOutElastic).setDelay(0.01f);
                // set sprite
                spriteRenderer.enabled = true;
                spriteRenderer.sprite = spinBar;
                // set text
                barText.text = "spin";
                barTextCanvas.SetActive(true);
                break;
            case "none":
                // animation
                transform.localScale = new Vector3(0.8f, 0.8f, 0.8f);
                LeanTween.cancel(gameObject);
                LeanTween.scale(gameObject, new Vector3(0.95f, 1f, 1f), 1f).setEase(LeanTweenType.easeOutElastic).setDelay(0.01f);
                LeanTween.moveLocalY(gameObject, -5, 1f).setEase(LeanTweenType.easeInElastic).setDelay(0.51f).setOnComplete(DisableBarImageAndText);
                break;
            default:
                DisableBarImageAndText();
                break;
        }
        return type;
    }

    private void DisableBarImageAndText()
    {
        spriteRenderer.enabled = false;
        barText.text = "";
        barTextCanvas.SetActive(false);
    }

    public void ToggleDim(bool dim)
    {
        if (dim)
        {
            spriteRenderer.color = new Color(1, 1, 1, 0.5f);
        }
        else
        {
            spriteRenderer.color = new Color(1, 1, 1, 1);
        }
    }
}
