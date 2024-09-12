// Script to control the angle/power/spin bar

using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BarScript : MonoBehaviour
{
    // self
    public static BarScript Instance;

    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private TMP_Text barText;
    [SerializeField] private Sprite angleBarLeft;
    [SerializeField] private Sprite angleBarRight;
    [SerializeField] private Sprite powerBar;
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
    }

    // change bar sprite
    public string ChangeBar(string type, bool isLeftside = true)
    {
        switch (type)
        {
            case "angle":
                spriteRenderer.enabled = true;
                spriteRenderer.sprite = (isLeftside ? angleBarLeft : angleBarRight);
                barText.text = "angle";
                break;
            case "power":
                spriteRenderer.enabled = true;
                spriteRenderer.sprite = powerBar;
                barText.text = "power";
                break;
            case "spin":
                spriteRenderer.enabled = true;
                spriteRenderer.sprite = spinBar;
                barText.text = "spin";
                break;
            case "none":
                spriteRenderer.enabled = false;
                barText.text = "";
                break;
            default:
                spriteRenderer.enabled = false;
                barText.text = "";
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
