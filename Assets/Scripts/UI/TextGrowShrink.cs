using UnityEngine;
using TMPro;

public class TextGrowShrink : MonoBehaviour
{
    [SerializeField] private float maxScale = 1.2f; // The maximum scale during the animation
    [SerializeField] private float shrinkRate = 0.9f; // multiplies by current scale every frame

    private Vector3 originalScale;
    [SerializeField] private TMP_Text textMeshPro;

    void Awake()
    {
        textMeshPro = GetComponent<TMP_Text>();
        if (textMeshPro == null)
        {
            Debug.LogError("TextGrowShrink script requires a TextMeshPro component on the same GameObject.");
            enabled = false;
            return;
        }

        originalScale = transform.localScale;
        textMeshPro.RegisterDirtyVerticesCallback(OnTextChanged);
    }
    private void OnDestroy()
    {
        if (textMeshPro != null)
        {
            textMeshPro.UnregisterDirtyVerticesCallback(OnTextChanged);
        }
    }

    private void OnTextChanged()
    {
        transform.localScale = originalScale * maxScale;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (transform.localScale.x > originalScale.x)
        {
            transform.localScale = transform.localScale * shrinkRate;
        }
    }
}
