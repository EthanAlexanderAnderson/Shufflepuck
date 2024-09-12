// Script to control the angle/power/spin bar

using UnityEngine;

public class BarScript : MonoBehaviour
{
    // self
    public static BarScript Instance;

    [SerializeField] private SpriteRenderer spriteRenderer;
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
                break;
            case "power":
                spriteRenderer.enabled = true;
                spriteRenderer.sprite = powerBar;
                break;
            case "spin":
                spriteRenderer.enabled = true;
                spriteRenderer.sprite = spinBar;
                break;
            case "none":
                spriteRenderer.enabled = false;
                break;
            default:
                spriteRenderer.enabled = false;
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
