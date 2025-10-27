using TMPro;
using UnityEngine;

public class FloatingTextScript : MonoBehaviour
{
    [SerializeField] TMP_Text TMPtext;

    public float destroyTime; // 3

    [SerializeField] private float speedUp; // 0.05
    [SerializeField] private float speedShrink; // 0.005f;
    [SerializeField] private float speedFade; // 0.005
    [SerializeField] private bool shouldFollowParent;

    private Vector2 initialPosition;
    private Vector2 initialPositionOffset;

    // Start is called before the first frame update
    void Start()
    {
        TextMeshPro textmeshPro = GetComponent<TextMeshPro>();
        textmeshPro.outlineWidth = 0.2f;
        textmeshPro.outlineColor = new Color32(0, 0, 0, 255);
        initialPosition = transform.position;
    }

    private float expoShrink;
    private float expoFade;
    private Vector2 positionSlideUpOffset;
    void FixedUpdate()
    {
        // move up and shrink
        positionSlideUpOffset += Vector2.up * speedUp;
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

    // im not sure why, but this has to appear in Update instead of FixedUpdate, so that the text doesn't appear slanted
    private void Update()
    {
        // set position
        if (shouldFollowParent)
        {
            transform.position = new Vector2(transform.parent.position.x, transform.parent.position.y) + initialPositionOffset + positionSlideUpOffset;
            transform.rotation = Quaternion.identity;
        }
        else
        {
            transform.position = initialPosition + initialPositionOffset + positionSlideUpOffset;
        }
    }

    public void Initialize(string text)
    {
        TMPtext.text = text;
        Destroy(gameObject, destroyTime);
    }

    public void Initialize(string text, float rate, float scale = 1)
    {
        TMPtext.text = text;
        speedUp *= rate;
        speedShrink *= rate;
        transform.localScale = new Vector3(scale, scale, scale);
        Destroy(gameObject, destroyTime);
    }

    public void Initialize(string text, float speedUpRate, float speedShrinkRate, float speedFadeRate, Vector2 positionOffset, float scale = 1, bool textShouldFollowParent = false, float destroyTime = 3)
    {
        // regular
        if (PlayerPrefs.GetInt("debug") != 1 || text == "nope!")
        {
            TMPtext.text = text;
            speedUp *= speedUpRate;
            speedShrink *= speedShrinkRate;
            speedFade *= speedFadeRate;
            initialPositionOffset = positionOffset;
            transform.localScale = new Vector3(scale, scale, scale);
            shouldFollowParent = textShouldFollowParent;
            Destroy(gameObject, destroyTime);
        }
        // debug
        else
        {

            TMPtext.text = text;
            speedUp = 0;
            speedShrink = 0;
            speedFade = 0;
            initialPositionOffset = positionOffset;
            transform.localScale = new Vector3(scale, scale, scale);
            shouldFollowParent = textShouldFollowParent;
        }
    }

    public void SetText(string text)
    {
        TMPtext.text = text;
    }
}
