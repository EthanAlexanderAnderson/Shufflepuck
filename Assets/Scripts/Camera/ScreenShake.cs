using UnityEngine;

[RequireComponent(typeof(Camera))]
public class ScreenShake : MonoBehaviour
{
    // self
    public static ScreenShake Instance;

    private Vector3 _originalPosition;
    private float shakeMagnitude;

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
    }

    private void Update()
    {
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
}
