using TMPro;
using UnityEngine;

public class FloatingTextScript : MonoBehaviour
{
    [SerializeField] TMP_Text TMPtext;

    public float destroyTime; // 3

    [SerializeField] private float speedUp; // 0.05
    [SerializeField] private float speedShrink; // 0.005f;
    [SerializeField] private float speedFade; // 0.005

    // Start is called before the first frame update
    void Start()
    {
        TextMeshPro textmeshPro = GetComponent<TextMeshPro>();
        textmeshPro.outlineWidth = 0.2f;
        textmeshPro.outlineColor = new Color32(0, 0, 0, 255);
        Destroy(gameObject, destroyTime);
    }

    private float expoShrink;
    private float expoFade;
    void FixedUpdate()
    {
        // move up and shrink
        transform.position += transform.up * speedUp;
        transform.localScale *= 1f - expoShrink;

        if (expoShrink < 0.99)
        {
            expoShrink += speedShrink;
        }
        TMPtext.color = new Color(TMPtext.color.r, TMPtext.color.g, TMPtext.color.b, 1 - expoFade);
        if (expoFade < 0.99)
        {
            expoFade += speedFade + expoFade/10; // +expofade (make speedfade less)
        }
    }

    private void Update()
    {
        transform.position = new Vector3(transform.parent.position.x, transform.position.y, transform.position.z);
        transform.rotation = Quaternion.identity;
    }

    public void Initialize(string text)
    {
        TMPtext.text = text;
    }

    public void Initialize(string text, float rate, float scale = 1)
    {
        TMPtext.text = text;
        speedUp *= rate;
        speedShrink *= rate;
        transform.localScale = new Vector3(scale, scale, scale);
    }

    public void Initialize(string text, float speedUpRate, float speedShrinkRate, float speedFadeRate, float scale = 1)
    {
        TMPtext.text = text;
        speedUp *= speedUpRate;
        speedShrink *= speedShrinkRate;
        speedFade *= speedFadeRate;
        transform.localScale = new Vector3(scale, scale, scale);
    }
}
