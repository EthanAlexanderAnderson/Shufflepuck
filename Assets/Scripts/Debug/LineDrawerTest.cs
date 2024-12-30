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
            highlightAroundCollider(bCollider, Color.green, Color.green, 0.1f);
        }
        else if (pCollider != null)
        {
            highlightAroundCollider(pCollider, Color.green, Color.green, 0.1f);
        }
        else if (cCollider != null)
        {
            //highlightAroundCollider(cCollider, Color.green, Color.green, 0.1f);
        }

    }
    void highlightAroundCollider(Component cpType, Color beginColor, Color endColor, float hightlightSize = 0.3f)
    {
        //1. Create new Line Renderer
        LineRenderer lineRenderer = gameObject.GetComponent<LineRenderer>();
        if (lineRenderer == null)
        {
            lineRenderer = cpType.gameObject.AddComponent<LineRenderer>();
        }

        //2. Assign Material to the new Line Renderer
        //lineRenderer.material = new Material(Shader.Find("Particles/Additive"));

        float zPos = 10f;//Since this is 2D. Make sure it is in the front

        //Set color and width
        lineRenderer.startColor = beginColor;
        lineRenderer.endColor = endColor;
        lineRenderer.startWidth = hightlightSize;
        lineRenderer.endWidth = hightlightSize;

        if (cpType is PolygonCollider2D)
        {
            //3. Get the points from the PolygonCollider2D
            Vector2[] pColliderPos = (cpType as PolygonCollider2D).points;

            //4. Convert local to world points
            for (int i = 0; i < pColliderPos.Length; i++)
            {
                pColliderPos[i] = cpType.transform.TransformPoint(pColliderPos[i]);
            }

            //5. Set the SetVertexCount of the LineRenderer to the Length of the points
            lineRenderer.positionCount = pColliderPos.Length + 1;
            for (int i = 0; i < pColliderPos.Length; i++)
            {
                //6. Draw the  line
                Vector3 finalLine = pColliderPos[i];
                finalLine.z = zPos;
                lineRenderer.SetPosition(i, finalLine);

                //7. Check if this is the last loop. Now Close the Line drawn
                if (i == (pColliderPos.Length - 1))
                {
                    finalLine = pColliderPos[0];
                    finalLine.z = zPos;
                    lineRenderer.SetPosition(pColliderPos.Length, finalLine);
                }
            }
        }
        else
        {
            //3. Get the points from the BoxCollider2D
            Bounds boxBounds = (cpType as BoxCollider2D).bounds;

            Vector2 topLeft = new Vector2(boxBounds.center.x - boxBounds.extents.x, boxBounds.center.y + boxBounds.extents.y);
            Vector2 topRight = new Vector2(boxBounds.center.x + boxBounds.extents.x, boxBounds.center.y + boxBounds.extents.y);
            Vector2 bottomLeft = new Vector2(boxBounds.center.x - boxBounds.extents.x, boxBounds.center.y - boxBounds.extents.y);
            Vector2 bottomRight = new Vector2(boxBounds.center.x + boxBounds.extents.x, boxBounds.center.y - boxBounds.extents.y);

            Vector2[] pColliderPos = { topLeft, topRight, bottomRight, bottomLeft };

            //5. Set the SetVertexCount of the LineRenderer to the Length of the points
            lineRenderer.positionCount = pColliderPos.Length + 1;
            for (int i = 0; i < pColliderPos.Length; i++)
            {
                //6. Draw the  line
                Vector3 finalLine = pColliderPos[i];
                finalLine.z = zPos;
                lineRenderer.SetPosition(i, finalLine);

                //7. Check if this is the last loop. Now Close the Line drawn
                if (i == (pColliderPos.Length - 1))
                {
                    finalLine = pColliderPos[0];
                    finalLine.z = zPos;
                    lineRenderer.SetPosition(pColliderPos.Length, finalLine);
                }
            }
        }
    }
#endif
}