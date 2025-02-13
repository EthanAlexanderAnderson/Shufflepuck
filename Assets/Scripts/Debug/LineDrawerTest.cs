// Draw line outlines around colliders. This script is partially based on the code from the link below (and also AI code)
// https://stackoverflow.com/questions/39153239/how-to-make-a-highlight-around-a-polygoncollider2d-in-unity

using UnityEngine;
public class LineDrawerTest : MonoBehaviour
{
    private PolygonCollider2D pCollider;
    private BoxCollider2D bCollider;
    private CircleCollider2D cCollider;

    protected void Start()
    {
        pCollider = GetComponent<PolygonCollider2D>();
        bCollider = GetComponent<BoxCollider2D>();
        cCollider = GetComponent<CircleCollider2D>();

        if (bCollider != null)
        {
            HighlightAroundCollider(bCollider, Color.green, Color.green, 0.1f);
        }
        else if (pCollider != null)
        {
            HighlightAroundCollider(pCollider, Color.green, Color.green, 0.1f);
        }
        else if (cCollider != null)
        {
            HighlightAroundCollider(cCollider, Color.green, Color.green, 0.1f);
        }
    }

    void HighlightAroundCollider(Component cpType, Color beginColor, Color endColor, float highlightSize = 0.3f)
    {
        // 1. Create new Line Renderer
        LineRenderer lineRenderer = gameObject.GetComponent<LineRenderer>();
        if (lineRenderer == null)
        {
            lineRenderer = cpType.gameObject.AddComponent<LineRenderer>();
        }

        // Set color and width
        lineRenderer.startColor = beginColor;
        lineRenderer.endColor = endColor;
        lineRenderer.startWidth = highlightSize;
        lineRenderer.endWidth = highlightSize;
        lineRenderer.loop = false;

        float zPos = 10f; // Since this is 2D, make sure it is in front

        if (cpType is PolygonCollider2D polygonCollider)
        {
            // Handle PolygonCollider2D
            Vector2[] points = polygonCollider.points;
            Vector3[] worldPoints = new Vector3[points.Length * 2 + 1];

            for (int i = 0; i < points.Length; i++)
            {
                worldPoints[i] = polygonCollider.transform.TransformPoint(points[i]);
            }

            worldPoints[points.Length] = worldPoints[0]; // Ensure closure

            for (int i = 0; i < points.Length; i++)
            {
                worldPoints[points.Length + 1 + i] = worldPoints[points.Length - i];
            }

            lineRenderer.positionCount = worldPoints.Length;
            for (int i = 0; i < worldPoints.Length; i++)
            {
                Vector3 finalLine = worldPoints[i];
                finalLine.z = zPos;
                lineRenderer.SetPosition(i, finalLine);
            }
        }
        else if (cpType is BoxCollider2D boxCollider)
        {
            // Handle BoxCollider2D
            Vector2 size = boxCollider.size;
            Vector2[] corners = new Vector2[4]
            {
                new Vector2(-size.x / 2, -size.y / 2), // Bottom-left
                new Vector2(-size.x / 2, size.y / 2),  // Top-left
                new Vector2(size.x / 2, size.y / 2),   // Top-right
                new Vector2(size.x / 2, -size.y / 2)   // Bottom-right
            };

            for (int i = 0; i < corners.Length; i++)
            {
                corners[i] = boxCollider.transform.TransformPoint(corners[i]);
            }

            lineRenderer.positionCount = corners.Length + 1;
            for (int i = 0; i < corners.Length; i++)
            {
                Vector3 finalLine = corners[i];
                finalLine.z = zPos;
                lineRenderer.SetPosition(i, finalLine);
            }
            lineRenderer.SetPosition(corners.Length, lineRenderer.GetPosition(0)); // Close the box
        }
        else if (cpType is CircleCollider2D circleCollider)
        {
            // Handle CircleCollider2D
            int segments = 36;
            Vector3[] circlePoints = new Vector3[segments + 1];
            float radius = circleCollider.radius * circleCollider.transform.lossyScale.x;
            Vector3 center = circleCollider.transform.TransformPoint(circleCollider.offset);

            for (int i = 0; i <= segments; i++)
            {
                float angle = i * (2 * Mathf.PI / segments);
                circlePoints[i] = new Vector3(
                    center.x + Mathf.Cos(angle) * radius,
                    center.y + Mathf.Sin(angle) * radius,
                    zPos
                );
            }

            lineRenderer.positionCount = circlePoints.Length;
            lineRenderer.SetPositions(circlePoints);
        }
    }
}