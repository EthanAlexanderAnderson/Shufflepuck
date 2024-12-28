using UnityEngine;
using UnityEngine.UI;

public class BreathingGlow : MonoBehaviour
{
    [SerializeField] private Image glowImage; // Reference to the glow image
    [SerializeField] private float pulseSpeed = 1f; // Speed of the pulsating effect
    [SerializeField] private float minOpacity = 0.2f; // Minimum opacity (alpha value)
    [SerializeField] private float maxOpacity = 1f;  // Maximum opacity (alpha value)

    private Color originalColor;

    void Start()
    {
        // Store the original color of the glow image
        originalColor = glowImage.color;
    }

    void FixedUpdate()
    {
        // Calculate the current alpha value using a sine wave
        float alpha = Mathf.Lerp(minOpacity, maxOpacity, (Mathf.Sin(Time.time * pulseSpeed) + 1) / 2f);

        // Apply the alpha value to the glow image's color
        glowImage.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
    }
}