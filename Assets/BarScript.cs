using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BarScript : MonoBehaviour
{
    public SpriteRenderer spriteRenderer;
    public Sprite angleBar;
    public Sprite powerBar;
    public Sprite spinBar;

    // Start is called before the first frame update
    void Start()
    {
        spriteRenderer.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void changeBar(string type)
    {
        switch (type)
        {
            case "angle":
                spriteRenderer.enabled = true;
                spriteRenderer.sprite = angleBar;
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
    }
}
