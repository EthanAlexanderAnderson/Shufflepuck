using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Camera))]
public class ScreenShake : MonoBehaviour
{
    // self
    public static ScreenShake Instance;

    private Vector3 _originalPosition;
    private float shakeMagnitude;
    private bool shakeEnabled;
    [SerializeField] private GameObject screenShakeToggle;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(Instance);
    }

    private void Start()
    {
        _originalPosition = transform.position;
        shakeEnabled = PlayerPrefs.GetInt("ScreenShakeEnabled", 1) == 1;
        screenShakeToggle.GetComponent<Toggle>().isOn = shakeEnabled;
    }

    private void FixedUpdate()
    {
        if (!shakeEnabled) { return; }
        float x = Random.Range(-1f, 1f) * shakeMagnitude;
        float y = Random.Range(-1f, 1f) * shakeMagnitude;

        transform.position = new Vector3(_originalPosition.x + x, _originalPosition.y + y, _originalPosition.z);

        shakeMagnitude *= 0.9f;

        if (transform.position != _originalPosition && shakeMagnitude <= 0)
        {
            transform.position = _originalPosition;
        }
    }

    public void Shake(float magnitude)
    {
        shakeMagnitude = magnitude;
    }

    public void ToggleShake(bool enabled)
    {
        shakeEnabled = enabled;
        PlayerPrefs.SetInt("ScreenShakeEnabled", enabled ? 1 : 0);
    }
}
