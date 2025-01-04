// This is only used with CPUPathScript to help me create CPU paths

using UnityEngine;

// https://stackoverflow.com/questions/39153239/how-to-make-a-highlight-around-a-polygoncollider2d-in-unity
public class LineDrawerTest : MonoBehaviour
{
#if (UNITY_EDITOR)
    //public GameObject myGameObject;
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
            //highlightAroundCollider(cCollider, Color.green, Color.green, 0.1f);
        }

    }
    void HighlightAroundCollider(Component cpType, Color beginColor, Color endColor, float hightlightSize = 0.3f)
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
        lineRenderer.startWidth = hightlightSize;
        lineRenderer.endWidth = hightlightSize;

        float zPos = 10f; // Since this is 2D, make sure it is in front

        if (cpType is PolygonCollider2D)
        {
            // Handle PolygonCollider2D
            Vector2[] pColliderPos = (cpType as PolygonCollider2D).points;

            for (int i = 0; i < pColliderPos.Length; i++)
            {
                pColliderPos[i] = cpType.transform.TransformPoint(pColliderPos[i]);
            }

            lineRenderer.positionCount = pColliderPos.Length + 1;
            for (int i = 0; i < pColliderPos.Length; i++)
            {
                Vector3 finalLine = pColliderPos[i];
                finalLine.z = zPos;
                lineRenderer.SetPosition(i, finalLine);

                if (i == (pColliderPos.Length - 1))
                {
                    finalLine = pColliderPos[0];
                    finalLine.z = zPos;
                    lineRenderer.SetPosition(pColliderPos.Length, finalLine);
                }
            }
        }
        else if (cpType is BoxCollider2D)
        {
            // Handle BoxCollider2D
            BoxCollider2D boxCollider = cpType as BoxCollider2D;
            Vector2[] corners = new Vector2[4];

            // Calculate the corners of the BoxCollider2D in local space
            Vector2 size = boxCollider.size;
            corners[0] = new Vector2(-size.x / 2, -size.y / 2); // Bottom-left
            corners[1] = new Vector2(-size.x / 2, size.y / 2);  // Top-left
            corners[2] = new Vector2(size.x / 2, size.y / 2);   // Top-right
            corners[3] = new Vector2(size.x / 2, -size.y / 2);  // Bottom-right

            // Transform corners to world space
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

                if (i == corners.Length - 1)
                {
                    finalLine = corners[0];
                    finalLine.z = zPos;
                    lineRenderer.SetPosition(corners.Length, finalLine);
                }
            }
        }
    }
#endif
}