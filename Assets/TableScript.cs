using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TableScript : MonoBehaviour
{
    public SpriteRenderer spriteRenderer;

    public Sprite title;
    public Sprite board;

    // Start is called before the first frame update
    void Start()
    {
        spriteRenderer.sprite = title;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ShowBoard()
    {
        spriteRenderer.sprite = board;
    }
}
