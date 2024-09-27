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
                spriteRenderer.enabled = true;
                spriteRenderer.sprite = (isPlayer ? angleBarLeft : angleBarRight);
                barText.text = "angle";
                barTextCanvas.SetActive(true);
                break;
            case "power":
                spriteRenderer.enabled = true;
                var lineValue = line.GetValue();
                var notPlayerAngleModifier = (isPlayer ? 0 : 27);
                spriteRenderer.sprite = (lineValue > (50 - notPlayerAngleModifier) && lineValue < (77 - notPlayerAngleModifier) ? powerBarMid : powerBarOut);
                barText.text = "power";
                barTextCanvas.SetActive(true);
                break;
            case "spin":
                spriteRenderer.enabled = true;
                spriteRenderer.sprite = spinBar;
                barText.text = "spin";
                barTextCanvas.SetActive(true);
                break;
            case "none":
                spriteRenderer.enabled = false;
                barText.text = "";
                barTextCanvas.SetActive(false);
                break;
            default:
                spriteRenderer.enabled = false;
                barText.text = "";
                barTextCanvas.SetActive(false);
                break;
        }
        return type;
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
