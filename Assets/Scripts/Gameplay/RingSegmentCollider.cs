// generated using ChatGPT

using UnityEngine;

[RequireComponent(typeof(PolygonCollider2D))]
public class RingSegmentCollider : MonoBehaviour
{
    [Range(3, 100)] public int segments = 20; // Number of segments for the arc
    [Range(0f, 360f)] public float angle = 180f; // Angle of the arc in degrees
    public float outerRadius = 1f; // Outer radius of the arc
    public float innerRadius = 0.5f; // Inner radius of the arc (for the "hole")
    [Range(0f, 360f)] public float initialRotation = 0f; // Initial rotation of the arc in degrees
    public bool flipDirection = false; // Flip the arc direction (e.g., clockwise or counter-clockwise)

    private void Start()
    {
        GenerateRingSegmentCollider();
    }

    public void GenerateRingSegmentCollider()
    {
        PolygonCollider2D collider = GetComponent<PolygonCollider2D>();

        // Convert angles to radians
        float angleRad = angle * Mathf.Deg2Rad;
        float rotationRad = initialRotation * Mathf.Deg2Rad; // Initial rotation in radians
        float angleStep = angleRad / segments;

        // Total points = segments + 1 for the outer edge + segments + 1 for the inner edge
        Vector2[] points = new Vector2[(segments + 1) * 2];

        float startAngle = flipDirection ? Mathf.PI : 0; // Start angle: 180° if flipped, 0° otherwise
        startAngle += rotationRad; // Apply the initial rotation

        // Generate points for the outer edge
        for (int i = 0; i <= segments; i++)
        {
            float currentAngle = startAngle + i * angleStep;
            float x = Mathf.Cos(currentAngle) * outerRadius;
            float y = Mathf.Sin(currentAngle) * outerRadius;
            points[i] = new Vector2(x, y);
        }

        // Generate points for the inner edge (reverse order to close the loop)
        for (int i = 0; i <= segments; i++)
        {
            float currentAngle = startAngle + (segments - i) * angleStep;
            float x = Mathf.Cos(currentAngle) * innerRadius;
            float y = Mathf.Sin(currentAngle) * innerRadius;
            points[segments + 1 + i] = new Vector2(x, y);
        }

        // Assign the points to the collider
        collider.pathCount = 1;
        collider.SetPath(0, points);
    }

    // Optionally, update in real-time in the editor
    private void OnValidate()
    {
        //if (Application.isPlaying)
        //{
            GenerateRingSegmentCollider();
        //}
    }
}
