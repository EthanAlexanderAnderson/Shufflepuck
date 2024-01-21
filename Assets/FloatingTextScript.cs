using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class FloatingTextScript : MonoBehaviour
{
    public float destroyTime = 3f;
    public Vector3 offset = new Vector3(0, -20, 0);

    // Start is called before the first frame update
    void Start()
    {
        TextMeshPro textmeshPro = GetComponent<TextMeshPro>();
        textmeshPro.outlineWidth = 0.2f;
        textmeshPro.outlineColor = new Color32(0, 0, 0, 255);
        Destroy(gameObject, destroyTime);
        transform.localPosition += offset;
    }

    private float expo;
    void FixedUpdate()
    {
        transform.rotation = Quaternion.identity;
        transform.position += transform.up * 0.05f;
        if (expo < 0.9)
        {
            transform.localScale *= 0.99f - expo;
            expo += 0.005f;
        }
    }
}
