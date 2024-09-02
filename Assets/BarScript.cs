// Script to control the angle/power/spin bar

using UnityEngine;

public class BarScript : MonoBehaviour
{
    public SpriteRenderer spriteRenderer;
    public Sprite angleBarLeft;
    public Sprite angleBarRight;

    public Sprite powerBar;
    public Sprite spinBar;

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

    public void toggleDim(bool dim)
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
